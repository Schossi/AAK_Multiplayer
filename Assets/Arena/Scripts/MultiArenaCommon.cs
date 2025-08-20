using AdventureCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiArenaCommon : MonoBehaviour
{
    public static MultiArenaCommon Instance;

    [Tooltip("used to fade camera to and from black and fade in sound when a scene starts or ends")]
    public Fader Fader;
    [Tooltip("persister used to save stage number and prestige level")]
    public PersisterBase Persister;
    [Tooltip("currency(awarded for finishing a stage under par time)")]
    public ItemBase Essence;
    public ItemBase StartingWeapon;
    public ResourceType Health;
    public Follower LockOn;

    private void Awake()
    {
        Instance = this;
    }

    public MultiArenaPlayerCharacter GetLocalPlayer() => CharacterBase.GetCharacter("PL") as MultiArenaPlayerCharacter;
    public IEnumerable<MultiArenaPlayerCharacter> GetPlayers() => CharacterBase.Characters.OfType<MultiArenaPlayerCharacter>();

    public void Exit()
    {
        FadeOutAll(() => LoadTitle());
    }
    public void Quit()
    {
        Fader.FadeOut(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    public void ReviveAll()
    {
        foreach (var player in FindObjectsByType<MultiArenaPlayer>(FindObjectsSortMode.None))
        {
            player.SendRevive();
        }
    }

    public bool Check() => Persister.Check("STAGE");
    public int GetStage() => Persister.Get("STAGE", 1);
    public string GetStageName()
    {
        var name = "STAGE " + GetStage();
        var prestige = GetPrestige();
        if (prestige > 0)
            name += $"(P{prestige})";
        return name;
    }
    public int GetPrestige() => Persister.Get("PRESTIGE", 0);
    public void SetStage(int value) => Persister.Set(value, "STAGE");
    public void SetPrestige(int value) => Persister.Set(value, "PRESTIGE");
    public void AdvanceStage()
    {
        var nextStage = GetStage() + 1;
        if (nextStage == 5)
        {
            nextStage = 1;
            SetPrestige(GetPrestige() + 1);
        }

        SetStage(nextStage);
    }
    public void SaveGame()
    {
        PersistenceContainer.Instance.Save(Persister.PersistenceArea);
    }
    public void ClearGame()
    {
        PersistenceContainer.Instance.Clear(Persister.PersistenceArea);
    }

    public void LoadPlayers()
    {
        PersistenceContainer.Instance.Load();

        foreach (var player in GetPlayers())
        {
            player.Networker.LoadPlayer();
        }
    }
    public void ResetPlayers()
    {
        foreach (var player in GetPlayers())
        {
            player.Networker.ResetPlayer();
        }
    }

    public void LoadTitle()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("MultiArenaTitle", LoadSceneMode.Single);
    }
    public void LoadShop()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("MultiArenaShop", LoadSceneMode.Single);
    }
    public void LoadStage()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("MultiArenaStage" + GetStage(), LoadSceneMode.Single);
    }

    public void FadeOutAll(Action callback)
    {
        GetLocalPlayer()?.Networker.SendFadeOut();

        this.Delay(() =>
        {
            callback?.Invoke();
        }, Fader.Duration + 0.5f);
    }
}