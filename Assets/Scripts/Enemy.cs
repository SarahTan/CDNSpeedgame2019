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
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float minSpeed;
    
    private RectTransform rectTransfrom;

    private List<EnemySegment> segments = new List<EnemySegment>();

    private string targetString;
    private int currentNumberOfSegments;
    private int currentActiveSegmentIndex;
    
    #endregion

    #region Properties


    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rectTransfrom = (RectTransform)transform;
    }

    private void FixedUpdate()
    {
        // Clamp the speed
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        else if(rb.velocity.sqrMagnitude < minSpeed * minSpeed)
        {
            rb.velocity = rb.velocity.normalized * minSpeed;
        }
    }

    #endregion

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
                newSegment.transform.SetParent(transform, worldPositionStays: false);
                segments.Add(newSegment);
            }

            segments[i].EnemySegmentStateChangeEvent += OnSegmentStateChanged;

            var substring = targetString.Substring(i * 3, Mathf.Min(3, targetString.Length - (i*3)));
            segments[i].SetTargetString(substring);
        }

        currentActiveSegmentIndex = 0;
        segments[currentActiveSegmentIndex].ActivateSegment();
        
        // Give the enemy an instantaneous force and let physics handle the rest of its movement
        var direction = Utils.GetRandomUnitVector();
        var speed = Random.Range(minSpeed, maxSpeed);
        rb.AddForce(direction * speed, ForceMode2D.Impulse);

        // Adjust collider size
        StartCoroutine(UpdateColliderSize());

        IEnumerator UpdateColliderSize()
        {
            // Need to wait for the GUI to render first, so that the rect transform will have the updated bounds
            yield return new WaitForEndOfFrame();
            collider.size = rectTransfrom.sizeDelta;
        }
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
            }
            else
            {
                // TODO: All completed, now what
            }
        }
        else if(segment.CurrentState == EnemySegment.EnemySegmentState.Destroyed)
        {
            // TODO: Once all segments are destroyed, we need to disable them and stop listening for this event
        }

    }
}
