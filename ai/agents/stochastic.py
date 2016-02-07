import hermes
from agent import Agent
from actions import Actions
import random
import sys

import argparse

class StochasticAgent(Agent):
    
    def __init__(self, name="Stepp", explore=0.1):
        super(StochasticAgent, self).__init__(name)
        self.lastAction = None
        self.exploration = explore
    # Override
    def chooseAction(self):
        """ Chooses a new single action randomly among the 
            list of enums with probability self.explore.
        """ 
        possActions = [a for a in Actions]
        
        if random.random() < self.exploration or self.lastAction is None:
            self.lastAction = random.choice(possActions)
        return self.lastAction


if __name__ == '__main__':
    
    ex = 0.05
    port = 4998
    if len(sys.argv) > 2:
        ex = float(sys.argv[2])
    if len(sys.argv) > 1:
        port = int(sys.argv[1])
    
    agent = StochasticAgent(explore = ex)
    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(port, debug=False, agent=agent)
    
