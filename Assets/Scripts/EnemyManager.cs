using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{ 
    #region Fields

    [SerializeField]
    private Cloud cloudPrefab;
    [SerializeField]
    private ShootingStar starPrefab;

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

    private GameObject cloudsParent;
    private GameObject starsParent;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        GameManager.EnemyManager = this;

        MarkedColorHex = ColorUtility.ToHtmlStringRGBA(MarkedColor);
        UnmarkedColorHex = ColorUtility.ToHtmlStringRGBA(UnmarkedColor);
        DestroyedColorHex = ColorUtility.ToHtmlStringRGBA(DestroyedColor);
    }

    private void Start()
    {
        Words.Instantiate(); // TODO: This can be brought further front to the start of the game

        cloudsParent = new GameObject("Clouds");
        cloudsParent.transform.SetParent(transform);
        starsParent = new GameObject("Stars");
        starsParent.transform.SetParent(transform);

        StartCoroutine(SpawnCloudsRoutine());
        StartCoroutine(SpawnStarsRoutine());
    }
    
    private void OnDestroy()
    {
        GameManager.EnemyManager = null;
    }

    #endregion

    private IEnumerator SpawnStarsRoutine()
    {
        while (true)
        {
            var star = stars.FirstOrDefault(s => !s.isActiveAndEnabled);
            if (star == null)
            {
                star = Instantiate(starPrefab, starsParent.transform);
                stars.Add(star);
            }

            star.ActivateStar(Utils.GetRandomPositionJustOutsideScreen());

            // Spawn rate is determined solely by the game time
            var spawnRate = 0.15f - Time.timeSinceLevelLoad / 1000f;
            if (spawnRate < 0)
            {
                spawnRate = 0;
            }

            yield return new WaitForSeconds(0.05f + spawnRate);
        }
    }

    private IEnumerator SpawnCloudsRoutine()
    {
        while (true)
        {
            var cloud = clouds.FirstOrDefault(e => !e.isActiveAndEnabled);
            if (cloud == null)
            {
                cloud = Instantiate(cloudPrefab, cloudsParent.transform);
                clouds.Add(cloud);
            }

            // Word length increases as the game goes on
            // At 120 seconds, we should be at maximum difficulty
            float divisor = 240f / Words.WordList.Count;
            var minimumIndexForCloudText = Time.timeSinceLevelLoad / divisor - Words.WordList.Count * 0.2f;
            var maximumIndexForCloudText = (Words.WordList.Count - 1)/2 + (Time.timeSinceLevelLoad / divisor);
            if (minimumIndexForCloudText < 0)
            {
                minimumIndexForCloudText = 0;
            }
            if (minimumIndexForCloudText > (Words.WordList.Count - 1) / 2)
            {
                minimumIndexForCloudText = (Words.WordList.Count - 1) / 2;
            }
            if (maximumIndexForCloudText > Words.WordList.Count - 1)
            {
                maximumIndexForCloudText = Words.WordList.Count - 1;
            }

            cloud.ActivateCloud(Utils.GetRandomPositionOnScreen(),
                                Words.WordList[Random.Range((int)minimumIndexForCloudText, (int)maximumIndexForCloudText)]);

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
