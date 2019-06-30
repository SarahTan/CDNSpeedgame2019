using System;
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

    private float reticleRadius = 0;

    private void Awake()
    {
        // Start the laser
        laser.startWidth = 0.5f;
        laser.endWidth = 0f; // Taper the laser so it vanishes smoothly
        laser.startColor = new Color(0.5f, 0, 0);
        laser.endColor = new Color(0, 0, 0.5f);
    }

    private void Start()
    {
        reticleRadius = GameManager.Player.Reticle.GetComponent<SpriteRenderer>().size.x / 2;
    }

    public void Activate(char newChar)
    {
        // Update character
        CurrentChar = newChar;
        text.SetText(newChar.ToString());

        // Update position
        transform.position = GameManager.Player.transform.position;
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
        laser.SetPosition(1, GameManager.Player.ReticleCenter);

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
        var reticleColor = GameManager.Player.Reticle.GetComponent<SpriteRenderer>().color;
        laser.endColor = new Color(reticleColor.r, reticleColor.g, reticleColor.b, 0.1f);
        laser.startColor = new Color(newR - 0.3f, newR - 0.3f, newR, 0.1f);
    }

    private void UpdateMovement()
    {
        if (IsActive)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                GameManager.Player.ReticleCenter,
                GameManager.AlphabetManager.ModifiedAlphabetSpeed * Time.fixedDeltaTime);
            distanceMoved += GameManager.AlphabetManager.ModifiedAlphabetSpeed * Time.fixedDeltaTime;

            if (Vector2.Distance(transform.position, GameManager.Player.ReticleCenter) < reticleRadius)
            {
                // Reached the target without colliding into anything, just deactivate it
                GameManager.Instance.LetterDisappearSound();
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
