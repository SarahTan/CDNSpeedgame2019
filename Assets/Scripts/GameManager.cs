using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region Statics

    public static PlayerController Player;
    public static EnemyManager EnemyManager;
    public static AlphabetManager AlphabetManager;

    #endregion

    #region Fields

    //*****************
    // UI 

    [Header("Game Running")]
    [SerializeField]
    private GameObject gameRunningUI;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [Header("Game Paused")]
    [SerializeField]
    private GameObject gamePausedUI;

    [Header("Game Over")]
    [SerializeField]
    private GameObject gameOverUI;
    [SerializeField]
    private TextMeshProUGUI gameOverScoreText;
    [SerializeField]
    private TextMeshProUGUI timeText;

    [Header("Main Menu")]
    [SerializeField]
    private GameObject mainMenuUI;

    [Header("Instructions")]
    [SerializeField]
    private GameObject instructionsUI;

    // ****************

    [SerializeField]
    private GameObject wallPrefab;
    [SerializeField]
    private int maxTime;

    [SerializeField]
    private AudioSource[] audioSources;

    [SerializeField]
    public AudioSource bgMusic;

    private GameObject wallsParent;
    
    #endregion

    #region Properties

    private int _currentScore = 0;
    private int scoreIncrement = 1;
    public int CurrentScore
    {
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            scoreText.SetText($"Score: {_currentScore}");
        }
    }

    private bool HasSeenTutorial
    {
        get { return PlayerPrefs.GetInt("SEEN_TUTORIAL") == 1 ? true : false; }
        set { PlayerPrefs.SetInt("SEEN_TUTORIAL", value ? 1 : 0); }
    }

    #endregion
        
    private void Awake()
    {
        // We want this to persist across all the different scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneLoaded;
        Cloud.CloudDestroyedEvent += OnEnemyDestroyed;
        PlayerController.HitEnemyEvent += OnPlayerHitEnemy;

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneLoaded;
        Cloud.CloudDestroyedEvent -= OnEnemyDestroyed;
        PlayerController.HitEnemyEvent -= OnPlayerHitEnemy;
    }

    private void ActivateUI(GameObject ui)
    {
        gameRunningUI.SetActive(ui == gameRunningUI);
        gamePausedUI.SetActive(ui == gamePausedUI);
        gameOverUI.SetActive(ui == gameOverUI);
        mainMenuUI.SetActive(ui == mainMenuUI);
        instructionsUI.SetActive(ui == instructionsUI);

        Time.timeScale = gamePausedUI.activeSelf ? 0 : 1;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            PauseGame(true);
        }
#else
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(true);
        }
