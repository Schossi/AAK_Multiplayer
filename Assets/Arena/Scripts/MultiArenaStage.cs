using AdventureCore;
using AdventureExtras;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using static AdventureExtras.ArenaStage;

public class MultiArenaStage : MonoBehaviour
{
    public static MultiArenaStage CurrentStage;

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

    private void Awake()
    {
        CurrentStage = this;
    }

    private void Start()
    {
        //connect if scene is started directly in editor
        if (!NetworkManager.Singleton.IsListening)
        {
            if (CurrentPlayer.IsMainEditor)
                NetworkManager.Singleton.StartHost();
            else
                NetworkManager.Singleton.StartClient();
        }

        if (NetworkManager.Singleton.IsHost)
        {
            this.Delay(() => DialogBase.Main.Show(MultiArenaCommon.Instance.GetStageName(), @$"PAR {TimeSpan.FromSeconds(ParTime):mm\:ss}", _ =>
            {
                _isPlaying = true;
            }, new string[] { "Fight!" }, selection: DialogResult.Option1), 1f);
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton?.IsHost != true)
            return;

        if (_isPlaying)
        {
            _playTime += Time.deltaTime;

            if (_spawnIndex == Spawns.Length)
            {
                if (_enemies.Count == 0)
                {
                    onWin();
                }
            }
            else
            {
                if (_playTime >= Spawns[_spawnIndex].Time || (_spawnIndex > 0 && _enemies.Count == 0))
                {
                    foreach (var enemy in Spawn(Prefabs, Spawns[_spawnIndex].Objects))
                    {
                        _enemies.Add(enemy);
                        enemy.Dying.AddListener(e => _enemies.Remove(e));
                    }

                    _spawnIndex++;
                }
            }
        }

        TimeText.text = TimeSpan.FromSeconds(_playTime).ToString(@"mm\:ss");
    }

    private void OnDestroy()
    {
        if (CurrentStage == this)
            CurrentStage = null;
    }

    public void CheckGameOver()
    {
        if (CharacterBase.Characters.Any(c => c.tag == "Player"))
            return;

        _isPlaying = false;

        this.Delay(() => DialogBase.Main.Show("STAGE " + MultiArenaCommon.Instance.GetStage().ToString(), "GAME OVER!", _ =>
        {
            foreach (var player in FindObjectsByType<MultiArenaPlayer>(FindObjectsSortMode.None))
            {
                player.SendRevive();
            }

            MultiArenaCommon.Instance.LoadTitle();
        }, new string[] { "Title Screen" }, selection: DialogResult.Option1), 2f);
    }

    private void onWin()
    {
        _isPlaying = false;

        var bonus = Math.Max(0, ParTime - Mathf.RoundToInt(_playTime));
        if (bonus > 0)
        {
            foreach (var player in CharacterBase.Characters.Where(c => c.tag == "Player"))
            {
                player.InventoryBase.AddItems(new ItemQuantity(MultiArenaCommon.Instance.Essence, bonus));
            }
        }

        this.Delay(() => DialogBase.Main.Show("STAGE " + MultiArenaCommon.Instance.GetStage().ToString(), @$"WELL DONE!{Environment.NewLine}TIME BONUS: {bonus}", _ =>
        {
            MultiArenaCommon.Instance.AdvanceStage();
            MultiArenaCommon.Instance.LoadShop();
        }, new string[] { "Enter Shop" }, selection: DialogResult.Option1), 3f);
    }

    public static IEnumerable<ArenaEnemy> Spawn(NetworkPrefabsList prefabs, GameObject[] objects)
    {
        foreach (var o in objects)
        {
            foreach (var enemy in o.GetComponentsInChildren<MultiArenaEnemy>())
            {
                var networkPrefab = prefabs.PrefabList[enemy.PrefabIndex];
                if (networkPrefab == null)
                {
                    Debug.LogWarning("No Enemy Prefab found for: " + enemy.name);
                    continue;
                }

                var instance = Instantiate(networkPrefab.Prefab, enemy.transform.position, enemy.transform.rotation);

                instance.GetComponent<NetworkObject>().Spawn(true);

                yield return instance.GetComponent<ArenaEnemy>();
            }

            //spawn enemies from solo player too so i dont have to redo spawns
            foreach (var enemy in o.GetComponentsInChildren<ArenaEnemy>())
            {
                if (enemy.GetComponent<MultiArenaEnemy>())
                    continue;//already spawned from multi arena enemy

                var name = enemy.name.Split(' ')[0];
                if (name.StartsWith("Arena"))
                    name = name.Replace("Arena", "MultiArena");

                var networkPrefab = prefabs.PrefabList.FirstOrDefault(p => p.Prefab.name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (networkPrefab == null)
                {
                    Debug.LogWarning("No Enemy Prefab found for: " + enemy.name);
                    continue;
                }

                var instance = Instantiate(networkPrefab.Prefab, enemy.transform.position, enemy.transform.rotation);

                instance.GetComponent<NetworkObject>().Spawn(true);

                yield return instance.GetComponent<ArenaEnemy>();
            }
        }
    }
}
