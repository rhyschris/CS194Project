import hermes
from agent import Agent
from actions import Actions
import random

class ReflexAgent(Agent):
    
    def __init__(self, name="Marty", explore=0.1):
        super(ReflexAgent, self).__init__(name)
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

    agent = ReflexAgent(explore=0.2)
    print "Agent {0} reporting for duty".format(agent.name)
    hermes.main(4998, debug=True, agent=agent)
    
