
'''

GameState is the recipient class for a network packet from a Unity fighting game.
It contains both player's positions and health, and their attack status. 

'''

class GameState(object):

	def __init__(self,p1X,p1Y,p1H,p2X,p2Y,p2H):
		self.p1AttackMask = 0x01; #1st bit
		self.p1BlockMask = 0x02; #2nd bit
		self.p1CrouchMask = 0x04; #3rd bit
		self.p1HighMask = 0x08; #4th bit

		self.p2AttackMask = 0x10; #5th bit
		self.p2BlockMask = 0x20; #6th bit
		self.p2CrouchMask = 0x40; #7th bit
		self.p2HighMask = 0x80; #8th bit

		self.p1Xpos = p1X
		self.p1Ypos = p1Y
		self.p1Health = p1H

		self.p2Xpos = p2X
		self.p2Ypos = p2Y
		self.p2Health = p2H

		self.actionFlags = 0

	def parseFlags(self,flags):

		self.actionFlags = flags
		self.p1Attacking = flags&self.p1AttackMask
		self.p1Blocking = flags&self.p1BlockMask
		self.p1Crouching = flags&self.p1CrouchMask
		self.p1High = flags&self.p1HighMask

		self.p2Attacking = flags&self.p2AttackMask
		self.p2Blocking = flags&self.p2BlockMask
		self.p2Crouching = flags&self.p2CrouchMask
		self.p2High = flags&self.p2HighMask

