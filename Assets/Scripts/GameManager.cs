using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private GameObject wallPrefab;

    [Header("Main scene")]
    [SerializeField]
    private GameObject mainSceneUI;
    [SerializeField]
    private TextMeshProUGUI scoreText;

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

    private void Awake()
    {
        // We want this to persist across all the different scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneLoaded;
        Enemy.EnemyDestroyedEvent += Enemy_EnemyDestroyedEvent;
    }

    private void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        if(newScene.name == "main")
        {
            SetUpWalls();
            mainSceneUI.SetActive(true);
            CurrentScore = 0;
        }

        if(oldScene.name == "main")
        {
            mainSceneUI.SetActive(false);
        }
    }

    private void Enemy_EnemyDestroyedEvent(int segmentLength, int stringLength)
    {
        UpdateScore(segmentLength, stringLength);
    }

    private void SetUpWalls()
    {
        var extents = Utils.GetScreenExtents() + Vector2.one;

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
        CurrentScore += segmentLength + stringLength;
    }

    #region Buttons

    public void Button_Quit()
    {
        Application.Quit();
    }

    #endregion
}
