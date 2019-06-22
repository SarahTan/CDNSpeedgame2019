using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private GameObject wallPrefab;

    private void Awake()
    {
        // We want this to persist across all the different scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        if(newScene.name == "main")
        {
            SetUpWalls();
        }
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
}
