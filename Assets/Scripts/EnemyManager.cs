﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    #region Fields

    [SerializeField]
    private Cloud cloudPrefab;
    [SerializeField]
    private ShootingStar starPrefab;

    [SerializeField]
    private List<string> cloudStrings = new List<string>();

    [Header("Stars")]
    public float starSpeed;

    [Header("Clouds")]
    [Header("Spawning")]
    public float SpawnDuration;

    [Header("Speed")]
    public float MaxSpeed;
    public float MinSpeed;

    [Header("Colors")]
    public Color MarkedColor;
    public Color UnmarkedColor;
    public Color DestroyedColor;

    [HideInInspector]
    public string MarkedColorHex;
    [HideInInspector]
    public string UnmarkedColorHex;
    [HideInInspector]
    public string DestroyedColorHex;

    private List<Cloud> clouds = new List<Cloud>();
    private List<ShootingStar> stars = new List<ShootingStar>();
    
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        MarkedColorHex = ColorUtility.ToHtmlStringRGBA(MarkedColor);
        UnmarkedColorHex = ColorUtility.ToHtmlStringRGBA(UnmarkedColor);
        DestroyedColorHex = ColorUtility.ToHtmlStringRGBA(DestroyedColor);
    }

    private void Start()
    {
        if (cloudStrings.Count() == 0)
        {
            Debug.LogError("ERROR! No cloud strings have been set on EnemyManager!", this);
            return;
        }

        for(int i = cloudStrings.Count-1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(cloudStrings[i]))
            {
                cloudStrings.RemoveAt(i);
            }
            else if(!cloudStrings[i].OnlyContainsAlphabetsAndSpaces())
            {
                Debug.LogError($"ERROR! Cloud strings can only contain alphabets and spaces! Removing \"{cloudStrings[i]}\"");
                cloudStrings.RemoveAt(i);
            }
            else
            {
                cloudStrings[i] = cloudStrings[i].ToUpperInvariant();
            }
        }

        cloudStrings.Sort((x, y) => x.Length.CompareTo(y.Length));

        StartCoroutine(SpawnCloudsRoutine());
        StartCoroutine(SpawnStarsRoutine());
    }

    #endregion

    private IEnumerator SpawnStarsRoutine()
    {
        while (true)
        {
            var star = stars.FirstOrDefault(s => !s.isActiveAndEnabled);
            if (star == null)
            {
                star = Instantiate(starPrefab, transform);
                stars.Add(star);
            }

            star.ActivateStar(Utils.GetRandomPositionJustOutsideScreen());

            // Spawn rate is determined solely by the game time
            var spawnRate = 0.15f - Time.timeSinceLevelLoad / 1000f;
            if (spawnRate < 0)
            {
                spawnRate = 0;
            }

            yield return new WaitForSeconds(0.075f + spawnRate);
        }
    }

    private IEnumerator SpawnCloudsRoutine()
    {
        while (true)
        {
            var cloud = clouds.FirstOrDefault(e => !e.isActiveAndEnabled);
            if (cloud == null)
            {
                cloud = Instantiate(cloudPrefab, transform);
                clouds.Add(cloud);
            }

            // Word length increases as the game goes on
            var minimumIndexForCloudText = Time.timeSinceLevelLoad / 20 - 2;
            var maximumIndexForCloudText = (cloudStrings.Count - 1)/2 + (Time.timeSinceLevelLoad / 20);
            if (minimumIndexForCloudText < 0)
            {
                minimumIndexForCloudText = 0;
            }
            if (minimumIndexForCloudText > (cloudStrings.Count - 1) / 2)
            {
                minimumIndexForCloudText = (cloudStrings.Count - 1) / 2;
            }
            if (maximumIndexForCloudText > cloudStrings.Count - 1)
            {
                maximumIndexForCloudText = cloudStrings.Count - 1;
            }

            cloud.ActivateCloud(Utils.GetRandomPositionOnScreen(),
                                cloudStrings[Random.Range((int)minimumIndexForCloudText, (int)maximumIndexForCloudText)]);

            /* Clouds spawn more rapidly:
             * As the game progresses
             * The fewer of them are on screen
             */

            var numberOfActiveClouds = clouds.Count(e => e.isActiveAndEnabled);
            var idealNumberOfActiveClouds = 20 + Time.timeSinceLevelLoad / 5;
            var baseSpawnRate = 100 - Time.timeSinceLevelLoad / 10;
            if (baseSpawnRate < 10)
            {
                baseSpawnRate = 10;
                Debug.Log("This should be impossible without catlike speed and reflexes.");
            }

            if (idealNumberOfActiveClouds > numberOfActiveClouds)
            {
                var minRate = baseSpawnRate / ((idealNumberOfActiveClouds - numberOfActiveClouds) * (idealNumberOfActiveClouds - numberOfActiveClouds));
                Debug.Log("Spawning with rate: " + minRate + ". MinIndex: " + minimumIndexForCloudText + ". MaxIndex: " + maximumIndexForCloudText);
                yield return (new WaitForSeconds(Random.Range(minRate, minRate + 1)));
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(baseSpawnRate, baseSpawnRate + 1));
            }

        }
    }
}
