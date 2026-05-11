using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    public Text reincarnationsText;
    public Text objectiveText;

    [Header("Combat HUD")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text shieldChargesText;
    [SerializeField] private Text grenadeCountText;
    [SerializeField] private Text activeWeaponText;

    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public Button returnToMenuButton;

    private Health playerHealth;
    private ShieldBlocker playerShield;
    private GrenadeSlotController playerGrenades;
    private LoadoutController playerLoadout;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }

        ResolvePlayerRefs();
        SubscribePlayerEvents();
        PrimeCombatHUD();
        UpdateHUD();
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += UpdateHUD;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= UpdateHUD;
        UnsubscribePlayerEvents();
    }

    private void ResolvePlayerRefs()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        playerHealth = player.GetComponent<Health>();
        playerShield = player.GetComponent<ShieldBlocker>();
        playerGrenades = player.GetComponent<GrenadeSlotController>();
        playerLoadout = player.GetComponent<LoadoutController>();
    }

    private void SubscribePlayerEvents()
    {
        if (playerHealth != null) playerHealth.HealthChanged += OnHealthChanged;
        if (playerShield != null) playerShield.ShieldChargesChanged += OnShieldChargesChanged;
        if (playerGrenades != null) playerGrenades.GrenadeCountsChanged += OnGrenadeCountsChanged;
        if (playerLoadout != null) playerLoadout.WeaponChanged += OnWeaponChanged;
    }

    private void UnsubscribePlayerEvents()
    {
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
        if (playerShield != null) playerShield.ShieldChargesChanged -= OnShieldChargesChanged;
        if (playerGrenades != null) playerGrenades.GrenadeCountsChanged -= OnGrenadeCountsChanged;
        if (playerLoadout != null) playerLoadout.WeaponChanged -= OnWeaponChanged;
    }

    private void PrimeCombatHUD()
    {
        if (playerHealth != null) OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        if (playerShield != null) OnShieldChargesChanged(playerShield.CurrentCharges, playerShield.MaxCharges);
        if (playerGrenades != null) OnGrenadeCountsChanged(playerGrenades.SelectedGrenadeType, playerGrenades.GetCount(playerGrenades.SelectedGrenadeType));
        if (playerLoadout != null) OnWeaponChanged(playerLoadout.GetActiveWeapon());
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthText != null) healthText.text = $"HP: {current} / {max}";
    }

    private void OnShieldChargesChanged(int current, int max)
    {
        if (shieldChargesText != null) shieldChargesText.text = $"SHIELD: {current} / {max}";
    }

    private void OnGrenadeCountsChanged(GrenadeType type, int count)
    {
        if (grenadeCountText == null || playerGrenades == null) return;
        GrenadeType selected = playerGrenades.SelectedGrenadeType;
        grenadeCountText.text = $"{selected.ToString().ToUpper()}: {playerGrenades.GetCount(selected)}";
    }

    private void OnWeaponChanged(WeaponDefinition weapon)
    {
        if (activeWeaponText == null) return;
        activeWeaponText.text = weapon != null ? $"WEAPON: {weapon.displayName}" : "WEAPON: -";
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
