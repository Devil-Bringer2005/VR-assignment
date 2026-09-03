
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;


    [Header("Puzzle Settings")]
    [SerializeField] private int totalSockets = 3;

    private int completedSockets = 0;

    private bool puzzleCompleted = false;
    private bool gameOver = false;



    [Header("Timer Settings")]
    [SerializeField] private float puzzleTime = 60f;

    private float remainingTime;
    private bool timerRunning = false;

   

    [Header("Door")]
    [SerializeField] private Animator doorAnimator;

    [Header("World Space UI")]

    [SerializeField]
    private TMP_Text objectsRemainingText;

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private TMP_Text taskCompletedText;

    [SerializeField]
    private TMP_Text gameOverText;

   

    [Header("UI Buttons")]

    [Tooltip("Reset button. Always visible during the task.")]
    [SerializeField] private GameObject resetButton;

    [Tooltip("Restart button. Only appears after Game Over.")]
    [SerializeField] private GameObject restartButton;

   

    private void Awake()
    {
        Instance = this;

        // Initial puzzle state
        completedSockets = 0;
        puzzleCompleted = false;
        gameOver = false;

        // Start timer
        remainingTime = puzzleTime;
        timerRunning = true;

        // Hide result messages
        if (taskCompletedText != null)
        {
            taskCompletedText.gameObject.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        // Reset is ALWAYS visible
        if (resetButton != null)
        {
            resetButton.SetActive(true);
        }

        // Restart is ONLY visible after Game Over
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        UpdateObjectsRemainingUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        if (puzzleCompleted || gameOver)
            return;

        // Countdown
        remainingTime -= Time.deltaTime;

        // Prevent negative timer
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimerUI();

            GameOver();

            return;
        }

        UpdateTimerUI();
    }

    public void SocketCompleted()
    {
        // Don't accept placement after Game Over
        // or after puzzle completion.
        if (gameOver || puzzleCompleted)
            return;

        completedSockets++;

        // Prevent accidental over-counting
        completedSockets =
            Mathf.Clamp(
                completedSockets,
                0,
                totalSockets
            );

        Debug.Log(
            "Puzzle Progress: " +
            completedSockets +
            "/" +
            totalSockets
        );

        UpdateObjectsRemainingUI();

        // Check completion
        if (completedSockets >= totalSockets)
        {
            PuzzleCompleted();
        }
    }


    private void PuzzleCompleted()
    {
        if (puzzleCompleted)
            return;

        puzzleCompleted = true;

        // Stop timer
        timerRunning = false;

        Debug.Log("PUZZLE COMPLETED!");

        // Open door
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }

        // Show Task Completed
        if (taskCompletedText != null)
        {
            taskCompletedText.gameObject.SetActive(true);
        }

        // Hide Game Over
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        // Reset remains visible
        if (resetButton != null)
        {
            resetButton.SetActive(true);
        }

        // Restart stays hidden after successful completion.
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }

        // Make sure UI says 0
        if (objectsRemainingText != null)
        {
            objectsRemainingText.text =
                "Tasks Remaining: 0";
        }
    }

    private void GameOver()
    {
        if (gameOver)
            return;

        if (puzzleCompleted)
            return;

        gameOver = true;

        timerRunning = false;

        Debug.Log("GAME OVER!");

        // Show Game Over
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        // Hide Task Completed
        if (taskCompletedText != null)
        {
            taskCompletedText.gameObject.SetActive(false);
        }

        // Reset remains visible
        if (resetButton != null)
        {
            resetButton.SetActive(true);
        }

        // Restart appears
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
    }

   
    private void UpdateObjectsRemainingUI()
    {
        if (objectsRemainingText == null)
            return;

        int remaining =
            totalSockets - completedSockets;

        remaining = Mathf.Max(
            remaining,
            0
        );

        objectsRemainingText.text =
            "Objects Remaining: " + remaining;
    }

   

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int minutes =
            Mathf.FloorToInt(
                remainingTime / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                remainingTime % 60f
            );

        timerText.text = string.Format(
            "Time: {0:00}:{1:00}",
            minutes,
            seconds
        );
    }


    public void ResetPuzzle()
    {
        Debug.Log("RESET BUTTON PRESSED");

        // Reload the entire scene
        ReloadCurrentScene();
    }

   

    public void RestartTask()
    {
        Debug.Log("RESTART BUTTON PRESSED");

        // Reload the entire scene
        ReloadCurrentScene();
    }

    

    private void ReloadCurrentScene()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }


    public void StartTimer()
    {
        if (puzzleCompleted || gameOver)
            return;

        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    
    public int GetCompletedSockets()
    {
        return completedSockets;
    }

    public int GetRemainingObjects()
    {
        return Mathf.Max(
            totalSockets - completedSockets,
            0
        );
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public bool IsPuzzleCompleted()
    {
        return puzzleCompleted;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }
}
