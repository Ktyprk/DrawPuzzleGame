using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameFlowController : MonoBehaviour
{
    private enum GameState
    {
        StartMenu,
        WaitingToDraw,
        DrawingPath,
        ReadyToMove,
        Moving,
        Ended
    }

    [Header("Refs")]
    [SerializeField] private InputPathDrawer pathDrawer;
    [SerializeField] private PlayerPathMover playerMover;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Levels")]
    [SerializeField] private LevelController[] levelPrefabs;
    [SerializeField] private Transform levelParent;
    [SerializeField, Min(0)] private int currentLevelIndex = 0;

    [Header("Menus")]
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameMenu;
    [SerializeField] private GameObject endGamePanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text endGameResultText;

    [Header("Messages")]
    [SerializeField] private string swipeToDrawText = "Swipe to draw";
    [SerializeField] private string holdToMoveText = "Hold to move";
    [SerializeField] private string winText = "You Win";
    [SerializeField] private string failedText = "Failed";

    private GameState currentState = GameState.StartMenu;
    private LevelController activeLevel;
    private bool gameStarted;

    private void OnEnable()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
    }

    private void OnDisable()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(StartGame);

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(LoadNextLevel);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartLevel);
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        if (!gameStarted) return;
        if (currentState == GameState.Ended) return;

        switch (currentState)
        {
            case GameState.WaitingToDraw:
                UpdateWaitingToDraw();
                break;

            case GameState.DrawingPath:
                UpdateDrawing();
                break;

            case GameState.ReadyToMove:
                UpdateReadyToMove();
                break;

            case GameState.Moving:
                UpdateMoving();
                break;
        }
    }

    private void InitializeGame()
    {
        SpawnCurrentLevel();

        gameStarted = false;
        currentState = GameState.StartMenu;

        if (startMenu != null)
            startMenu.SetActive(true);

        if (gameMenu != null)
            gameMenu.SetActive(false);

        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (instructionText != null)
            instructionText.text = string.Empty;

        if (endGameResultText != null)
            endGameResultText.text = string.Empty;

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        RefreshLevelText();
    }

    private void SpawnCurrentLevel()
    {
        if (levelPrefabs == null || levelPrefabs.Length == 0)
        {
            activeLevel = null;
            return;
        }

        if (currentLevelIndex < 0)
            currentLevelIndex = 0;

        if (currentLevelIndex >= levelPrefabs.Length)
            currentLevelIndex = 0;

        if (activeLevel != null)
            Destroy(activeLevel.gameObject);

        LevelController prefab = levelPrefabs[currentLevelIndex];
        if (prefab == null)
        {
            activeLevel = null;
            return;
        }

        activeLevel = Instantiate(prefab, levelParent);
        activeLevel.Setup(this);
        activeLevel.ResetLevel();

        if (pathDrawer != null && playerTransform != null)
            pathDrawer.SetPlayerTransform(playerTransform);

        MovePlayerToSpawnPoint();
    }

    private void MovePlayerToSpawnPoint()
    {
        if (playerTransform == null) return;
        if (activeLevel == null) return;
        if (activeLevel.PlayerSpawnPoint == null) return;

        playerTransform.position = activeLevel.PlayerSpawnPoint.position;
        playerTransform.rotation = activeLevel.PlayerSpawnPoint.rotation;
    }

    private void RefreshLevelText()
    {
        if (levelText != null)
            levelText.text = "Level " + (currentLevelIndex + 1);
    }

    private void StartGame()
    {
        ResetPlayerChildren();

        gameStarted = true;
        currentState = GameState.WaitingToDraw;

        if (startMenu != null)
            startMenu.SetActive(false);

        if (gameMenu != null)
            gameMenu.SetActive(true);

        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (instructionText != null)
            instructionText.text = swipeToDrawText;

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        if (pathDrawer != null)
            pathDrawer.ClearPath();

        if (playerMover != null)
            playerMover.StopMove();

        MovePlayerToSpawnPoint();
    }

    private void ResetPlayerChildren()
    {
        if (playerTransform == null) return;

        for (int i = playerTransform.childCount - 1; i >= 1; i--)
        {
            Destroy(playerTransform.GetChild(i).gameObject);
        }

        SetWalking(false);
    }

    private void UpdateWaitingToDraw()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pathDrawer == null) return;

            bool started = pathDrawer.BeginDraw();
            if (!started) return;

            if (instructionText != null)
                instructionText.text = string.Empty;

            currentState = GameState.DrawingPath;
        }
    }

    private void UpdateDrawing()
    {
        if (Input.GetMouseButton(0))
        {
            if (pathDrawer != null)
                pathDrawer.DrawStep();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (pathDrawer != null)
                pathDrawer.EndDraw();

            bool hasValidPath = pathDrawer != null && pathDrawer.GetPointCount() > 1;

            if (hasValidPath)
            {
                if (playerMover != null)
                    playerMover.PreparePath();

                if (instructionText != null)
                    instructionText.text = holdToMoveText;

                currentState = GameState.ReadyToMove;
            }
            else
            {
                if (pathDrawer != null)
                    pathDrawer.ClearPath();

                if (instructionText != null)
                    instructionText.text = swipeToDrawText;

                currentState = GameState.WaitingToDraw;
            }
        }
    }

    private void UpdateReadyToMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentState = GameState.Moving;
        }
    }

    private void UpdateMoving()
    {
        bool isHolding = Input.GetMouseButton(0);

        if (playerMover != null)
        {
            playerMover.TickMove(isHolding);
            SetWalking(isHolding);
        }

        if (playerMover != null && !playerMover.CanMove)
        {
            SetWalking(false);
            FinishLevel(false);
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentState = GameState.ReadyToMove;
            SetWalking(false);
        }
    }

    private void SetWalking(bool value)
    {
        if (playerMover == null) return;

        Animator[] animators = playerMover.gameObject.GetComponentsInChildren<Animator>();

        foreach (var anim in animators)
        {
            anim.SetBool("Walking", value);
        }
    }

    public void OnPlayerReachedTarget(LevelController level)
    {
        if (currentState == GameState.Ended) return;
        if (level == null) return;

        bool success = level.AllCollected;
        FinishLevel(success);
    }

    private void FinishLevel(bool success)
    {
        currentState = GameState.Ended;

        if (playerMover != null)
            playerMover.StopMove();

        SetWalking(false);

        if (instructionText != null)
            instructionText.text = string.Empty;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (endGamePanel != null)
            endGamePanel.SetActive(true);

        if (endGameResultText != null)
            endGameResultText.text = success ? winText : failedText;

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(success);

        if (restartButton != null)
            restartButton.gameObject.SetActive(!success);
    }

    private void RestartLevel()
    {
        ResetPlayerChildren();

        if (activeLevel != null)
            Destroy(activeLevel.gameObject);

        SpawnCurrentLevel();

        if (pathDrawer != null)
            pathDrawer.ClearPath();

        if (playerMover != null)
            playerMover.StopMove();

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        if (instructionText != null)
            instructionText.text = swipeToDrawText;

        if (endGameResultText != null)
            endGameResultText.text = string.Empty;

        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        currentState = GameState.WaitingToDraw;
        gameStarted = true;
    }

    private void LoadNextLevel()
    {
        ResetPlayerChildren();
        currentLevelIndex++;

        if (levelPrefabs == null || levelPrefabs.Length == 0)
            currentLevelIndex = 0;
        else if (currentLevelIndex >= levelPrefabs.Length)
            currentLevelIndex = 0;

        SpawnCurrentLevel();

        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (endGameResultText != null)
            endGameResultText.text = string.Empty;

        if (instructionText != null)
            instructionText.text = swipeToDrawText;

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        if (pathDrawer != null)
            pathDrawer.ClearPath();

        if (playerMover != null)
            playerMover.StopMove();

        currentState = GameState.WaitingToDraw;
        gameStarted = true;

        RefreshLevelText();
    }

    public void FailFromEnemy()
    {
        if (currentState == GameState.Ended)
            return;

        FinishLevel(false);
    }
}