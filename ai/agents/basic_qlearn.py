
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
				 epsilon=.75, name="Qlearner", plot_freq=0):
		super(BasicQlearnAgent, self).__init__(name)
		self.p1 = isPlayer1

		self.epsilon = epsilon

		self.alpha = .5
		self.prevGamestate = GameState(-4,0,100,4,0,100)
		self.prevAction = Actions.doNothing
		self.bodywidth = 1.0;

		self.possibleXdists = [0, 1, 2, 3, 4,5, 6]

		if (overwriteFile):
			signal.signal(signal.SIGINT, self.signalHandler)
		self.actions = [a for a in Actions]
		self.numActions = len(self.actions)
		self.actionDic = dict()
		self.makeActionDic()
		self.Qtable = dict()


		self.fn = 'savedQTablep1.txt'
		self.bindport = 5555
		if (not self.p1):
			self.fn = 'savedQTablep2.txt'
			self.bindport=5565

		self.ipc_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)    
		self.ipc_server.bind((HOST, self.bindport))
    
		self.ipc_client =  socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
		
		if (not loadOldTable):
			self.initializeTable()
		else:
			self.retrieveQtableFromFile(self.fn)

		# times per second to plot
		# 0 is 'off', but we add modulo plot_freq + 1, and update
		# when counter = 1
		self.plot_freq = (60/plot_freq) + 1
		self.plot_counter = 0

		if plot_freq > 0:
			self.plotter = Plotter()

		print("Done initializing!")

	def signalHandler(self,err,ernum):
		print("dumping to file!")
		self.dumpQtableToFile(self.fn)
		sys.exit(0)

	def makeActionDic(self):
		for i in range(0,self.numActions):
			self.actionDic[self.actions[i]] = i


	def initializeTable(self):
		for xdist in self.possibleXdists:
			for ydist in [-3, -1, 0, 1, 3]:
				#for p1h in [0,1]: #low and high
					#for p2h in [0,1]: #low and high
				for p2flags in [0b00000000,0b10010000,0b00010000, 0b01000000,0b10100000,0b00100000]:
					for p1flags in [0b0000,0b1001,0b0001, 0b0010,0b1010,0b0100]:
						Gamestate = (xdist,ydist,p1flags|p2flags)
						self.Qtable[Gamestate] = [0]*self.numActions
						if (abs(xdist)>2):
							self.Qtable[Gamestate][self.actionDic[Actions.walkTowards]]+=.5
							self.Qtable[Gamestate][self.actionDic[Actions.runTowards]]+=.5




	def getGSTuple(self,gamestate):
		xdist = abs(int(gamestate.p1Xpos-gamestate.p2Xpos-self.bodywidth))

		tupXdist =xdist
		if(xdist>5):
			tupXdist = 6

		ydist = int(gamestate.p1Ypos-gamestate.p2Ypos)
		tupYdist =ydist
		if (ydist<-1):
			tupYdist = -3
		elif(ydist>1):
			tupYdist = 3

		return (tupXdist,tupYdist,gamestate.actionFlags)

	def getQRow(self,gamestate):	
		tup = self.getGSTuple(gamestate)

		return self.Qtable[tup]

	def updateQForStateAction(self,prevGS,curGS,prevAction,reward):
		prevtup = self.getGSTuple(prevGS)
		index = self.actionDic[prevAction]

		curqRow = self.getQRow(curGS)
		maxCurQ = max(curqRow)

		self.Qtable[prevtup][index]+= self.alpha*(reward + maxCurQ - self.Qtable[prevtup][index])

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

		if (p1damage and (p1damage>0)):#end of game, don't count negative
			if (self.p1):
				self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,-1*p1damageval)
			else:
				self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,p1damageval)

		if (p2damage and (p2damage>0)):
			if (self.p1):
				self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,p2damageval)
			else:
				self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,-1*p2damageval)


		self.prevGamestate = gameState
		if (p1win):
			self.prevGamestate = GameState(-4,0,100,4,0,100)
			if (self.p1):
				dumpQtableToFile('geneticQtableFile.txt') #dump to mutual file
				data = bytearray()
				data.append(255)
				print("sending ipc")
				self.ipc_client.sendto(data, (HOST, self.bindport+10))	#let other player know file is ready
			else:
				data, addr = self.ipc_server.recvfrom(1) 
				print('retrieved ipc')
				retrieveQtableFromFile('geneticQtableFile.txt')
		if (p2win):
			self.prevGamestate = GameState(-4,0,100,4,0,100)
			if (not self.p1):
				dumpQtableToFile('geneticQtableFile.txt') #dump to mutual file
				data = bytearray()
				data.append(255)
				self.ipc_client.sendto(data, (HOST, self.bindport-10))	#let other player know file is ready
			else:
				data, addr = self.ipc_server.recvfrom(1) 
				print('got ipc')
				retrieveQtableFromFile('geneticQtableFile.txt')


	def chooseAction(self):
		qRow = self.getQRow(self.prevGamestate)
		maxQ = max(qRow)
		# update plot according to frequency
		self.plot_counter = (self.plot_counter + 1) % (self.plot_freq + 1)

		if self.plot_counter == 1:
			self.plotter.updateGraph(qRow)

		count = qRow.count(maxQ)
		action = None
	 	
		if random.random() < self.epsilon: # exploration 
			print("exploring!!!!!")
			action = random.choice(self.actions) 
		elif count > 1:
			best = [i for i in range(len(self.actions)) if qRow[i] == maxQ]
			print("randomly chose an action out of best possible")
			action = self.actions[random.choice(best)]
		else:
			print("maxq = ",maxQ)
			action = self.actions[qRow.index(maxQ)]
	 
		self.prevAction = action
		self.epsilon-=0.00001
		return action

	def dumpQtableToFile(self,filename):
		#filename = "savedQTablep2.txt"
		#if self.p1:
		#	filename="savedQTablep1.txt"
		with open(filename, "wb") as myFile:
			pickle.dump(self.Qtable, myFile)


	def retrieveQtableFromFile(self,filename):
		#filename = "savedQTablep2.txt"
		#if self.p1:
		#	filename="savedQTablep1.txt"
		with open(filename, "rb") as myFile:
			self.Qtable = pickle.load(myFile)


if __name__ == '__main__':
    port = 4998
    p1 = True
    if len(sys.argv) > 1:
        port = int(sys.argv[1])
    
        p1 = False

    agent = BasicQlearnAgent(p1, loadOldTable=False,epsilon=.15, 
    						overwriteFile=True, plot_freq=20)

    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(port, debug=False, agent=agent)

