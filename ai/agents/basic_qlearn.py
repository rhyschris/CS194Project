
import hermes
from gamestate import GameState
from agent import Agent
from actions import Actions
import struct 
import random
import sys
import cPickle as pickle


class BasicQlearnAgent(Agent):

	def __init__(self,loadOldTable=False, name="Qlearner"):
		super(BasicQlearnAgent, self).__init__(name)
		self.epsilon = .75
		self.alpha = .5
		self.prevGamestate = GameState(-4,0,100,4,0,100)
		self.prevAction = Actions.doNothing

		self.possibleXdists = [-6, -4, -3, -2, -1, 0, 1, 2, 3, 4, 6]

		self.actions = [a for a in Actions]
		self.numActions = len(self.actions)
		self.actionDic = dict()
		self.makeActionDic()
		self.Qtable = dict()

		if not loadOldTable:
                    self.initializeTable()
		else:
                    self.retrieveQtableFromFile()

		print("Done initializing!")

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
							self.Qtable[Gamestate][self.actionDic[Actions.walkTowards]]+=.1
							self.Qtable[Gamestate][self.actionDic[Actions.runTowards]]+=.1




	def getGSTuple(self,gamestate):
		xdist = int(gamestate.p1Xpos - gamestate.p2Xpos)

		tupXdist =xdist
		if  xdist < -4:
			tupXdist = -6
		elif xdist > 4 :
			tupXdist = 6

		ydist = int(gamestate.p1Ypos - gamestate.p2Ypos)
		tupYdist = ydist
		if ydist < -1:
			tupYdist = -3
		elif ydist > 1:
			tupYdist = 3

		return (tupXdist,tupYdist,gamestate.actionFlags)

	def getQRow(self,gamestate):
		tup = self.getGSTuple(gamestate)#(int(gamestate.p1XPos),int(gamestate.p2XPos),int(gamestate.p1XPos),int(gamestate.p1XPos),int(gamestate.p1XPos),int(gamestate.p1XPos),gamestate.actionFlags)

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

		if (gameState.p1Health <self.prevGamestate.p1Health):
			self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,1)

		if (gameState.p2Health <self.prevGamestate.p2Health):
			self.updateQForStateAction(self.prevGamestate,gameState,self.prevAction,-1)

		self.prevGamestate = gameState

	def chooseAction(self):
		qRow = self.getQRow(self.prevGamestate)
		maxQ = max(qRow)
		count = qRow.count(maxQ)
		action = None
	 	
		if random.random() < self.epsilon: # exploration 
			print("exploring!!!!!")
			action = random.choice(self.actions) 
		elif count > 1:
			best = [i for i in range(len(self.actions)) if qRow[i] == maxQ]
			print("ranomly chose an action out of best possible")
			action = self.actions[random.choice(best)]
		else:
			print("maxq = ",maxQ)
			action = self.actions[qRow.index(maxQ)]
	 
		self.prevAction = action
		self.epsilon-=0.0001
		return action

	def dumpQtableToFile(self):
		with open("savedQTable.txt", "wb") as myFile:
			pickle.dump(self.Qtable, myFile)

	def retrieveQtableFromFile(self):
		with open("savedQTable.txt", "rb") as myFile:
			self.Qtable = pickle.load(myFile)


if __name__ == '__main__':
    agent = BasicQlearnAgent(False)
    port = 4998
    if len(sys.argv) > 1:
	    port = int(sys.argv[1])
    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(port, debug=False, agent=agent)
