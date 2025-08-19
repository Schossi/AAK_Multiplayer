using AdventureExtras;
using System.Collections.Generic;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

public class MultiArenaDebug : MonoBehaviour
{
    public NetworkPrefabsList Prefabs;
    public GameObject[] Spawns;

    private List<ArenaEnemy> _enemies = new List<ArenaEnemy>();

    private void Start()
    {
        if (CurrentPlayer.IsMainEditor)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
    }

    public void DebugSpawn(int index)
    {
        foreach (var enemy in MultiArenaStage.Spawn(Prefabs, new GameObject[] { Spawns[index] }))
        {
            _enemies.Add(enemy);
            enemy.Dying.AddListener(e => _enemies.Remove(e));
        }
    }

    public void DebugClear()
    {
        foreach (var enemy in _enemies.ToArray())
        {
            enemy.Die();
        }
    }
}
