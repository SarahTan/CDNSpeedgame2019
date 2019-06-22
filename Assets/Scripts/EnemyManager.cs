using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // TODO: Create a LayerMasks class to handle this so we don't hardcode values like this!
    public static int ENEMY_LAYERMASK = 1 << 8;             // 8: Layer of "Enemy"
    public static int ENEMY_SEGMENT_LAYERMASK = 1 << 9;     // 9: Layer of "EnemySegment"

    #region Fields

    [SerializeField]
    private Enemy enemyPrefab;

    [SerializeField]
    private List<string> enemyStrings = new List<string>();

    private List<Enemy> enemies = new List<Enemy>();

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (enemyStrings.Count() == 0)
        {
            Debug.LogError("ERROR! No enemy strings have been set on EnemyManager!", this);
            return;
        }

        for(int i = enemyStrings.Count-1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(enemyStrings[i]))
            {
                enemyStrings.RemoveAt(i);
            }
            else if(!enemyStrings[i].OnlyContainsAlphabetsAndSpaces())
            {
                Debug.LogError($"ERROR! Enemy strings can only contain alphabets and spaces! Removing \"{enemyStrings[i]}\"");
                enemyStrings.RemoveAt(i);
            }
            else
            {
                enemyStrings[i] = enemyStrings[i].ToUpperInvariant();
            }
        }

        enemyStrings.OrderBy(x => x.Length);

        StartCoroutine(SpawnEnemiesRoutine());
    }

    #endregion

    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            // TODO: Might want to use states instead of checking isActiveAndEnabled
            var enemy = enemies.FirstOrDefault(e => !e.isActiveAndEnabled);
            if (enemy == null)
            {
                enemy = Instantiate(enemyPrefab);
                enemy.transform.parent = transform;
                enemies.Add(enemy);
            }

            // Word length increases as the game goes on
            var minimumIndexForEnemyText = Time.time/20 - 2;
            var maximumIndexForEnemyText = (enemyStrings.Count - 1)/2 + (Time.time / 20);
            if (minimumIndexForEnemyText < 0)
            {
                minimumIndexForEnemyText = 0;
            }
            if (minimumIndexForEnemyText > (enemyStrings.Count - 1) / 2)
            {
                minimumIndexForEnemyText = (enemyStrings.Count - 1) / 2;
            }
            if (maximumIndexForEnemyText > enemyStrings.Count - 1)
            {
                maximumIndexForEnemyText = enemyStrings.Count - 1;
            }

            enemy.ActivateEnemy(Utils.GetRandomPositionOnScreen(),
                                enemyStrings[Random.Range((int)minimumIndexForEnemyText, (int)maximumIndexForEnemyText)]);

            /* Enemies spawn more rapidly:
             * As the game progresses
             * The fewer of them are on screen
             */

            var numberOfActiveEnemies = enemies.Count(e => e.isActiveAndEnabled);
            var idealNumberOfActiveEnemies = 15 + Time.time / 5;
            var baseSpawnRate = 100 - Time.time / 10;
            if (baseSpawnRate < 10)
            {
                baseSpawnRate = 10;
                Debug.Log("This should be impossible without catlike speed and reflexes.");
            }

            if (idealNumberOfActiveEnemies > numberOfActiveEnemies)
            {
                var minRate = baseSpawnRate / ((idealNumberOfActiveEnemies - numberOfActiveEnemies) * (idealNumberOfActiveEnemies - numberOfActiveEnemies));
                Debug.Log("Spawning with rate: " + minRate + ". MinIndex: " + minimumIndexForEnemyText + ". MaxIndex: " + maximumIndexForEnemyText);
                yield return (new WaitForSeconds(Random.Range(minRate, minRate + 1)));
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(baseSpawnRate, baseSpawnRate + 1));
            }

        }
    }
}
