Agents Module
--------------

Package requirements (install with pip):
- enum34 

Here you can find the logic for communicating actions to the Unity game.  Files:

agent.py: Provides the basic agent superclass.  Agents have two overarching behaviors
- ingestState: Make sense of the state passed in (optional)
- chooseAction: Decide what action to take, given the last ingested gamestate.


stochastic.py: A reflex agent layering on top of agent.  Chooses actions randomly with probability
p, or repeats the previous action with probability 1 - p.  


actions.py: Exposes the Actions enum to Python agents in the Unity codebase, with equivalent network functionality.  


hermes.py: A messenger that binds to the UDP port required for Unity to communicate the game state.
It also sets up a UDP client that sends the action to Unity on behalf of the agent. 
You shouldn't have to change anything in this file.


Invocation:
-------------
Launch the reflex agent by calling 
```
python stochastic.py [explore rate]
```
The only argument, explore rate, is a decimal 0 - 1, which gives a probability of choosing a new action at every cycle
Default clock speed is 10 Hz; default exploration rate is 0.3.  

