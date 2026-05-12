using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Serializable]
    public class WeaponSlotView
    {
        public GameObject root;
        public Image icon;
        public TMP_Text label;
        public Image border;
        public CanvasGroup canvasGroup;
        public TMP_Text shieldChargesText;
    }

    [Serializable]
    public class GrenadeSubSlotView
    {
        public GameObject root;
        public Image icon;
        public TMP_Text label;
        public TMP_Text countText;
        public Image border;
        public CanvasGroup canvasGroup;
    }

    [Header("Top-Left: Echoes")]
    [SerializeField] private TMP_Text echoesLeftText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Top-Right: Objective")]
    [SerializeField] private TMP_Text objectiveLabelText;
    [SerializeField] private TMP_Text objectiveValueText;

    [Header("Bottom-Right: Loadout")]
    [SerializeField] private WeaponSlotView primarySlot;
    [SerializeField] private WeaponSlotView secondarySlot;
    [SerializeField] private GrenadeSubSlotView fragSlot;
    [SerializeField] private GrenadeSubSlotView flashbangSlot;

    [Header("Definitions")]
    [SerializeField] private GrenadeDefinition fragDefinition;
    [SerializeField] private GrenadeDefinition flashbangDefinition;

    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public Button restartButton;
    public Button returnToMenuButton;

    [Header("Win Screen")]
    public Button winRestartButton;
    public Button winMenuButton;

    [Header("Pause Screen")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMenuButton;
    public Button quitButton;

    private ShieldBlocker playerShield;
    private GrenadeSlotController playerGrenades;
    private LoadoutController playerLoadout;
    private UnityEngine.InputSystem.InputAction pauseAction;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);

        if (winRestartButton != null) winRestartButton.onClick.AddListener(OnRestartClicked);
        if (winMenuButton != null) winMenuButton.onClick.AddListener(OnReturnToMenuClicked);

        if (winRestartButton != null) winRestartButton.onClick.AddListener(OnRestartClicked);
        if (winMenuButton != null) winMenuButton.onClick.AddListener(OnReturnToMenuClicked);

        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(OnRestartClicked);
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        pauseAction = UnityEngine.InputSystem.InputSystem.actions.FindAction("Pause");
        if (pauseAction != null) pauseAction.performed += OnPausePerformed;

        ResolvePlayerRefs();
        SubscribePlayerEvents();
        RefreshAll();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += RefreshAll;
        GameManager.OnPauseStateChanged += OnPauseStateChanged;
        // In case player was found later or script re-enabled
        if (playerLoadout == null) ResolvePlayerRefs();
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= RefreshAll;
        GameManager.OnPauseStateChanged -= OnPauseStateChanged;
        UnsubscribePlayerEvents();
        if (pauseAction != null) pauseAction.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    private void OnResumeClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.QuitGame();
    }

    private void ResolvePlayerRefs()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        playerShield = player.GetComponent<ShieldBlocker>();
        playerGrenades = player.GetComponent<GrenadeSlotController>();
        playerLoadout = player.GetComponent<LoadoutController>();
    }

    private void SubscribePlayerEvents()
    {
        if (playerShield != null) playerShield.ShieldChargesChanged += OnShieldChargesChanged;
        if (playerGrenades != null)
        {
            playerGrenades.GrenadeCountsChanged += OnGrenadeCountsChanged;
            playerGrenades.SelectedGrenadeChanged += OnSelectedGrenadeChanged;
        }
        if (playerLoadout != null)
        {
            playerLoadout.WeaponChanged += OnWeaponChanged;
            playerLoadout.ShieldEquipped += OnShieldEquipped;
            playerLoadout.ShieldRemoved += OnShieldRemoved;
            playerLoadout.ActiveSlotChanged += OnActiveSlotChanged;
        }
    }

    private void UnsubscribePlayerEvents()
    {
        if (playerShield != null) playerShield.ShieldChargesChanged -= OnShieldChargesChanged;
        if (playerGrenades != null)
        {
            playerGrenades.GrenadeCountsChanged -= OnGrenadeCountsChanged;
            playerGrenades.SelectedGrenadeChanged -= OnSelectedGrenadeChanged;
        }
        if (playerLoadout != null)
        {
            playerLoadout.WeaponChanged -= OnWeaponChanged;
            playerLoadout.ShieldEquipped -= OnShieldEquipped;
            playerLoadout.ShieldRemoved -= OnShieldRemoved;
            playerLoadout.ActiveSlotChanged -= OnActiveSlotChanged;
        }
    }

    private void RefreshAll()
    {
        RefreshEchoes();
        RefreshObjective();
        RefreshLoadout();
        UpdateStatePanels();
    }

    private void RefreshEchoes()
    {
        if (echoesLeftText == null || GameManager.Instance == null) return;

        int echoes = GameManager.Instance.reincarnations;
        echoesLeftText.text = $"ECHOES LEFT: {echoes}";

        if (echoes >= 2) echoesLeftText.color = normalColor;
        else if (echoes == 1) echoesLeftText.color = warningColor;
        else echoesLeftText.color = dangerColor;
    }

    private void RefreshObjective()
    {
        if (objectiveValueText == null || GameManager.Instance == null) return;
        objectiveValueText.text = GameManager.Instance.currentObjective;
    }

    private void RefreshLoadout()
    {
        if (playerLoadout == null) return;

        UpdatePrimarySlot();
        UpdateWeaponSlot(secondarySlot, playerLoadout.SecondaryWeapon, playerLoadout.ActiveSlot == WeaponSlot.Secondary);

        RefreshGrenadeSlot(fragSlot, GrenadeType.Frag);
        RefreshGrenadeSlot(flashbangSlot, GrenadeType.Flashbang);
    }

    private void UpdatePrimarySlot()
    {
        if (primarySlot == null || primarySlot.root == null) return;

        bool isActive = playerLoadout.ActiveSlot == WeaponSlot.Primary;
        ShieldDefinition shield = playerLoadout.PrimaryShield;

        if (shield != null)
        {
            RenderShieldInPrimary(shield, isActive);
            return;
        }

        UpdateWeaponSlot(primarySlot, playerLoadout.PrimaryWeapon, isActive);
    }

    private void RenderShieldInPrimary(ShieldDefinition shield, bool isActive)
    {
        bool shouldHighlight = isActive;
        if (primarySlot.border != null) primarySlot.border.enabled = shouldHighlight;
        if (primarySlot.canvasGroup != null) primarySlot.canvasGroup.alpha = shouldHighlight ? 1f : 0.6f;

        if (primarySlot.icon != null)
        {
            primarySlot.icon.preserveAspect = true;
            if (shield.icon != null)
            {
                primarySlot.icon.sprite = shield.icon;
                primarySlot.icon.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"[UIManager] ShieldDefinition '{shield.displayName}' is missing an icon.", shield);
            }
        }

        if (primarySlot.label != null)
        {
            primarySlot.label.text = shield.displayName.ToUpper();
            primarySlot.label.color = shouldHighlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
        }

        if (primarySlot.shieldChargesText != null)
        {
            primarySlot.shieldChargesText.gameObject.SetActive(true);
            if (playerShield != null)
            {
                primarySlot.shieldChargesText.text = $"{playerShield.CurrentCharges}/{playerShield.MaxCharges}";
            }
        }
    }

    private void UpdateWeaponSlot(WeaponSlotView slotView, WeaponDefinition weapon, bool isActive)
    {
        if (slotView == null || slotView.root == null) return;

        bool hasWeapon = weapon != null;

        bool shouldHighlight = isActive && hasWeapon;
        if (slotView.border != null) slotView.border.enabled = shouldHighlight;
        if (slotView.canvasGroup != null) slotView.canvasGroup.alpha = shouldHighlight ? 1f : 0.6f;

        if (hasWeapon)
        {
            if (slotView.icon != null)
            {
                slotView.icon.preserveAspect = true;
                if (weapon.icon != null)
                {
                    slotView.icon.sprite = weapon.icon;
                    slotView.icon.color = Color.white;
                }
                else
                {
                    Debug.LogWarning($"[UIManager] WeaponDefinition '{weapon.displayName}' is missing an icon.", weapon);
                }
            }
            if (slotView.label != null)
            {
                slotView.label.text = weapon.displayName.ToUpper();
                slotView.label.color = shouldHighlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            }

            if (slotView.shieldChargesText != null)
            {
                slotView.shieldChargesText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (slotView.label != null) slotView.label.text = "EMPTY";
            if (slotView.icon != null)
            {
                slotView.icon.preserveAspect = true;
                slotView.icon.color = new Color(1, 1, 1, 0.2f);
            }
            if (slotView.shieldChargesText != null) slotView.shieldChargesText.gameObject.SetActive(false);
            if (slotView.canvasGroup != null) slotView.canvasGroup.alpha = 0.3f;
            if (slotView.border != null) slotView.border.enabled = false;
        }
    }

    private void RefreshGrenadeSlot(GrenadeSubSlotView slotView, GrenadeType type)
    {
        if (slotView == null || playerGrenades == null) return;

        int count = playerGrenades.GetCount(type);
        bool isSelected = playerGrenades.SelectedGrenadeType == type;
        
        if (slotView.countText != null) slotView.countText.text = $"x{count}";
        
        // Highlight
        if (slotView.border != null) slotView.border.enabled = isSelected;
        
        float alpha = 1f;
        if (count == 0) alpha = 0.4f;
        else if (!isSelected) alpha = 0.7f;
        
        if (slotView.canvasGroup != null) slotView.canvasGroup.alpha = alpha;

        // Set icon/label from definitions
        GrenadeDefinition def = (type == GrenadeType.Frag) ? fragDefinition : flashbangDefinition;
        if (def != null)
        {
            if (slotView.icon != null)
            {
                slotView.icon.preserveAspect = true;
                if (def.icon != null) slotView.icon.sprite = def.icon;
                else Debug.LogWarning($"[UIManager] GrenadeDefinition '{def.displayName}' is missing an icon.", def);
            }
            if (slotView.label != null)
            {
                slotView.label.text = def.displayName.ToUpper();
                // slotView.label.textWrappingMode = TextWrappingModes.NoWrap;
            }
}
    }

    private void OnShieldChargesChanged(int current, int max)
    {
        if (playerLoadout != null && playerLoadout.PrimaryShield != null)
        {
            UpdatePrimarySlot();
        }
    }

    private void OnShieldEquipped(ShieldDefinition _)
    {
        UpdatePrimarySlot();
    }

    private void OnShieldRemoved()
    {
        UpdatePrimarySlot();
    }

    private void OnActiveSlotChanged(WeaponSlot _)
    {
        RefreshLoadout();
    }

    private void OnGrenadeCountsChanged(GrenadeType type, int count)
    {
        if (type == GrenadeType.Frag) RefreshGrenadeSlot(fragSlot, type);
        else if (type == GrenadeType.Flashbang) RefreshGrenadeSlot(flashbangSlot, type);
    }

    private void OnSelectedGrenadeChanged(GrenadeType type)
    {
        RefreshGrenadeSlot(fragSlot, GrenadeType.Frag);
        RefreshGrenadeSlot(flashbangSlot, GrenadeType.Flashbang);
    }

    private void OnWeaponChanged(WeaponDefinition weapon)
    {
        RefreshLoadout();
    }

    private void UpdateStatePanels()
    {
        if (GameManager.Instance == null) return;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(GameManager.Instance.isGameOver && !GameManager.Instance.isWin);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(GameManager.Instance.isWin);
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnReturnToMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
