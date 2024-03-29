﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    #region Statics

    public static event Action<int, int> CloudDestroyedEvent;

    #endregion

    #region Fields

    [SerializeField]
    private CloudSegment cloudSegmentPrefab;
    [SerializeField]
    private new BoxCollider2D collider;
    [SerializeField]
    private Rigidbody2D rb;

    private RectTransform rectTransfrom;

    private List<CloudSegment> segments = new List<CloudSegment>();

    private string targetString;
    private int currentNumberOfSegments;
    private int currentActiveSegmentIndex;
    private int destroyedSegmentCount;
    private Vector3 originalScale;
    
    // Used for calculating bounce angle
    Vector2 incident, reflected, normal, oldVelocity;

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
        if (rb.velocity.sqrMagnitude > GameManager.EnemyManager.MaxSpeed * GameManager.EnemyManager.MaxSpeed)
        {
            rb.velocity = rb.velocity.normalized * GameManager.EnemyManager.MaxSpeed;
        }
        else if (rb.velocity.sqrMagnitude < GameManager.EnemyManager.MinSpeed * GameManager.EnemyManager.MinSpeed)
        {
            rb.velocity = rb.velocity.normalized * GameManager.EnemyManager.MinSpeed;
        }

        oldVelocity = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Do our own bounce calculation, because Cloud has a non uniform shape, Z rotation is frozen, and values are relatively small
        // That sometimes causes Unity's physics calculation to result in a reflection angle which is parallel to the wall.
        // Also, use a cached velocity from the previous FixedUpdate since this calculation needs the velocity from before Unity did
        // its wrong calculation, and this happens at most once per frame, out of sync with the physics step
        if (collision.gameObject.layer == (int)Layers.Wall)
        {
            foreach (var contact in collision.contacts)
            {
                incident = oldVelocity;
                normal = contact.normal;
                reflected = incident - (2 * Vector2.Dot(incident, normal) * normal);

                rb.velocity = reflected;
            }
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
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsedTime / GameManager.EnemyManager.SpawnDuration);
        }
        transform.localScale = originalScale;

        foreach (var segment in segments)
        {
            segment.SetState(CloudSegment.State.Idle);
        }
        segments[0].SetState(CloudSegment.State.Active);

        // Wait a frame so the GUI has updated, and the rect transform will have the latest bounds based on the GUI
        yield return null;
        collider.size = rectTransfrom.sizeDelta;
        collider.enabled = true;

        // Give the cloud an instantaneous force and let physics handle the rest of its movement
        var direction = Utils.GetRandomUnitVector();
        var speed = UnityEngine.Random.Range(GameManager.EnemyManager.MinSpeed, GameManager.EnemyManager.MaxSpeed);
        rb.AddForce(direction * speed, ForceMode2D.Impulse);
    }
    
    public void ActivateCloud(Vector2 position, string newTarget)
    {
        if (string.IsNullOrEmpty(newTarget))
        {
            Debug.LogError("CLOUD TARGET STRING IS NULL! :(", this);
            return;
        }

        // Generate a random color
        // Could implement CMCI to get distinct colors... but that's too much work
        var color = new Color(UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.7f, 1f));

        transform.position = position;
        transform.localScale = Vector3.zero;
        collider.enabled = false;
        targetString = newTarget;
        
        currentNumberOfSegments = Mathf.CeilToInt(targetString.Length / 3f);
        for (int i = 0; i < currentNumberOfSegments; i++)
        {
            if (segments.Count == i)
            {
                var newSegment = Instantiate(cloudSegmentPrefab);
                newSegment.transform.SetParent(transform, worldPositionStays: false);
                newSegment.SetColor(color);
                segments.Add(newSegment);
            }

            segments[i].CloudSegmentStateChangeEvent += OnSegmentStateChanged;

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

    private void OnSegmentStateChanged(CloudSegment segment)
    {
        switch (segment.CurrentState)
        {
            // Activate the next segment
            case CloudSegment.State.Completed:

                if (currentActiveSegmentIndex + 1 < currentNumberOfSegments)
                {
                    segments[++currentActiveSegmentIndex].SetState(CloudSegment.State.Active);
                }
                break;

            case CloudSegment.State.Destroyed:

                destroyedSegmentCount++;

                if (destroyedSegmentCount == currentNumberOfSegments)
                {
                    DestroyCloud();
                }
                break;
            case CloudSegment.State.Collided:
                // TODO: Decide if we destroy the cloud or break off earlier pieces
                DestroyCloud(false);
                break;
        }
    }

    public void DestroyCloud(bool giveScore = true)
    {
        for (int i = 0; i < currentNumberOfSegments; i++)
        {
            segments[i].CloudSegmentStateChangeEvent -= OnSegmentStateChanged;
        }
        collider.enabled = false;

        if (giveScore)
        {
            GameManager.Instance.DestroyCloudSound();
            CloudDestroyedEvent?.Invoke(currentNumberOfSegments, targetString.Length);
        }

        StartCoroutine(RunDestroyAnimation());
    }

    private IEnumerator RunDestroyAnimation()
    {
        // Scale down
        var startTime = Time.time;
        while (transform.localScale.x > 0.1f)
        {
            yield return null;
            var elapsedTime = Time.time - startTime;

            // TODO: Use an animation curve for snappier feeling animation
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / GameManager.EnemyManager.SpawnDuration);
        }

        gameObject.SetActive(false);
    }
}
