__author__ = 'rhyschris'

'''
The sender that handles waiting on a GameState and returning an action.
Layers lightly over a socket.

'''

import sys
import socket
import time # for sleep (precursor implementation to connecting directly and awaiting gamestate)
from agent import Agent


HOST = '127.0.0.1'
_PAGE_SZ = 4096
_SLEEP_QUANTUM = 0.400


def main(port, debug=False, agent=Agent()):
    """ Control logic.
        Starts by binding UDP socket to localhost : port + 1
        and blocks to wait for a response.
    """
    server, client = create_socket(port + 1)

    if debug:
        send_random (client, port, agent)
    else:
        listen (server, client, port, agent)
        
def send_action (client, outport, action):
    ''' Sends the action through the client socket '''
    data = action.value
    client.sendto(data, (HOST, port))

def send_random (client, outport, agent):
    """ Wakes up and sends packets every once in a while. """
    while True:
        time.sleep(_SLEEP_QUANTUM)
        agent.ingestState("")
        action = agent.chooseAction()
        print "action: ", action, " type: ", type(action)

def create_socket (bindport):
    """ Creates listening UDP socket, server, and client UDP socket, client.
        Binds the server, and sets up the client.  
        Return: (server socket, client socket)
    """    
    server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)    
    server.bind((HOST, bindport))
    
    print "Listening on localhost, port {0}".format(bindport)
    
    client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
    return server, client

def listen (server, client, outport, agent):
 
   while True:
        data, addr = server.recvfrom(_PAGE_SZ) 
        print "Msg from host: {0}".format(addr)
        print "Data: \n{0}".format(data)

        agent.ingestState(data)
        action = agent.chooseActions()
        print "action: ", action, " type: ", type(action)
        

if __name__ == '__main__':
    port = 4998
    if len(sys.argv) > 1:
        port = int(sys.argv[1])
    main(port, debug=True)
