'''

Implements a Q-Learning with deep function approximation.

'''

import numpy as np
import hermes
import agent

from actions import Actions

from gamestate import GameState
import neural



def DeepQLearningAgent (Agent):

    def __init__(self, name='Andrew Ng'):
        super(DeepQLearningAgent, self).__init__(name)
        self.epsilon = 0.20 # epsilon-greediness
        self.alpha = 0.5  # learning rate
        self.actions = [a for a in Actions]
        self.recent_states = []

        # TODO: get model imports right
        # self.model = NeuralNet(hidden_dims=[100], input_dim=20, num_classes=8, reg=0.0)
    
    def featurize (self, state, action):
        ''' Takes a (state, action) pair and translates it 
            into a dense feature vector
            (represented as a numpy array) 
            that can be fed into the non-linear function approximator Q.
        ''' 
    
    def ingestState(self, gamestate):
        pass
        


    def policy (self, s):
        ''' Computes the optimal policy for this state-action pair
            by running the feed-forward approximator network.
        
            Returns:
            - index: the ordered (zero-indexed) action with the highest score.
            Indexing is in the enumerated ascending order.
            - scores: The per-class probability scores.
        
            '''
        # Use function approximator provided by model
        vec = self.featurize(s)
        scores = self.model.loss (vec, None)
        return np.argmax (scores), scores
    
    # Override
    def chooseAction(self):
        ''' 
        Performs epsilon-greedy model selection
        ''' 
        if np.random.random() < epsilon:
            print '--- EXPLORATION ---'
            return np.random.choice (self.actions)
        else:
            s = self.recent_states[-1]
            return self.policy (s)

    def learn(self, s, a, r, newState):
        ''' Update model weights using an SGD method on the model. 
            TODO: modify neural.py to give a forward pass and a backward pass option
        '''
        # reward = r + self.discount * V_opt(new_state) - Q_score (s)
        # Back-prop into model (backpass should have some argument to give an 
        # an augmented reward. 
    
        # w += alpha * reward * phi(x)
        pass
