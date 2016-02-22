#encoding:utf-8

'''

Module for real-time plotting of action values
for a q-learning agent.
'''
import matplotlib.figure as figure
import matplotlib.pyplot as plt
import numpy as np
from actions import Actions


class Plotter(object):

	def __init__(self):
		self.fig = None
		plt.ion()

	def preprocess(self, data):
		''' Converts a list into a normalized numpy array ''' 
		container = np.ones ((1, len(data), 3))
		modified = np.array(data)
		modified /= np.sum(modified)

		container[:, :, 1:] = 1 - modified[None, :, None]
		return container

	def launchGraph(self, data):
		''' Launches the graph '''

		self.fig = plt.imshow(data, interpolation='nearest')

		font = {'family' : 'normal',
		        'weight' : 'normal',
		        'size'   : 14}

		plt.rc('font', **font)
		multiplier = 1
		
		for i, act in enumerate(Actions):
			plt.text(i, 0.75*multiplier, act.name, va='center', ha='center')
			multiplier *= -1	

		# Remove ticks and numbers
		plt.tick_params(axis='both', which='both', 
						bottom='off', top='off',\
						labelbottom='off',left='off', labelleft='off',\
						right='off')
	
		plt.show (block=False)

		plt.pause(0.0001)
		plt.tight_layout()		

	def updateGraph(self, data):
		if not data:
			data = np.random.random((1, 12, 3))
		elif type(data) is type([]):
			# Convert to numpy array
			data = self.preprocess(data)

		if self.fig is None:
			self.launchGraph(data)
			return 

		self.fig.set_data(data)
		plt.pause(0.0001)

if __name__ == '__main__':
	# example
	plotter = Plotter()
 	for i in xrange(1, 100):
		plotter.updateGraph(None)	
		
