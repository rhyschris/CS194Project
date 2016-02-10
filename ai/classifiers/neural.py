__author__='rhyschris'

''' 

  Implements a self-contained, fully-connected network 
  neural network of arbitrary depth, on top of a least squares objective.

  Uses Kaiming He or Xavier initialization

  TODO: Support batch normalization (Ioffe & Szegedy 2015) after each layer

'''

import numpy as np
from layers import *
import optimization

class NeuralNet(object):
  """
  A fully-connected neural network with an arbitrary number of hidden layers 
  of arbitrary linearity
  """

  def __init__(self, hidden_dims, input_dim, num_classes=10, reg=0.0, func='relu'):
    """
    Initialize a new FullyConnectedNet.
    
    Inputs:
    - hidden_dims: A list of integers giving the size of each hidden layer.
    - input_dim: An integer giving the size of the input.
    - num_classes: An integer giving the number of classes to classify.
    - reg: Scalar giving L2 regularization strength.
    - weight_scale: Scalar giving the standard deviation for random
      initialization of the weights.
    """
    
    self.reg = reg
    self.num_layers = 1 + len(hidden_dims)
    self.params = {}

    self.forward_func = None
    self.back_func = None

    if func == 'relu':
      self.forward_func = affine_relu_forward
      self.back_func = affine_relu_backward
    else:
      self.forward_func = affine_tanh_forward
      self.back_func = affine_tanh_backward

    # Notes whether to use kaiming_he (2015) or xavier (2010) initialization.
    # Xavier initialization optimizes variance for symmetric nonlinearities.
    # Kaiming-He initialization optimizes variance for RELU's.
    # The difference between the two is an extra factor of 2 for Kaiming-He, so 
    # use a multiplier instead.
    using_kaiming_he = np.sqrt(2.) if func == 'relu' else 1.
    
    dims = [input_dim] + hidden_dims + [num_classes]

    for i in xrange(1, len(dims)):
      fan_in = dims[i-1] * dims[i]
      weight_scale = using_kaiming_he / np.sqrt(fan_in) 

      self.params[ 'W{0}'.format(i) ] = np.random.normal (0, weight_scale, (dims[i-1], dims[i]))
      self.params[ 'b{0}'.format(i) ] = np.zeros (dims[i])

  def loss(self, X, y=None):
    """
    Compute loss and gradient for the fully-connected net.

    Params:
    - X: (N, D) shaped matrix of input vectors
    - y: (N, ) output vector of class labels if training, None if testing

    Return: 
    - loss: The loss over this minibatch
    - grad: The gradients over this minibatch.
    """
    X = X.astype(self.dtype)
    mode = 'test' if y is None else 'train'

    for bn_param in self.bn_params:
      bn_param[mode] = mode

    scores = None

    cache = {}
    inpt = X

    for i in range(1, self.num_layers):
      w, b = self.params['W{0}'.format(i)], self.params['b{0}'.format(i)]
      inpt, cache[i] = self.forward_func (inpt, w, b)
  
    # set scores with top layer
    # i is still in local namespace with i == num_layers
    scores, cache[self.num_layers] = affine_forward (inpt, self.params['W{0}'.format(self.num_layers)], \
                                                   self.params['b{0}'.format(self.num_layers)])

    # If test mode return early
    if mode == 'test':
      return scores
    # Least squares loss

    loss, dout = softmax_loss (scores, y)
    for i in xrange(1, self.num_layers + 1):
      loss += 0.5 * self.reg * np.sum(self.params['W{0}'.format(i)] ** 2)

    # Calculate gradient    
    for j in list(np.arange(self.num_layers, 0, -1)):
      
      dW, db = None, None
      # back-propagate batchnorm, dropout
      if j != self.num_layers:   
        dout, dW, db = affine_relu_backward (dout, cache[j])
      else:
        dout, dW, db = affine_backward (dout, cache[j])
      
      dW += self.reg * self.params['W{0}'.format(j)]
      grads['W{0}'.format(j)] = dW
      grads['b{0}'.format(j)] = db

    return loss, grads


  def train(self, X, y):
    """ Trains the neural network on the given examples. """

    loss, grads = self.loss (X, y)
    pass    


