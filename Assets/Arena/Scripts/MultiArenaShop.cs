using AdventureCore;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

namespace AdventureExtras
{
    /// <summary>
    /// logic for the ArenaShop scene, spawn random shops where players can exchange essence for useful items
    /// </summary>
    /// <remarks><see href="https://adventure.softleitner.com/manual">https://adventure.softleitner.com/manual</see></remarks>
    [HelpURL("https://adventureapi.softleitner.com/class_adventure_extras_1_1_arena_shop.html")]
    public class MultiArenaShop : NetworkBehaviour
    {
        [Tooltip("when this action is started the script displays a dialog for entering the next stage")]
        public CharacterActionBase ExitAction;
        [Tooltip("slots that shops can be put in")]
        public Transform[] Slots;
        [Tooltip("prefabs for shops, random shops are instantiated in slots unless the player already owns the item")]
        public Transform[] Prefabs;
        [Tooltip("displays Next Up: {NextStageName}")]
        public TMP_Text StageText;

        public NetworkVariable<int> Seed { get; } = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);

        private void Start()
        {
            MultiArenaCommon.Instance.Fader.DelayedFadeIn();

            //connect if scene is started directly in editor
            if (!NetworkManager.Singleton.IsListening)
            {
                if (CurrentPlayer.IsMainEditor)
                    NetworkManager.Singleton.StartHost();
                else
                    NetworkManager.Singleton.StartClient();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            StageText.text = "Next Up: " + MultiArenaCommon.Instance.GetStageName();

            if (NetworkManager.Singleton.IsHost)
            {
                ExitAction.Starting.AddListener(onExit);

                var seed = Random.seed;
                Seed.Value = seed;
                createShops(seed);
            }
            else
            {
                ExitAction.IsAvailable = false;
                ExitAction.name = "Waiting for Host...";

                if (Seed.Value != 0)
                    createShops(Seed.Value);
                else
                    Seed.OnValueChanged += seedChanged;
            }
        }

        private void seedChanged(int oldValue, int newValue)
        {
            Seed.OnValueChanged -= seedChanged;
            createShops(newValue);
        }
        private void createShops(int seed)
        {
            var options = new List<Transform>();
            foreach (var prefab in Prefabs)
            {
                var action = prefab.GetComponentInChildren<PurchaseAction>();

                if (action.Items.Any(i => !i.Item.IsStackable && MultiArenaCommon.Instance.GetPlayers().All(p => p.Inventory.HasItem(i.Item))))
                    continue;//skip non stackable items all players already own(weapons and trinkets)

                options.Add(prefab);
            }

            Random.seed = seed;

            foreach (var slot in Slots)
            {
                if (options.Count == 0)
                    break;

                var index = Random.Range(0, options.Count);
                var prefab = options[index];
                options.RemoveAt(index);
                Instantiate(prefab, slot);
            }
        }

        private void onExit()
        {
            this.Delay(() =>
            {
                DialogBase.Main.Show("Exit Shop", "Enter next Stage?", r =>
                {
                    if (r == DialogResult.Yes)
                    {
                        MultiArenaCommon.Instance.FadeOutAll(() => MultiArenaCommon.Instance.LoadStage());
                    }
                    else
                    {
                        ExitAction.EndAction();
                    }
                }, DialogButtons.YesNo, DialogResult.Yes);
            }, 1);
        }
    }
}
