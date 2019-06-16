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

    private void Update()
    {
        if (IsActive)
        {
            if (player.LaserIsActive)
            {
                // Use Time.fixedDeltaTime here so that the distance traveled calculation is consistent.
                // Note: This speed calculation is different from that of PlayerController since time.fixedDeltaTime is used, but close enough
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
    }

    // Do our own collision detection
    private void FixedUpdate()
    {
        Array.Clear(hitColliders, 0, hitColliders.Length);

        // TODO: Add layer mask once we have more stuff to collide with

        // bounds.center is in local space, so convert it to world space
        var numHits = Physics2D.OverlapBoxNonAlloc(transform.TransformPoint(text.mesh.bounds.center), text.mesh.bounds.size, 0f, hitColliders);
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
}
