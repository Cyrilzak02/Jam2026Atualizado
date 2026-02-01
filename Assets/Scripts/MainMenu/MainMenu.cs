using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadMainScene()
    {
        SceneManager.LoadScene("SampleScene"); // 👈 EXACT scene name
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
