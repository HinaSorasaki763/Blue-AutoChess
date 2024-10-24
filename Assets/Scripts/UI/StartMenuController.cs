using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
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
}
