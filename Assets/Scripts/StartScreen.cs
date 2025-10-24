using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    [SerializeField] Button StartGameBtn = null;
    [SerializeField] Button SettingBtn = null;
    [SerializeField] Button ExitBtn = null;

    [SerializeField] CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
        StartGameBtn.onClick.AddListener(StartNewGame);
        ExitBtn.onClick.AddListener(Quit);
        SettingBtn.onClick.AddListener(Settings);
    }

    void StartNewGame()
    {
        canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
        GameMgr.Instance.StartGame();
    }

    public void Settings()
    {
        SettingMgr.Instance.Show();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
