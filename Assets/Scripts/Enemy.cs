using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Statics

    public static event Action<int, int> EnemyDestroyedEvent;

    #endregion

    #region Fields

    [SerializeField]
    private EnemySegment enemySegmentPrefab;
    [SerializeField]
    private new BoxCollider2D collider;
    [SerializeField]
    private Rigidbody2D rb;

    private RectTransform rectTransfrom;

    private List<EnemySegment> segments = new List<EnemySegment>();

    private string targetString;
    private int currentNumberOfSegments;
    private int currentActiveSegmentIndex;
    private int destroyedSegmentCount;
    private Vector3 originalScale;

    #endregion

    #region Properties


    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rectTransfrom = (RectTransform)transform;
        originalScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        // Clamp the speed
        if (rb.velocity.sqrMagnitude > EnemyManager.Instance.MaxSpeed * EnemyManager.Instance.MaxSpeed)
        {
            rb.velocity = rb.velocity.normalized * EnemyManager.Instance.MaxSpeed;
        }
        else if (rb.velocity.sqrMagnitude < EnemyManager.Instance.MinSpeed * EnemyManager.Instance.MinSpeed)
        {
            rb.velocity = rb.velocity.normalized * EnemyManager.Instance.MinSpeed;
        }
    }

    #endregion

    private IEnumerator RunSpawnAnimation()
    {
        // Scale up
        var startTime = Time.time;
        while (transform.localScale.x < originalScale.x)
        {
            yield return null;
            var elapsedTime = Time.time - startTime;

            // TODO: Use an animation curve for snappier feeling animation
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsedTime / EnemyManager.Instance.SpawnDuration);
        }
        transform.localScale = originalScale;

        foreach (var segment in segments)
        {
            segment.CurrentState = EnemySegment.EnemySegmentState.Inactive;
        }

        // Wait a frame so the GUI has updated, and the rect transform will have the latest bounds based on the GUI
        yield return null;
        collider.size = rectTransfrom.sizeDelta;
        collider.enabled = true;

        // Give the enemy an instantaneous force and let physics handle the rest of its movement
        var direction = Utils.GetRandomUnitVector();
        var speed = UnityEngine.Random.Range(EnemyManager.Instance.MinSpeed, EnemyManager.Instance.MaxSpeed);
        rb.AddForce(direction * speed, ForceMode2D.Impulse);
    }
    
    public void ActivateEnemy(Vector2 position, string newTarget)
    {
        if (string.IsNullOrEmpty(newTarget))
        {
            Debug.LogError("ENEMY TARGET STRING IS NULL! :(", this);
            return;
        }

        transform.position = position;
        transform.localScale = Vector3.zero;
        collider.enabled = false;
        targetString = newTarget;
        
        currentNumberOfSegments = Mathf.CeilToInt(targetString.Length / 3f);
        for (int i = 0; i < currentNumberOfSegments; i++)
        {
            if (segments.Count == i)
            {
                var newSegment = Instantiate(enemySegmentPrefab);
                newSegment.transform.SetParent(transform, worldPositionStays: false);
                segments.Add(newSegment);
            }

            segments[i].EnemySegmentStateChangeEvent += OnSegmentStateChanged;

            var substring = targetString.Substring(i * 3, Mathf.Min(3, targetString.Length - (i * 3)));
            segments[i].SetTargetString(substring);
        }

        destroyedSegmentCount = 0;
        currentActiveSegmentIndex = 0;
        
        gameObject.SetActive(true);

        StartCoroutine(RunSpawnAnimation());
    }

    public void OnAlphabetImpact(char charToTry)
    {
        segments[currentActiveSegmentIndex].TryMarkChar(charToTry);
    }

    private void OnSegmentStateChanged(EnemySegment segment)
    {
        switch (segment.CurrentState)
        {
            // Activate the next segment
            case EnemySegment.EnemySegmentState.Completed:

                if (currentActiveSegmentIndex + 1 < currentNumberOfSegments)
                {
                    segments[++currentActiveSegmentIndex].ActivateSegment();
                }
                break;

            case EnemySegment.EnemySegmentState.Destroyed:

                destroyedSegmentCount++;

                if (destroyedSegmentCount == currentNumberOfSegments)
                {
                    DestroyEnemy();
                }
                break;
            case EnemySegment.EnemySegmentState.Collided:
                // TODO: Decide if we destroy the enemy or break off earlier pieces
                DestroyEnemy();
                break;
        }
    }

    public void DestroyEnemy()
    {
        for (int i = 0; i < currentNumberOfSegments; i++)
        {
            segments[i].EnemySegmentStateChangeEvent -= OnSegmentStateChanged;
        }
        collider.enabled = false;
        gameObject.SetActive(false);

        EnemyDestroyedEvent?.Invoke(currentNumberOfSegments, targetString.Length);
    }
}
