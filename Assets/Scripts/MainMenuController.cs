using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    private void Start()
    {
        // Setup panel initial state
        ShowMainPanel();

        // Setup main buttons
        if (startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptionsPanel);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
        if (backButton != null) backButton.onClick.AddListener(ShowMainPanel);

        // Setup difficulty buttons
        if (easyButton != null) easyButton.onClick.AddListener(() => SetDifficulty(GameManager.DifficultyLevel.Easy));
        if (mediumButton != null) mediumButton.onClick.AddListener(() => SetDifficulty(GameManager.DifficultyLevel.Medium));
        if (hardButton != null) hardButton.onClick.AddListener(() => SetDifficulty(GameManager.DifficultyLevel.Hard));
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void ShowOptionsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    private void SetDifficulty(GameManager.DifficultyLevel level)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.difficulty = level;
            Debug.Log($"Difficulty set to: {level}");
            // Return to main panel after selection
            ShowMainPanel();
        }
    }

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
