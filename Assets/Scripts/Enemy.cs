using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private EnemySegment enemySegmentPrefab;
    
    private List<EnemySegment> segments = new List<EnemySegment>();

    private string targetString;
    private int currentNumberOfSegments;
    private int currentActiveSegmentIndex;

    #endregion

    #region Properties


    #endregion

    private void Start()
    {
        ActivateEnemy(new Vector2(7, 4), "TESTING");
    }
    
    public void ActivateEnemy(Vector2 position, string newTarget)
    {
        if (string.IsNullOrEmpty(newTarget))
        {
            Debug.LogError("ENEMY TARGET STRING IS NULL! :(", this);
            return;
        }

        transform.position = position;
        targetString = newTarget;

        // TODO: Randomize the length instead of hardcoding the value 3 everywhere
        currentNumberOfSegments = Mathf.CeilToInt(targetString.Length / 3f);
        for(int i = 0; i < currentNumberOfSegments; i++)
        {
            if(segments.Count == i)
            {
                var newSegment = Instantiate(enemySegmentPrefab);
                newSegment.transform.localScale = transform.lossyScale;
                newSegment.transform.parent = transform;
                segments.Add(newSegment);
            }

            segments[i].EnemySegmentStateChangeEvent += OnSegmentStateChanged;

            var substring = targetString.Substring(i * 3, Mathf.Min(3, targetString.Length - (i*3)));
            segments[i].SetTargetString(substring);
        }

        currentActiveSegmentIndex = 0;
        segments[currentActiveSegmentIndex].ActivateSegment();
    }

    private void OnSegmentStateChanged(EnemySegment segment)
    {
        // Activate the next segment
        if(segment.CurrentState == EnemySegment.EnemySegmentState.Completed)
        {
            if (currentActiveSegmentIndex + 1 < currentNumberOfSegments)
            {
                segments[++currentActiveSegmentIndex].ActivateSegment();
                Debug.LogError(currentActiveSegmentIndex, segments[currentActiveSegmentIndex]);
            }
            else
            {
                // TODO: All completed, now what
            }
        }

        // TODO: Once all segments are destroyed, we need to disable them and stop listening for this event
    }

    // TODO: Implement TryMarkChar function for Alphabet to call (right now it calls EnemySegment directly), and forward it to the current active segment.
    // Remove collider from EnemySegment and put it on Enemy instead
}
