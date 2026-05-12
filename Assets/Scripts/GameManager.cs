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
    public string currentObjective = "Survive and reach the goal";
    public bool isGameOver = false;
    public bool isPaused { get; private set; } = false;
    public Transform spawnPoint;

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
        SubscribeToPlayerHealth();
        
        // Reset pause on scene load
        if (isPaused) TogglePause();

        // Re-find spawn point in the new scene to fix restart/loading issues
        GameObject sp = GameObject.Find("spawnPoint");
        if (sp != null)
        {
            spawnPoint = sp.transform;
        }
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
