"""
  Modular implementation of layers for deep learning systems.

  Format and architecture adapted from @jcjohnson.   

"""

import numpy as np


def softmax_loss (x, y):
  """ Computes the cross-entropy loss. 
      Return:
      - probs: the class-wise probability
      - dx: the gradient of this loss fn wrt x  
  """

  b = np.max(x, axis=1, keepdims=True)
  probs = np.exp(x - b)
  probs /= np.sum(probs, axis=1, keepdims=True)

  N = x.shape[0]
  dx = probs.copy()
  dx[np.arange(N), y] = 1
  dx /= N
  return probs, dx


def affine_forward (x, w, b):
  """ Does Wx + b """
  cache = x, w, b
  return x.dot(w) + b, cache 

def affine_backward (dout, cache):
  """ Backpropagates Wx + b with respect to all partials """
  x, w, b = cache
  dx = dout.dot(w.T)
  dw = dout.T.dot(dout)
  db = np.sum(dout, axis=0)
  return dx, dw, db


def affine_relu_forward (x, w, b):
  """ 
  Takes an activation and returns max(Wx + b, 0)
  """
  activation, affine_cache = affine_forward
  relu_cache = activation
  return np.maximum(0, cache), (affine_cache, relu_cache)

def affine_relu_backward (dout, cache):
  """ Back propagation through an affine-relu layer """
  affine_cache, relu_cache = cache
  d_inter = np.zeros_like (relu_cache)
  d_inter[np.where(relu_cache > 0) ] = 1
  dout *= d_inter
  return affine_backward (dout, affine_cache)


def affine_tanh_forward (x, w, b):
  """ Takes an activation and returns tanh(Wx + b) """
  activation, affine_cache = affine_forward (x, w, b)
  tanh_cache =  activation
  out = np.tanh(activation)
  return out, (affine_cache, tanh_cache)

def affine_tanh_backward (dout, cache):
  """ Backpropagation through a tanh layer """
  affine_cache, out = cache
  dout *= (1 - out*out) # d_tanh / da = 1 - a**2
  return affine_backward (dout, affine_cache)


def batchnorm_forward(x, gamma, beta, bn_param):
  """
  Forward pass for batch normalization.
  
  During training the sample mean and (uncorrected) sample variance are
  computed from minibatch statistics and used to normalize the incoming data.
  During training we also keep an exponentially decaying running mean of the mean
  and variance of each feature, and these averages are used to normalize data
  at test-time.

  At each timestep we update the running averages for mean and variance using
  an exponential decay based on the momentum parameter:

  running_mean = momentum * running_mean + (1 - momentum) * sample_mean
  running_var = momentum * running_var + (1 - momentum) * sample_var

  Note that the batch normalization paper suggests a different test-time
  behavior: they compute sample mean and variance for each feature using a
  large number of training images rather than using a running average. For
  this implementation we have chosen to use running averages instead since
  they do not require an additional estimation step; the torch7 implementation
  of batch normalization also uses running averages.

  Input:
  - x: Data of shape (N, D)
  - gamma: Scale parameter of shape (D,)
  - beta: Shift paremeter of shape (D,)
  - bn_param: Dictionary with the following keys:
    - mode: 'train' or 'test'; required
    - eps: Constant for numeric stability
    - momentum: Constant for running mean / variance.
    - running_mean: Array of shape (D,) giving running mean of features
    - running_var Array of shape (D,) giving running variance of features

  Returns a tuple of:
  - out: of shape (N, D)
  - cache: A tuple of values needed in the backward pass
  """
  mode = bn_param['mode']
  eps = bn_param.get('eps', 1e-5)
  momentum = bn_param.get('momentum', 0.9)

  N, D = x.shape
  running_mean = bn_param.get('running_mean', np.zeros(D, dtype=x.dtype))
  running_var = bn_param.get('running_var', np.zeros(D, dtype=x.dtype))

  out, cache = None, None
  if mode == 'train':
  
    sample_mu = x.mean(axis=0)
    sample_var = x.var(axis=0)

    running_mean = momentum * running_mean  + (1-momentum) * sample_mu
    running_var = momentum * running_var + (1-momentum) * sample_var

    out = np.copy(x)

    out -= sample_mu
    std = np.sqrt(sample_var + eps)
    out /= std
    cache = (x, out, gamma, beta, sample_mu, std)

    out = gamma * out + beta

  elif mode == 'test':
 
    out = np.copy(x)
    out -= running_mean
    out /= (np.sqrt(running_var + eps))
    out = gamma * out + beta

  else:
    raise ValueError('Invalid forward batchnorm mode "%s"' % mode)

  # Store the updated running means back into bn_param
  bn_param['running_mean'] = running_mean
  bn_param['running_var'] = running_var

  return out, cache


def batchnorm_backward(dout, cache):
  """
  Backpropagation for batch normalization.  Uses a simplified gradient flow.
  """
  dx, dgamma, dbeta = None, None, None

  x, activations, gamma, beta, mu, std = cache

  N, D = dout.shape
  k = x - mu

  dbeta = np.sum(dout, axis=0)
  dgamma = np.sum(dout * activations, axis=0)
  dx = gamma / std * ( (-k / (std**2) * np.sum (dout*k, axis=0) - dbeta) / N + dout)

  return dx, dgamma, dbeta

