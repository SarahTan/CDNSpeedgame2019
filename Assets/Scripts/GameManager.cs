using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region Statics

    public static PlayerController Player;
    public static EnemyManager EnemyManager;
    public static AlphabetManager AlphabetManager;

    public static event Action<bool> GamePausedEvent = null;

    #endregion

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

    [SerializeField]
    private AudioSource[] audioSources;

    [SerializeField]
    public AudioSource bgMusic;

    [SerializeField]
    private Texture2D fadeOutTexture;
    private float fadeOutTime = 0;

    private GameObject wallsParent;

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
    
    //private void OnGUI()
    //{
    //    if (Player?.HitPoints <= 0)
    //    {
    //        var fadeOutAlpha = Mathf.SmoothStep(0, 0.95f, fadeOutTime / 15);
    //        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
    //            fadeOutTexture, ScaleMode.StretchToFill, true, 0, 
    //            new Color(0, 0, 0, fadeOutAlpha), 0, 0);

    //        fadeOutTime = fadeOutTime + Time.deltaTime;
    //    }
    //}


    private void Awake()
    {
        // We want this to persist across all the different scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneLoaded;
        Cloud.CloudDestroyedEvent += OnEnemyDestroyed;

        SceneManager.LoadScene("MainMenu");
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
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Main");
        }
    }

    #region Events

    private Coroutine updateScoreRoutine = null;

    private void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        if(newScene.name == "Main")
        {
            SetUpWalls();
            gameRunningUI.SetActive(true);
            gamePausedUI.SetActive(false);
            CurrentScore = 0;
            PlayerController.HitEnemyEvent += OnPlayerHitEnemy;
            SafeStartRunUpdateScore();
        }

        if(oldScene.name == "Main")
        {
            gameRunningUI.SetActive(false);
            gamePausedUI.SetActive(false);
            PlayerController.HitEnemyEvent -= OnPlayerHitEnemy;
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

    private void UpdateScore(int segmentLength, int stringLength)
    {
        // Destroying things gets more score later in the game
        CurrentScore += segmentLength + stringLength + scoreIncrement;
    }

    private void PauseGame(bool pause)
    {
        // Can't pause on death
        if (Player?.HitPoints <= 0)
        {
            return;
        }

        ClickButtonSound();
        Time.timeScale = pause ? 0 : 1;
        gamePausedUI.SetActive(pause);

        GamePausedEvent?.Invoke(pause);
        if (pause)
        {
            bgMusic.volume = 0.2f;
        }
        else
        {
            bgMusic.volume = 0.6f;
        }
    }

    #region Buttons

    public void Button_Resume()
    {
        ClickButtonSound();
        PauseGame(false);
    }

    public void Button_Restart()
    {
        ClickButtonSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
