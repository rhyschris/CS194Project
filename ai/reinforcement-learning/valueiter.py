''' 
Implements value iteration using numpy.

Game state:
 - player_dist - opp_dist (X, Y): Dim = (x_grid, y_grid) 
 - user-attack vs opp_block (attack1-4) x (blockUp blockDown) (attack x block)
 - user-block vs opp_attack (attack x block)
 - expected size: 200 x 200 x 8 x 8

Consider using parameter sharing in the end range
'''
import numpy as np

class ValueIteration (object):
    
    def __init__(self, shape, epsilon = 0.001):
        """ 
        Initializes a value iteration solver on a state space. 
        Params:
        - shape: yields 4D tensor (x_grid, y_grid, attack x blocks, attack x blocks)
                 to represent all possible game states
        """ 
        self.eps = epsilon

        # The value of each state
        self.V = np.zeros (shape, dtype=np.float32)
        # Policy: represents a numeric action policy according to actions.py
        self.pi = np.zeros (shape, dtype=np.int8)
    
    def Q_opt(self, s, a):
        '''
        Given a state, take s 

        
        '''

