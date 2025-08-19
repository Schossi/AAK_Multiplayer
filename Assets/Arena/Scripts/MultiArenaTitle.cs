using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MultiArenaTitle : MonoBehaviour
{
    public Button StartButton;
    public Button ContinueButton;

    private void Awake()
    {
        StartButton.interactable = false;
        ContinueButton.interactable = false;
    }

    private void Start()
    {
        if (CurrentPlayer.IsMainEditor)
        {
            if (!NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.StartHost();

            StartButton.interactable = true;

            StartButton.onClick.AddListener(new UnityAction(startGame));
            ContinueButton.onClick.AddListener(new UnityAction(continueGame));

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

            StartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "waiting for host...";
            ContinueButton.GetComponentInChildren<TMPro.TMP_Text>().text = "waiting for host...";
        }
    }

    private void startGame()
    {
        MultiArenaCommon.Instance.ClearGame();
        MultiArenaCommon.Instance.LoadStage();
    }

    private void continueGame()
    {
        MultiArenaCommon.Instance.LoadShop();
    }
}