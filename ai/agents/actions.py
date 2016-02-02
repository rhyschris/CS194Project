__author__='rhyschris'

""" Defines the set of actions.
    This functions exactly the same as 
    Actions.cs in the Unity game.
"""
from enum import Enum


class Actions(Enum):
    doNothing = 0
    crouch = 1
    jump = 3
    walkTowards = 0x1 << 2
    runTowards = 0x2 << 2
    moveAway = 0x3 << 2
    blockUp = 0x1 << 4
    blockDown = 0x2 << 4
    attack1 = 0x3 << 4
    attack2 = 0x4 << 4
    attack3 = 0x5 << 4
    attack4 = 0x6 << 4

if __name__ == '__main__':
    print "Contents of actions:"
    
    for act in Actions:
        print repr(act)
