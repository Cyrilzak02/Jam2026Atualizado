using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject howToPlayPanel;
    public void LoadMainScene()
    {
        StartCoroutine(ChangeSceneAfterDelay());
    }

    void Update()
    {
        // Close How To Play with ESC
        if (howToPlayPanel != null &&
            howToPlayPanel.activeSelf &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            HideHowToPlay();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void HideHowToPlay()
    {
        
        howToPlayPanel.SetActive(false);
    }
    

    IEnumerator ChangeSceneAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("SampleScene"); 
    }
    

}
