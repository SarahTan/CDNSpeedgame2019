using System;
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

    [SerializeField]
    private int maxTime;

    [Header("Game Running")]
    [SerializeField]
    private GameObject gamePausedUI;

    [Header("Main Menu")]
    [SerializeField]
    private GameObject mainMenuUI;

    #endregion

    #region Properties

    private int _currentScore = 0;
    private int scoreIncrement = 1;
    private int lastScoreUpdate = 1;
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

    private void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad > lastScoreUpdate)
        {
            CurrentScore = CurrentScore + scoreIncrement;
            lastScoreUpdate = lastScoreUpdate + 4;
            scoreIncrement++;
        }

        if (Time.timeSinceLevelLoad > maxTime)
        {
            // TODO: WIN!!
            return;
        }
    }

    private void Awake()
    {
        // We want this to persist across all the different scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneLoaded;
        Cloud.CloudDestroyedEvent += OnEnemyDestroyed;
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

    public void OnPlayerHitEnemy()
    {
        CurrentScore--;
    }

    private void OnEnemyDestroyed(int segmentLength, int stringLength)
    {
        UpdateScore(segmentLength, stringLength);
    }

    #endregion

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
        // Destroying things gets more score later in the game
        CurrentScore += segmentLength + stringLength + scoreIncrement;
    }

    private void PauseGame(bool pause)
    {
        ClickButtonSound();
        Time.timeScale = pause ? 0 : 1;
        gamePausedUI.SetActive(pause);

        GamePausedEvent?.Invoke(pause);
    }

    #region Buttons

    public void Button_Resume()
    {
        ClickButtonSound();
        PauseGame(false);
    }

    public void Button_MainMenu()
    {
        ClickButtonSound();
        // TODO
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

    }

    public void BadMoveSound() // When you move badly
    {

    }

    public void HitCloudSound() // When you hit a cloud with your body
    {

    }

    public void FireLetterSound() // When you fire a letter
    {

    }

    public void LetterDisappearSound() // When a letter disappears into the reticle
    {

    }

    public void LetterHitCorrectSound() // Letter hits the right cloud
    {

    }

    public void LetterHitWrongSound() // Letter hits the wrong cloud
    {

    }

    public void DestroySegmentSound() // Destroy a cloud segment
    {

    }

    public void DestroyCloudSound() // Destroy a whole cloud
    {

    }

    public void HitStarSound() // Run into a star
    {

    }

    public void ClickButtonSound() // Click a button
    {

    }
    #endregion SoundFx
}
