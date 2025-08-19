using AdventureCore;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiArenaCommon : MonoBehaviour
{
    public static MultiArenaCommon Instance;

    [Tooltip("currency(awarded for finishing a stage under par time)")]
    public ItemBase Essence;
    public Follower LockOn;

    private static int _stage = 1;
    private static int _prestige = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void Exit()
    {
        LoadTitle();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public IEnumerable<MultiArenaPlayerCharacter> GetPlayers() => CharacterBase.Characters.OfType<MultiArenaPlayerCharacter>();

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
}