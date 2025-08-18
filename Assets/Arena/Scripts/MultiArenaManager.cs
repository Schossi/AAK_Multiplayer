using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class MultiArenaManager : MonoBehaviour
{
    private void Start()
    {
        if (CurrentPlayer.IsMainEditor)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
    }
}