#endif
    }

    #region Events


    private void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        switch (newScene.name)
        {
            case "Main":
                StartGame();
                break;

            case "MainMenu":
                ActivateUI(mainMenuUI);
                break;

            case "Instructions":
                ActivateUI(instructionsUI);
                break;
        }
    }

    public void OnPlayerHitEnemy()
    {
        CurrentScore--;
    }

    private void OnEnemyDestroyed(int segmentLength, int stringLength)
    {
        // Destroying things gets more score later in the game
        CurrentScore += segmentLength + stringLength + scoreIncrement;
    }

    #endregion

    private void SetUpWalls()
    {
        if (wallsParent == null)
        {
            wallsParent = new GameObject("Walls");
            wallsParent.transform.SetParent(transform);

            var extents = Utils.GetScreenExtents() + new Vector2(0.5f, 0.5f);

            var rightWall = Instantiate(wallPrefab, wallsParent.transform);
            rightWall.transform.position = new Vector2(extents.x, 0f);
            rightWall.transform.localScale = new Vector2(1f, extents.y * 2);

            var leftWall = Instantiate(wallPrefab, wallsParent.transform);
            leftWall.transform.position = new Vector2(-extents.x, 0f);
            leftWall.transform.localScale = new Vector2(1f, extents.y * 2);

            var topWall = Instantiate(wallPrefab, wallsParent.transform);
            topWall.transform.position = new Vector2(0f, extents.y);
            topWall.transform.localScale = new Vector2(extents.x * 2, 1f);

            var botWall = Instantiate(wallPrefab, wallsParent.transform);
            botWall.transform.position = new Vector2(0f, -extents.y);
            botWall.transform.localScale = new Vector2(extents.x * 2, 1f);
        }
    }


    private Coroutine updateScoreRoutine = null;
    private void SafeStartRunUpdateScore()
    {
        if(updateScoreRoutine != null)
        {
            StopCoroutine(updateScoreRoutine);
        }
        updateScoreRoutine = StartCoroutine(RunUpdateScore());
    }

    private IEnumerator RunUpdateScore()
    {
        while (SceneManager.GetActiveScene().name == "Main")
        {
            CurrentScore = CurrentScore + scoreIncrement;
            scoreIncrement++;

            if (Time.timeSinceLevelLoad > maxTime)
            {
                // TODO: WIN!!
                
            }

            yield return new WaitForSeconds(4f);
        }

        updateScoreRoutine = null;
    }
    
    private void LockCursor(bool locked)
    {
        Cursor.visible = locked ? false : true;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

    }

    #region Game States

    private void StartGame()
    {
        SetUpWalls();
        ActivateUI(gameRunningUI);
        CurrentScore = 0;
        SafeStartRunUpdateScore();
        LockCursor(true);
    }

    private void PauseGame(bool pause)
    {
        // Can't pause on death
        if (Player?.HitPoints <= 0)
        {
            return;
        }

        LockCursor(!pause);
        gamePausedUI.SetActive(pause);
        Time.timeScale = pause ? 0 : 1;
        bgMusic.volume = pause ? 0.2f : 0.6f;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void EndGame()
    {
        ActivateUI(gameOverUI);
        Time.timeScale = 0f;
        gameOverScoreText.SetText($"Score: {CurrentScore}");
        timeText.SetText($"Time alive: {Time.timeSinceLevelLoad.ToString("0.0")}s");

        LockCursor(false);
    }

    #endregion

    #region Buttons

    public void Button_Play()
    {
        ClickButtonSound();
        if (HasSeenTutorial)
        {
            SceneManager.LoadScene("Main");
        }
        else
        {
            SceneManager.LoadScene("Instructions");
            HasSeenTutorial = true;
        }
    }

    public void Button_Instructions()
    {
        ClickButtonSound();
        SceneManager.LoadScene("Instructions");
        HasSeenTutorial = true;
    }

    public void Button_Resume()
    {
        ClickButtonSound();
        PauseGame(false);
    }

    public void Button_Restart()
    {
        ClickButtonSound();
        RestartGame();
    }

    public void Button_MainMenu()
    {
        ClickButtonSound();
        SceneManager.LoadScene("MainMenu");
    }

    public void Button_Quit()
    {
        ClickButtonSound();
        Application.Quit();
    }

    #endregion

    #region SoundFx
    // Finding code where sound is played is confusing
    // So here's where all sound is played
    
    public void DeathSound()
    {
        audioSources[8].Play();
        bgMusic.Stop();
    }

    public void BadMoveSound() // When you move badly
    {
        audioSources[3].Play();
    }

    public void HitCloudSound() // When you hit a cloud with your body
    {
        audioSources[2].Play();
    }

    public void FireLetterSound() // When you fire a letter
    {
        // Heh. Let's not play a sound. Too much spam.
    }

    public void LetterDisappearSound() // When a letter disappears into the reticle
    {
        audioSources[5].Play(); // Same sound as letter hitting the right cloud
    }

    public void LetterHitCorrectSound() // Letter hits the right cloud
    {
        audioSources[5].Play();
    }

    public void LetterHitWrongSound() // Letter hits the wrong cloud
    {
        audioSources[4].Play();
    }

    public void DestroySegmentSound() // Destroy a cloud segment
    {
        audioSources[7].Play();
    }

    public void DestroyCloudSound() // Destroy a whole cloud
    {
        audioSources[6].Play();
    }

    public void HitStarSound() // Run into a star
    {
        audioSources[1].Play();
    }

    public void ClickButtonSound() // Click a button
    {
        audioSources[0].Play();
    }
    #endregion SoundFx
}
