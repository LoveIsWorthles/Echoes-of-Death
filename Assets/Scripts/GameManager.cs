using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int reincarnations = 5;
    public string currentObjective = "Survive and reach the goal";
    public bool isGameOver = false;
    public Transform spawnPoint;

    public static event Action OnGameStateChanged;
    public static event Action OnReincarnationLost;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        reincarnations = 5;
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

        // Ensure health is reset (Health also listens to OnReincarnationLost)
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }
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
