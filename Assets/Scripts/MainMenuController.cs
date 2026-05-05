using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    public void OnQuitButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
