using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum DifficultyLevel { Easy, Medium, Hard }

    [Header("Difficulty")]
    public DifficultyLevel difficulty = DifficultyLevel.Medium;
    [Min(1)] public int easyLives = 7;
    [Min(1)] public int mediumLives = 5;
    [Min(1)] public int hardLives = 3;

    [Header("Game State")]
    public int reincarnations = 5;
    public string currentObjective = "Rescue all hostages";
    public bool isGameOver = false;
    public bool isWin = false;
    public bool isPaused { get; private set; } = false;
    public Transform spawnPoint;

    [Header("Objectives")]
    public int totalHostages = 0;
    public int savedHostages = 0;
    public int totalEnemies = 0;
    public int remainingEnemies = 0;

    public static event Action OnGameStateChanged;
    public static event Action OnReincarnationLost;
    public static event Action<bool> OnPauseStateChanged;

    private Health playerHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SubscribeToPlayerHealth();
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (playerHealth != null) playerHealth.Death -= OnPlayerDeath;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset transient gameplay state so a game-over/win/pause from the previous scene
        // doesn't leak forward (Time.timeScale=0 freezes the camera and player on restart).
        Time.timeScale = 1f;
        isGameOver = false;
        isWin = false;
        isPaused = false;

        bool isMenuScene = scene.name == "MainMenu";
        Cursor.visible = isMenuScene;
        Cursor.lockState = isMenuScene ? CursorLockMode.None : CursorLockMode.Confined;

        SubscribeToPlayerHealth();

        // Re-find spawn point in the new scene to fix restart/loading issues
        GameObject sp = GameObject.Find("spawnPoint");
        if (sp != null)
        {
            spawnPoint = sp.transform;
        }

        FloorManager floorManager = GetComponent<FloorManager>();
        if (floorManager != null)
        {
            floorManager.RefreshSceneReferences();
            floorManager.GoToFloor1();
        }

        InitializeObjectives();
    }

    private void InitializeObjectives()
    {
        savedHostages = 0;
        isWin = false;
        
        // Find all hostages in the scene (including inactive ones on other floors)
        Hostage[] hostages = UnityEngine.Object.FindObjectsByType<Hostage>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        totalHostages = hostages.Length;

        // Find all enemies in the scene (including inactive ones)
        EnemyDeathHandler[] enemies = UnityEngine.Object.FindObjectsByType<EnemyDeathHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        totalEnemies = enemies.Length;
        remainingEnemies = totalEnemies;

        UpdateObjectiveText();
    }

    public void HostageSaved()
    {
        if (isGameOver || isWin) return;

        savedHostages++;
        UpdateObjectiveText();
        CheckWinCondition();
    }

    public void EnemyKilled()
    {
        if (isGameOver || isWin) return;

        remainingEnemies--;
        UpdateObjectiveText();
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (savedHostages >= totalHostages && remainingEnemies <= 0 && (totalHostages > 0 || totalEnemies > 0))
        {
            Win();
        }
    }

    private void UpdateObjectiveText()
    {
        currentObjective = $"RESCUE HOSTAGES: {savedHostages}/{totalHostages}\nENEMIES REMAINING: {remainingEnemies}";
        OnGameStateChanged?.Invoke();
    }

    private void Win()
    {
        isWin = true;
        isGameOver = true; // Use game over logic for pausing/locking
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnGameStateChanged?.Invoke();
        Debug.Log("Mission Accomplished: All hostages rescued.");
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        // Handle cursor
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Confined;

        OnPauseStateChanged?.Invoke(isPaused);
    }

    private void SubscribeToPlayerHealth()
    {
        if (playerHealth != null) playerHealth.Death -= OnPlayerDeath;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        playerHealth = player.GetComponent<Health>();
        if (playerHealth != null) playerHealth.Death += OnPlayerDeath;
    }

    private void OnPlayerDeath(DamageInfo _)
    {
        LoseReincarnation();
    }

    public void StartGame()
    {
        // Set lives based on selected difficulty
        switch (difficulty)
        {
            case DifficultyLevel.Easy: reincarnations = easyLives; break;
            case DifficultyLevel.Medium: reincarnations = mediumLives; break;
            case DifficultyLevel.Hard: reincarnations = hardLives; break;
        }

        isGameOver = false;
        SceneManager.LoadScene("SampleScene");
    }

    public void LoseReincarnation()
    {
        if (isGameOver) return;

        reincarnations--;
        OnReincarnationLost?.Invoke();

        if (reincarnations <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn logic: move player to spawn, reset velocity and health
            RespawnPlayer();
            OnGameStateChanged?.Invoke();
        }
    }

    public void RespawnPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // Move to spawn point if assigned
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }

        // Reset camera position to avoid slow slide from previous death location
        CameraPivotFollow camFollow = UnityEngine.Object.FindFirstObjectByType<CameraPivotFollow>();
        if (camFollow != null)
        {
            camFollow.SnapToTarget();
        }

        // Reset floor view to Floor 1
        FloorManager floorManager = GetComponent<FloorManager>();
        if (floorManager != null)
        {
            floorManager.GoToFloor1();
        }

        // Reset physics (if any)
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Health health = player.GetComponent<Health>();
        if (health != null) health.ResetHealth();
    }

    private void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnGameStateChanged?.Invoke();
        Debug.Log("Game Over: No more echoes.");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
