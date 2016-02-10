"""

Implements an online SGD solver.

TODO: Import RMSProp, etc,
"""

class SGDSolver(object):

	def __init__(self, model):
		""" Creates the solver.

			Params:	
			- model: The model to optimize
			- 