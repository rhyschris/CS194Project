import hermes
from agent import Agent
from actions import Actions
import random
import sys

class DumbAggressiveAgent(Agent):
    
    def __init__(self, name="Stepp"):
        super(DumbAggressiveAgent, self).__init__(name)

    def chooseAction(self):
        return beAggressive()


    def beAggressive(self):
        if (abs(self.gamestate.p1Xpos-self.gamestate.p2Xpos)<self.hitboxsize):
            if (!self.gamestate.p1attacking):
                return random.choice([Actions.attack1, Actions.attack2, Actions.attack3, Actions.attack4])
            elif (!self.gamestate.p1high):
                return random.choice([Actions.blockup, Actions.blockup, Actions.moveAway,Actions.attack1, Actions.attack2, Actions.attack3, Actions.attack4])
            else:
                return random.choice([Actions.blockdown, Actions.blockup, Actions.moveAway,Actions.attack1, Actions.attack2, Actions.attack3, Actions.attack4])

        else:
            return random.choice([Actions.walkTowards, Actions.walkTowards, Actions.runTowards, Actions.jump])



if __name__ == '__main__':
    agent = DumbAggressiveAgent()
    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(4998, debug=True, agent=agent)