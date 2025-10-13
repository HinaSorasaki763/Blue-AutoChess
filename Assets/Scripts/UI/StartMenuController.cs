using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public TextMeshProUGUI PlayerName;
    public void StartGame()
    {
        // 加载场景，确保场景名正确

        SceneManager.LoadScene("SampleScene");
    }

    public void Multiplayer()
    {
        //  SceneManager.LoadScene("MultiplayerPort");
    }

    public void QuitGame()
    {
        // 退出游戏
        Application.Quit();
    }

    public void BGMPlay()
    {
        BGMManager.Instance.ToggleBGM();
    }
    public void Update()
    {
        if (PlayerName.gameObject.activeInHierarchy && PlayerSession.Instance.Data != null)
        {
            PlayerName.text = PlayerSession.Instance.Data.Name;
        }

    }
}
