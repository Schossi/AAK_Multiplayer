using AdventureCore;
using AdventureExtras;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static AdventureExtras.ArenaStage;

public class MultiArenaStage : MonoBehaviour
{
    public bool IsDebug;
    public NetworkPrefabsList Prefabs;
    [Tooltip("when and which enemies spawn in the stage")]
    public StageSpawn[] Spawns;
    [Tooltip("players earn points for each second under par they finish the stage")]
    public int ParTime;
    [Tooltip("displays the current play time of the stage")]
    public TMP_Text TimeText;

    private bool _isPlaying;
    private List<ArenaEnemy> _enemies = new List<ArenaEnemy>();
    private float _playTime;
    private int _spawnIndex;

    private void Start()
    {
        if (!IsDebug)
            NetworkManager.Singleton.OnServerStarted += serverStarted;
    }

    private void Update()
    {
        if (_isPlaying)
        {
            _playTime += Time.deltaTime;

            if (_spawnIndex == Spawns.Length)
            {
                if (_enemies.Count == 0)
                {
                    //onWin();
                }
            }
            else
            {
                if (_playTime >= Spawns[_spawnIndex].Time || (_spawnIndex > 0 && _enemies.Count == 0))
                {
                    foreach (var enemy in spawn(Spawns[_spawnIndex]))
                    {
                        _enemies.Add(enemy);
                        enemy.Dying.AddListener(e => _enemies.Remove(e));
                    }

                    _spawnIndex++;
                }
            }
        }

        //TimeText.text = TimeSpan.FromSeconds(_playTime).ToString(@"mm\:ss");
    }

    public void DebugSpawn(int index)
    {
        foreach (var enemy in spawn(Spawns[index]))
        {
            _enemies.Add(enemy);
            enemy.Dying.AddListener(e => _enemies.Remove(e));
        }
    }

    private IEnumerable<ArenaEnemy> spawn(StageSpawn spawn)
    {
        foreach (var o in spawn.Objects)
        {
            foreach (var enemy in o.GetComponentsInChildren<MultiArenaEnemy>())
            {
                var networkPrefab = Prefabs.PrefabList[enemy.PrefabIndex];
                if (networkPrefab == null)
                {
                    Debug.LogWarning("No Enemy Prefab found for: " + enemy.name);
                    continue;
                }

                var instance = Instantiate(networkPrefab.Prefab, enemy.transform.position, enemy.transform.rotation);

                instance.GetComponent<NetworkObject>().Spawn();

                yield return instance.GetComponent<ArenaEnemy>();
            }
        }
    }

    private void serverStarted()
    {
        DialogBase.Main.Show(ArenaCommon.Instance.GetStageName(), @$"PAR {TimeSpan.FromSeconds(ParTime):mm\:ss}", _ => _isPlaying = true, new string[] { "Fight!" }, selection: DialogResult.Option1);
    }
}
