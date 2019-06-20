using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
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
            else
            {
                enemyStrings[i] = enemyStrings[i].ToUpperInvariant();
            }
        }

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

            // TODO: Don't hard code the min and max position values, calculate based on screen size
            enemy.ActivateEnemy(new Vector2(Random.Range(-7f, 7f), Random.Range(-4.5f, 4.5f)), enemyStrings[Random.Range(0, enemyStrings.Count-1)]);

            // TODO: Figure out a smarter way of spawning enemies based on how many are active on screen
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }
}
