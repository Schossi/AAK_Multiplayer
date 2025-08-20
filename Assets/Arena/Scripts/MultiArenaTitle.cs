using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiArenaTitle : MonoBehaviour
{
    [Tooltip("button that starts a new game, script automatically attaches to onClick")]
    public Button NewGameButton;
    [Tooltip("button continues the last game, script automatically attaches to onClick and sets interactable")]
    public Button ContinueButton;

    private void Awake()
    {
        NewGameButton.interactable = false;
        ContinueButton.interactable = false;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        MultiArenaCommon.Instance.Fader.DelayedFadeIn();

        if (CurrentPlayer.IsMainEditor)
        {
            if (!NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.StartHost();

            NewGameButton.interactable = true;

            NewGameButton.onClick.AddListener(onNewGame);
            ContinueButton.onClick.AddListener(onContinue);

            if (MultiArenaCommon.Instance.GetStage() > 1)
            {
                ContinueButton.interactable = true;
                ContinueButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Continue " + MultiArenaCommon.Instance.GetStageName();
            }
        }
        else
        {
            if (!NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.StartClient();

            NewGameButton.GetComponentInChildren<TMPro.TMP_Text>().text = "waiting for host...";
            ContinueButton.GetComponentInChildren<TMPro.TMP_Text>().text = "waiting for host...";
        }
    }

    private void onNewGame()
    {
        MultiArenaCommon.Instance.FadeOutAll(() =>
        {
            MultiArenaCommon.Instance.ClearGame();
            MultiArenaCommon.Instance.LoadStage();
        });
    }

    private void onContinue()
    {
        MultiArenaCommon.Instance.FadeOutAll(() =>
        {
            MultiArenaCommon.Instance.LoadShop();
        });
    }
}