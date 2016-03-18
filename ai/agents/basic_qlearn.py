
import hermes
from gamestate import GameState
from agent import Agent
from actions import Actions
import struct 
import random
import sys
import signal
import cPickle as pickle
from plotter import Plotter
import socket

HOST = '127.0.0.1'


class BasicQlearnAgent(Agent):

	def __init__(self,isPlayer1=False, loadOldTable=False, overwriteFile = False, 
				 epsilon=.20, alpha=0.5, name="Qlearner", plot_freq=0, trainingMode=False):

		super(BasicQlearnAgent, self).__init__(name)
		self.p1 = isPlayer1

		self.epsilon = epsilon

		self.discount = 0.90

		# queue of previous states 
		self.prevStates = []
		self.counter = 0

		# high 
		self.alpha = 0.1
		self.prevGamestate = GameState(-4,0,100,4,0,100)
		self.prevAction = Actions.doNothing
		self.bodywidth = 1.0;
		self.isTraining = False
		self.possibleXdists = [0, 1, 2, 3, 4, 5, 6]

		if (overwriteFile):
			signal.signal(signal.SIGINT, self.signalHandler)
		self.actions = [a for a in Actions]
		self.numActions = len(self.actions)
		self.actionDic = dict()
		self.makeActionDic()

		# sparse feature extractor
		self.w = {}

		self.fn = 'savedWeightsp1.txt'
		self.bindport = 5555
		if (not self.p1):
			self.fn = 'savedWeightsp2.txt'
			self.bindport=5565

		self.ipc_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)    
		self.ipc_server.bind((HOST, self.bindport))
    
		self.ipc_client =  socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
		
		if (not loadOldTable):
			self.initW()
			self.epsilon = .1
		else:
			self.retrieveWeightsFromFile(self.fn)

		# times per second to plot
		# 0 is 'off', but we add modulo plot_freq + 1, and update
		# when counter = 1
		self.plot_freq = (60/(plot_freq + 1))
		self.plot_counter = 0

		if plot_freq > 0:
			self.plotter = Plotter()

		print("Done initializing!")

	def signalHandler(self,err,ernum):
		print("dumping to file!")
		self.dumpWeightsToFile(self.fn)
		sys.exit(0)

	def makeActionDic(self):
		for i in range(0,self.numActions):
			self.actionDic[self.actions[i]] = i

	def initW(self):
		''' Initialize w to random weights '''
		for elem in self.featureExtractor ((0, 0, 0)):
			self.w[elem] = 0.01 * random.random()


	# def initializeTable(self):
	# 	for xdist in self.possibleXdists:
	# 		for ydist in [-1.0, .5, 0, .5, 1.0]:
	# 			for p2flags in [0b00000000,0b10010000,0b00010000, 0b01000000,0b10100000,0b00100000]:
	# 				for p1flags in [0b0000,0b1001,0b0001, 0b0010,0b1010,0b0100]:
	# 					Gamestate = (xdist,ydist,p1flags|p2flags)
	# 					self.Qtable[Gamestate] = [0]*self.numActions
	# 					if (abs(xdist)>2):
	# 						self.Qtable[Gamestate][self.actionDic[Actions.walkTowards]]+=.5
	# 						self.Qtable[Gamestate][self.actionDic[Actions.runTowards]]+=.5

	# TODO: 
	# Temporal information
	# 
	def getGSTuple(self,gamestate):
		xdist = abs(int(gamestate.p1Xpos-gamestate.p2Xpos-self.bodywidth))

		tupXdist =xdist
		if(xdist>5):
			tupXdist = 6

		ydist = int(gamestate.p1Ypos-gamestate.p2Ypos)
		tupYdist =ydist
		if (ydist<-.5):
			tupYdist = -1
		elif(ydist>.5):
			tupYdist = 1

		return (tupXdist,tupYdist,gamestate.actionFlags)


	def featureExtractor (self, gsTuple):
		''' 
		Extracts a sparse feature vector phi from the gamestate.

		'''
		tup = gsTuple
		features = {}
		features['x'], features['y'] = tup[0], tup[1]
		features['1/x'], features['1/y'] = 1.0 / (abs(tup[0]) + 1.0), (1.0/ (abs(tup[1]) + 1.0))

		# indicator feature for each action
		actionFlags = tup[2]

		for elem in self.actions:
			if actionFlags & elem.value:
				features[elem.name] = 1
			else:
				features[elem.name] = 0

		return features

	def dot(self, w, x):
		''' fast and dirty dot product '''
		total = 0.0
		for elem in x:
			total += x[elem] * \
				w[elem]
		return total


	def allActionMasks(self):
		''' Generator for all the action flags '''
		for p2flags in [0b00000000,0b10010000,0b00010000, 0b01000000,0b10100000,0b00100000]:
			for p1flags in [0b0000,0b1001,0b0001, 0b0010,0b1010,0b0100]: 
				yield p1flags | p2flags


	def getQRow(self,gamestate):	
		tup = self.getGSTuple(gamestate)
		row = [(tup[0], tup[1], action.value) for action in self.actions ]
		''' weights version '''
		values = [ self.dot(self.w, self.featureExtractor(cell)) for cell in row ]
		
		return values
		#return self.Qtable[tup]

	def updateQForStateAction(self,prevGS,curGS,prevAction,reward):
		
		prevtup = self.getGSTuple(prevGS)
		index = self.actionDic[prevAction]

		x = self.featureExtractor(prevtup)

		curqRow = self.getQRow(curGS)
		maxCurQ = max(curqRow)
		prevQ = self.dot(self.w, x)

		r = reward + (self.discount * maxCurQ) - prevQ

		# Gradient descent
		for feat in x:
			self.w[feat] += (self.alpha	 * r * x[feat])

		# self.Qtable[prevtup][index]+= self.alpha*(reward + maxCurQ - self.Qtable[prevtup][index])

	def ingestState(self, data):
		args = struct.unpack('!ffffffB',data)
		gameState = GameState(args[0],args[1],args[2],args[3],args[4],args[5])
		gameState.parseFlags(args[6])

		p1damage = self.prevGamestate.p1Health - gameState.p1Health;
		p2damage = self.prevGamestate.p2Health - gameState.p2Health;

		p1win = gameState.p2Health ==0
		p2win = gameState.p1Health ==0

		if (p1win):
			p2damage = 250
			
		if (p2win):
			p1damage = 250

		p1damageval = p1damage/100

		p2damageval = p2damage/100

		reward = -0.01
		if (p1damage and (p1damage>0)):#end of game, don't count negative

			if (self.p1):
				reward -= p1damageval
				# self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,-1*p1damageval)
			else:
				reward += p1damageval
				# self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,p1damageval)
			self.epsilon -= 0.0002

		if (p2damage and (p2damage>0)):
			if (self.p1):
				reward += p2damageval
				#self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,p2damageval)
			else:
				reward -= p2damageval
				#self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,-1*p2damageval)
			self.epsilon -= 0.0002

		# Penalize attacking 
		if (gameState.p1Attacking and self.p1) or \
			gameState.p2Attacking and not self.p1:
			reward -= 0.01

		# Set temporal counter to sparsely track previous states
		self.counter += 1
		if self.counter % 50 == 0:
			# Add state
			self.prevStates.insert(0, gameState)
			if len(self.prevStates) > 10:
				# remove old 
				self.prevStates.pop()
			r = reward
			for elem in self.prevStates:
				r *= (self.discount * self.discount)
				self.updateQForStateAction(self.prevGamestate, elem, self.prevAction, r)

		self.updateQForStateAction(self.prevGamestate, gameState, self.prevAction, reward)

		self.prevGamestate = gameState


		if (self.isTraining):
			if (p1win):
				self.prevGamestate = GameState(-4,0,100,4,0,100)
				if (self.p1):
					dumpWeightsToFile('geneticQtableFile.txt') #dump to mutual file
					data = bytearray()
					data.append(255)
					print("sending ipc")
					self.ipc_client.sendto(data, (HOST, self.bindport+10))	#let other player know file is ready
				else:
					data, addr = self.ipc_server.recvfrom(1) 
					print('retrieved ipc')
					retrieveWeightsFromFile('geneticQtableFile.txt')

			if (p2win):
				self.prevGamestate = GameState(-4,0,100,4,0,100)
				if (not self.p1):
					dumpWeightsToFile('geneticQtableFile.txt') #dump to mutual file
					data = bytearray()
					data.append(255)
					self.ipc_client.sendto(data, (HOST, self.bindport-10))	#let other player know file is ready
				else:
					data, addr = self.ipc_server.recvfrom(1) 
					print('got ipc')
					retrieveWeightsFromFile('geneticQtableFile.txt')


	def chooseAction(self):
		qRow = self.getQRow(self.prevGamestate)
		print "row: ", qRow
		maxQ = max(qRow)
		# update plot according to frequency
		self.plot_counter = (self.plot_counter + 1) % (self.plot_freq + 1)

		if self.plot_counter == 1 and hasattr(self, 'plotter'):
			self.plotter.updateGraph(qRow)

		count = sum([1 for elem in qRow if elem > (0.99 * maxQ)])
		action = None
	 	
		if random.random() < self.epsilon: # exploration 
			print("exploring!!!!!")
			action = random.choice(self.actions) 

		elif count > 1:

			best = [i for i in xrange(len(self.actions)) if qRow[i] >= (0.99 * maxQ) ]
			print("randomly chose an action out of best possible")
			print "best: ", best

			action = self.actions[random.choice(best)]
		else:
			print("maxq = ",maxQ)
			action = self.actions[qRow.index(maxQ)]
	 
	 	if (self.xdist(self.prevGamestate)<=2.0 and action ==Actions.jump):
	 		if (random.random()<0.70):
	 			return self.chooseAction()

		self.prevAction = action
		return action

	def xdist(self,gs):
		return abs(gs.p1Xpos-gs.p2Xpos)

	def dumpWeightsToFile(self,filename):
		with open(filename, "wb") as myFile:
			pickle.dump(self.w, myFile)


	def retrieveWeightsFromFile(self,filename):
		with open(filename, "rb") as myFile:
			self.w = pickle.load(myFile)


if __name__ == '__main__':
    port = 4998
    p1 = True
    if len(sys.argv) > 1:
        port = int(sys.argv[1])
    
        p1 = False

    agent = BasicQlearnAgent(p1, loadOldTable=False,epsilon=.15, 
    						overwriteFile=True, plot_freq=0)

    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(port, debug=False, agent=agent)

