using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    public Text reincarnationsText;
    public Text objectiveText;

    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public Button returnToMenuButton;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }

        UpdateHUD();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += UpdateHUD;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= UpdateHUD;
    }

    private void UpdateHUD()
    {
        if (GameManager.Instance == null) return;

        if (reincarnationsText != null)
        {
            reincarnationsText.text = "REINCARNATIONS: " + GameManager.Instance.reincarnations;
        }

        if (objectiveText != null)
        {
            objectiveText.text = "OBJECTIVE: " + GameManager.Instance.currentObjective;
        }

        if (GameManager.Instance.isGameOver && gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void OnReturnToMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
    }
}
