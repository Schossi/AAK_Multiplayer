# __AAK__ Multiplayer Example Project

This repository contains an example integration of Unity Netcode for Gameobjects in the Action Adventure Kit. The implementation can be tried out directly in the Editor using Unity Multiplayer Play Mode.

- [Action Adventure Kit](https://assetstore.unity.com/packages/templates/systems/action-adventure-kit-217284)  
- [Netcode for GameObjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@latest/)
- [Multiplayer Play Mode](https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@latest/)

## Setup

The asset is not included in this repository and has to be downloaded separately. After cloning, the project will be missing SoftLeitner folder.  
![project structure](https://github.com/Schossi/AAK_Multiplayer/blob/master/Project.png)  
Start by downloading it from the asset store or copy it from another project before opening this one to avoid all the errors from missing dependencies.

## MultiArena

MultiArena is a multiplayer version of the Arena demo included in the AAK extras project. It demonstrates a possible minimal multiplayer setup for a simple coop game.

One central part of synchronization is the networking of the actor component. When an action is started by a player locally the name or path of that action is sent over the network and also started on remote instances. This handles a large part of what players actions like jumping, rolling and attacking.

Another important part of synchronization is damage handling. In Multi Arena only local player instances handle damage and send damage events to their counterparts. Remote instances discard any damage they would send or receive themselves.

Player and Scene management is done in the most basic way demonstrated by NfG examples. Players are spawned by the NetworkManager and carried over to any other scenes. Persistance is done at the Host only, data for other players is sent when a game is continued. The ActionAdventure/PlayerData window can be used to check the currently saved data.
![project structure](https://github.com/Schossi/AAK_Multiplayer/blob/master/Project.png)   
