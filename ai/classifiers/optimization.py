""" 
Library of optimization algorithms for minibatch training.

"""

def momentum(w, dw, config=None):
  """
  Performs stochastic gradient descent with momentum.

  config format:
  - learning_rate: Scalar learning rate.
  - momentum: Scalar between 0 and 1 giving the momentum value.
    Setting momentum = 0 reduces to sgd.
  - velocity: A numpy array of the same shape as w and dw used to store a moving
    average of the gradients.
  """
  if config is None: config = {}

  config.setdefault('learning_rate', 1e-2)
  config.setdefault('momentum', 0.9)
  v = config.get('velocity', np.zeros_like(w))
  
  next_w = None

  v = config['momentum'] * v - config['learning_rate'] * dw
  next_w = w + v

  config['velocity'] = v
  return next_w, config

def rmsprop(w, dw, config=None):
	if config is None: config = {}
	config.setdefault('learning_rate', 1e-2)
	config.setdefault('decay_rate', 0.99)
	config.setdefault('epsilon', 1e-8)
	config.setdefault('cache', np.zeros_like(x))

	next_w = None

	config['cache'] *= config['decay_rate']
	config['cache'] += (1 - config['decay_rate']) * dw **2
	next_w = w - config['learning_rate'] * w/ np.sqrt (config['cache'] + 1e-8)

	return next_x, config