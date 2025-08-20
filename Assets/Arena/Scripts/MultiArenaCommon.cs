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
    [Tooltip("currency(awarded for finishing a stage under par time)")]
    public ItemBase Essence;
    public ResourceType Health;
    public Follower LockOn;

    private static int _stage = 1;
    private static int _prestige = 0;

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

    public int GetStage() => _stage;
    public string GetStageName()
    {
        var name = "STAGE " + GetStage();
        var prestige = GetPrestige();
        if (prestige > 0)
            name += $"(P{prestige})";
        return name;
    }
    public int GetPrestige() => _prestige;
    public void SetStage(int value) => _stage = value;
    public void SetPrestige(int value) => _prestige = value;
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
    public void ClearGame()
    {
        _stage = 1;
        _prestige = 0;
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