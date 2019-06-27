using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region Fields

    [SerializeField]
    private GameObject wallPrefab;

    [Header("Game Running")]
    [SerializeField]
    private GameObject gameRunningUI;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [Header("Game Running")]
    [SerializeField]
    private GameObject gamePausedUI;

    [Header("Main Menu")]
    [SerializeField]
    private GameObject mainMenuUI;

    private bool oldGameIsPaused = false;

    #endregion

    #region Properties

    private int _currentScore = 0;
    public int CurrentScore
    {
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            scoreText.SetText($"Score: {_currentScore}");
        }
    }

    #endregion

    public bool GameIsPaused { get { return Time.timeScale == 0; } }

    public event Action<bool> GamePausedEvent = null;

    private void Awake()
    {
        // We want this to persist across all the different scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneLoaded;
        Cloud.CloudDestroyedEvent += OnEnemyDestroyed;
    }

    private void Update()
    {
        if(oldGameIsPaused != GameIsPaused)
        {
            GamePausedEvent?.Invoke(GameIsPaused);
        }
        oldGameIsPaused = GameIsPaused;
    }

    private void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        if(newScene.name == "main")
        {
            SetUpWalls();
            gameRunningUI.SetActive(true);
            gamePausedUI.SetActive(false);
            CurrentScore = 0;
            PlayerController.Instance.HitEnemyEvent += OnPlayerHitEnemy;
        }

        if(oldScene.name == "main")
        {
            gameRunningUI.SetActive(false);
            gamePausedUI.SetActive(false);
            PlayerController.Instance.HitEnemyEvent -= OnPlayerHitEnemy;
        }
    }

    private void OnPlayerHitEnemy()
    {
        CurrentScore--;
    }

    private void OnEnemyDestroyed(int segmentLength, int stringLength)
    {
        UpdateScore(segmentLength, stringLength);
    }

    private void SetUpWalls()
    {
        var extents = Utils.GetScreenExtents() + new Vector2(0.5f, 0.5f);

        var rightWall = Instantiate(wallPrefab);
        rightWall.transform.position = new Vector2(extents.x, 0f);
        rightWall.transform.localScale = new Vector2(1f, extents.y * 2);

        var leftWall = Instantiate(wallPrefab);
        leftWall.transform.position = new Vector2(-extents.x, 0f);
        leftWall.transform.localScale = new Vector2(1f, extents.y * 2);

        var topWall = Instantiate(wallPrefab);
        topWall.transform.position = new Vector2(0f, extents.y);
        topWall.transform.localScale = new Vector2(extents.x * 2, 1f);

        var botWall = Instantiate(wallPrefab);
        botWall.transform.position = new Vector2(0f, -extents.y);
        botWall.transform.localScale = new Vector2(extents.x * 2, 1f);
    }    

    private void UpdateScore(int segmentLength, int stringLength)
    {
        // TODO: Some smarter scoring system
        // This totally doesn't work right now. You can cheese it by just colliding into stuff and getting points
        CurrentScore += segmentLength + stringLength;
    }

    #region Buttons

    public void Button_Pause()
    {
        Time.timeScale = 0;
        gamePausedUI.SetActive(true);
    }

    public void Button_Resume()
    {
        Time.timeScale = 1;
        gamePausedUI.SetActive(false);
    }

    public void Button_MainMenu()
    {
        // TODO
    }

    public void Button_Quit()
    {
        Application.Quit();
    }

    #endregion
}
