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
    private float speed;
    [SerializeField]
    private TextMeshPro text;
    [SerializeField]
    private BoxCollider2D collider;

    PlayerController player;
    int steps = 0;

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
        steps = 0;

        // Set active
        IsActive = true;
    }
    
    private void FixedUpdate()
    {
        // Movement
        // Calculate movement here so we can use Time.fixedDeltaTime and have a consistence distance traveled calculation
        if (IsActive)
        {
            if (player.LaserIsActive)
            {
                transform.position = Vector2.MoveTowards(player.transform.position, player.ReticleCenter, speed * Time.fixedDeltaTime * ++steps);

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


        // Collision
        Array.Clear(hitColliders, 0, hitColliders.Length);

        // TODO: Add layer mask once we have more stuff to collide with

        // bounds.center is in local space, so convert it to world space
        var numHits = Physics2D.OverlapBoxNonAlloc(transform.TransformPoint(collider.bounds.center), collider.bounds.size, 0f, hitColliders);
        if(numHits > 0)
        {
            foreach(var collider in hitColliders)
            {
                // TODO: Create layer and tag manager and use variable here instead 
                if (collider != null && collider.CompareTag("Enemy"))
                {
                    var enemySegment = collider.GetComponent<EnemySegment>();
                    if(enemySegment != null)
                    {
                        // TODO: Create Enemy.cs and have that handle which segment to call TryMarkChar on,
                        // and to handle the bad stuff which happens when a wrong segment is chosen
                        enemySegment.TryMarkChar(CurrentChar);
                        IsActive = false;
                    }
                }
            }
        }
    }

    // TODO: Figure out why the heck the collider is moving faster than the gameobject
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.TransformPoint(collider.bounds.center), collider.bounds.size);
    }
}
