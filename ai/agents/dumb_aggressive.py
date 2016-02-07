import hermes
from agent import Agent
from actions import Actions
import random
import sys

class DumbAggressiveAgent(Agent):
    
    def __init__(self, name="Stepp"):
        super(DumbAggressiveAgent, self).__init__(name)
        self.hitboxsize = 1.0 + 1.0; #width of hitbox + 2*halfwidth player  

    def chooseAction(self):
        return self.beAggressive()


    def beAggressive(self):
        if (abs(self.gameState.p1Xpos-self.gameState.p2Xpos)<self.hitboxsize):
            if (not self.gameState.p1Attacking):
                return random.choice([Actions.attack1, Actions.attack2, Actions.attack3, Actions.attack4,Actions.moveAway])
            elif (not self.gameState.p1High):
                return random.choice([Actions.blockUp, Actions.blockUp, Actions.moveAway,Actions.attack1, Actions.attack2, Actions.attack3, Actions.attack4])
            else:
                return random.choice([Actions.blockDown, Actions.blockDown, Actions.moveAway,Actions.attack1, Actions.attack2, Actions.attack3, Actions.attack4])

        else:
            return random.choice([Actions.walkTowards, Actions.walkTowards, Actions.runTowards, Actions.jump])



if __name__ == '__main__':
    agent = DumbAggressiveAgent()
    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(4998, debug=False, agent=agent)