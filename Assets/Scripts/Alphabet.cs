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

    private void Awake()
    {
        player = PlayerController.Instance;
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
    }

    private void UpdateMovement()
    {
        if (IsActive)
        {
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
