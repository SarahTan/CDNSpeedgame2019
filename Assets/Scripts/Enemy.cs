using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private EnemySegment enemySegmentPrefab;
    [SerializeField]
    private BoxCollider2D collider;

    private RectTransform rectTransfrom;

    private List<EnemySegment> segments = new List<EnemySegment>();

    private string targetString;
    private int currentNumberOfSegments;
    private int currentActiveSegmentIndex;

    #endregion

    #region Properties


    #endregion

    private void Awake()
    {
        rectTransfrom = (RectTransform)transform;
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
                
        // Adjust collider size
        StartCoroutine(UpdateColliderSize());
    }
    
    private IEnumerator UpdateColliderSize()
    {
        // Need to wait for the GUI to render first, so that the rect transform will have the updated bounds
        yield return new WaitForEndOfFrame();
        collider.size = rectTransfrom.sizeDelta;
    }
    public void OnAlphabetImpact(char charToTry)
    {
        segments[currentActiveSegmentIndex].TryMarkChar(charToTry);
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
}
