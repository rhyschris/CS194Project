__author__ = 'rhyschris'

from gamestate import gameState
import struct 

"""
An agent has the ability to ingest a gamestate and do something with it.
This stub implementation does nothing.
"""


class Agent(object):
    
    def __init__(self, name="Marty"):
       self.name = name
       self.gameState = None
       
    def ingestState(self, data):
        """ Ingests the given gamestate from the network.
        """
        args = struct.unpack('!ffffffB',data)
        self.gameState = GameState(args[0:5])
        self.gameState.parseFlags(args[6])


    def chooseAction(self):
        """ Chooses an action, based off of a given game state.
            Returned is the bitwise or'ed set of actions.  
        """
        print "nothing"
        pass
# end
        
