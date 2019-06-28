using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Alphabet : MonoBehaviour
{
    #region Statics

    private static int MAX_COLLISIONS = 8;

    #endregion

    #region Fields

    [SerializeField]
    private TextMeshPro text;

    PlayerController player;
    float distanceMoved = 0;

    private Collider2D[] hitColliders = new Collider2D[MAX_COLLISIONS];
    #endregion

    #region Properties

    private bool _isActive = false;
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            if(value != _isActive)
            {
                gameObject.SetActive(value);
                _isActive = value;
            }
        }
    }
    public char CurrentChar { get; private set; }

    #endregion

    public static bool TRACKINGMISSILEMODE = true;
    private float reticleRadius = 0;

    private void Awake()
    {
        player = PlayerController.Instance;
        reticleRadius = player.Reticle.GetComponent<SpriteRenderer>().size.x / 2;

        // Start the laser
        laser.startWidth = 0.5f;
        laser.endWidth = 0.7f;
        laser.startColor = new Color(0.5f, 0, 0);
        laser.endColor = new Color(0, 0, 0.5f);
    }

    public void Activate(char newChar)
    {
        // Update character
        CurrentChar = newChar;
        text.SetText(newChar.ToString());

        // Update position
        transform.position = player.transform.position;
        distanceMoved = 0;

        // Set active
        IsActive = true;
    }
    
    private void FixedUpdate()
    {
        // Calculate movement here so we can use Time.fixedDeltaTime and have a consistence distance traveled calculation
        UpdateMovement();

        CheckForCloudCollision();

        UpdateLaser();
    }

    [SerializeField]
    private LineRenderer laser;

    private float laserIncrement = 1f;
    private void UpdateLaser()
    {
        laser.SetPosition(0, transform.position);
        laser.SetPosition(1, player.ReticleCenter);

        var newR = laser.endColor.r + Time.deltaTime * laserIncrement;

        //Since we're reversing polarity, we can't afford to have 2 cycles in the wrong polarity
        if (newR > 0.9)
        {
            newR = 0.9f;
            laserIncrement = -laserIncrement;
        }
        else if (newR < 0.5)
        {
            newR = 0.5f;
            laserIncrement = -laserIncrement;
        }
        var reticleColor = player.Reticle.GetComponent<SpriteRenderer>().color;
        laser.endColor = new Color(reticleColor.r, reticleColor.g, reticleColor.b, 0.1f);
        laser.startColor = new Color(newR - 0.3f, newR - 0.3f, newR, 0.1f);
    }

    private void UpdateMovement()
    {
        if (IsActive)
        {
            if (TRACKINGMISSILEMODE)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.ReticleCenter,
                    AlphabetManager.Instance.ModifiedAlphabetSpeed * Time.fixedDeltaTime);
                distanceMoved += AlphabetManager.Instance.ModifiedAlphabetSpeed * Time.fixedDeltaTime;

                if (Vector2.Distance(transform.position, player.ReticleCenter) < reticleRadius)
                {
                    // Reached the target without colliding into anything, just deactivate it
                    IsActive = false;
                }
                return;
            }

            if (player.LaserIsActive)
            {
                transform.position = Vector2.MoveTowards(
                    player.transform.position,
                    player.ReticleCenter,
                    AlphabetManager.Instance.ModifiedAlphabetSpeed * Time.fixedDeltaTime + distanceMoved);
                distanceMoved += AlphabetManager.Instance.ModifiedAlphabetSpeed * Time.fixedDeltaTime;

                if (Vector2.Distance(transform.position, player.ReticleCenter) < Mathf.Epsilon)
                {
                    // Reached the target without colliding into anything, just deactivate it
                    IsActive = false;
                }
            }
            else
            {
                IsActive = false;
            }
        }
    }

    private void CheckForCloudCollision()
    {
        Array.Clear(hitColliders, 0, hitColliders.Length);

        // Unfortunately, the bounds don't seem to respect scale, so we have to manually calculate the correct box size
        var boxSize = Vector3.Scale(text.bounds.size, transform.lossyScale);
        var numHits = Physics2D.OverlapBoxNonAlloc(transform.position, boxSize, 0f, hitColliders, (int)LayerMasks.Cloud);
        if (numHits > 0)
        {
            foreach (var collider in hitColliders)
            {
                if (collider != null)
                {
                    if (collider.CompareTag(Tags.Enemy))
                    {
                        var cloud = collider.GetComponent<Cloud>();
                        if (cloud != null)
                        {
                            cloud.OnAlphabetImpact(CurrentChar);
                            IsActive = false;
                        }
                    }
                    else
                    {
                        // TODO: Bad things happen. Filter out the stuff to collide with first using layer masks before implementing this
                    }
                }
            }
        }
    }
}
