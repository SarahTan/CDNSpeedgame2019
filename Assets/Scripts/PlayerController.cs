using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    #region Constants and Statics

    // Names and bindings are set in Unity's InputManager
    private const string HORIZONTAL_ARROW = "Horizontal";
    private const string VERTICAL_ARROW = "Vertical";

    private const string HORIZONTAL_NUMPAD = "HorizontalNumpad";
    private const string VERTICAL_NUMPAD = "VerticalNumpad";

    public event Action HitEnemyEvent;

    #endregion

    #region Fields

    [SerializeField]
    private float hitInvincibilityDuration;
    [SerializeField]
    private int hitPoints;

    [Header("Mouse")]
    [SerializeField]
    private Reticle reticlePrefab;
    [SerializeField]
    private LineRenderer laser;

    [Header("Mover")]
    [SerializeField]
    private float maxMoveDurationPerKeypress;
    [SerializeField]
    private float minMovedurationPerKeypress;
    [SerializeField]
    private float speed;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float slowdownDuration;
    [SerializeField]
    private float slowdownFactor;
    
    private bool wasUsingNumpad = false; // Tracks whether the LAST PRESSED MOVEMENT was using the numpad
    private Vector2 movementDirection = Vector2.zero; // Tracks the CURRENT MOVEMENT DIRECTION
    private float moveStopTime; // Tracks the time AFTER WHICH holding a button NO LONGER MOVES
    private float moveStartTime; // Tracks the time that movement started
    private float laserIncrement = 1f;

    private float invincibilityEndTime = 0f;
    private float lastBadStuffTime = 0;

    // Expiration time of modifiers to slow down player movement - sorted
    private Queue<float> playerSpeedModifiers = new Queue<float>();

    #endregion

    #region Properties

    public Reticle Reticle { get; private set; }
    private ParticleSystem glow;

    public Vector2 ReticleCenter
    {
        get
        {
            return Reticle.transform.position.ToVector2();
        }
    }

    public bool LaserIsActive { get; private set; } = false;

    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Reticle = Instantiate(reticlePrefab, Utils.MainCam.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
        moveStopTime = Time.time;

        // Start the laser
        laser.startWidth = 0.5f;
        laser.endWidth = 0.7f;
        laser.startColor = new Color(0.5f, 0, 0);
        laser.endColor = new Color(0, 0, 0.5f);

        glow = GetComponent<ParticleSystem>();

        GameManager.Instance.GamePausedEvent += OnGamePaused;
    }
    
    private void Update()
    {
        if (!GameManager.Instance.GameIsPaused)
        {
            if (!Alphabet.TRACKINGMISSILEMODE)
            {
                UpdateLaser();
            }
        }
    }

    private void UpdateLaser()
    {
        LaserIsActive = Input.GetMouseButton(0);
        if (LaserIsActive)
        {
            var playerToReticle = ReticleCenter.ToVector3() - transform.position;

            // Cast a ray from the player to the ReticleCenter to check if there's anything blocking the laser.
            // If it hits something, the laser ends at the hit point, else the laser ends at the ReticleCenter. 
            var hit = Physics2D.Raycast(transform.position, playerToReticle, playerToReticle.magnitude, (int)LayerMasks.LaserBlocker);
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, hit.collider ? hit.point : ReticleCenter);

            var newR = laser.endColor.r + Time.deltaTime * laserIncrement;

            // Since we're reversing polarity, we can't afford to have 2 cycles in the wrong polarity
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
            laser.endColor = Reticle.GetComponent<SpriteRenderer>().color;
            laser.startColor = new Color(newR - 0.3f, newR - 0.3f, newR);

            if (hit.collider != null)
            {
                // TODO: Set a scattered laser
            }
        }

        laser.enabled = LaserIsActive;
    }

    private void FixedUpdate()
    {
        // Mover
        UpdateMovement();
    }

    private void OnGUI()
    {
        // Typer
        if ((LaserIsActive || Alphabet.TRACKINGMISSILEMODE)
            && Event.current.isKey)
        {
            var downChar = Event.current.character;
            if (char.IsLetter(downChar))
            {
                AlphabetManager.Instance.ActivateAlphabet(char.ToUpperInvariant(downChar));
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Even though we are only interested in EnemySegment, we must check against Enemy as it has a Rigidbody and hence will treat the
        // children EnemySegment's colliders as its own compound colliders, and be the one sending the collision events instead.
        if (collision.gameObject.layer == (int)Layers.Cloud) // Hits an enemy segment
        {
            var enemySegment = collision.collider.GetComponentInParent<CloudSegment>();
            if (enemySegment != null && 
                enemySegment.CurrentState != CloudSegment.State.Spawning)
            {
                if (Time.time > invincibilityEndTime)
                {
                    enemySegment.SetState(CloudSegment.State.Collided);

                    invincibilityEndTime = Time.time + hitInvincibilityDuration;
                    HitEnemyEvent?.Invoke();
                    GetHit();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GamePausedEvent -= OnGamePaused;
        }
    }

    #endregion

    public void GetHit()
    {
        hitPoints--;
        Debug.Log("Hit points left: " + hitPoints);
        if (hitPoints <= 0)
        {
            // TODO: Lose the game
            return;
        }

        ChangeColor();
    }

    private void OnGamePaused(bool isPaused)
    {
        laser.enabled = false;
        Cursor.visible = isPaused;
    }

    /// <summary>
    /// Makes bad stuff happen
    /// </summary>
    /// <param name="BadStuffCategories">The categories of which bad stuff happens - "Typing", "Targeting", or "Moving"</param>
    public void BadStuffHappens(params string[] BadStuffCategories)
    {
        var chosenCategory = new System.Random().Next(0, BadStuffCategories.Length);
        switch (BadStuffCategories[chosenCategory])
        {
            case "Targeting":
                Reticle.BadStuffHappens();
                break;
            case "Typing":
                AlphabetManager.Instance.BadStuffHappens();
                Debug.Log("Make bad stuff happen to typing.");
                break;
            case "Moving":
                if (lastBadStuffTime < Time.time - 0.1f) // For some reason, this fires like, 3 times at once
                {
                    playerSpeedModifiers.Enqueue(Time.time + slowdownDuration);
                    lastBadStuffTime = Time.time;
                    Debug.Log("Player slowdowns: " + playerSpeedModifiers.Count);
                    ChangeColor();
                }
                break;
            default:
                Debug.Log("Bad stuff happened while making bad stuff happen. Or just lose HP.");
                break;
        }
    }

    private void ChangeColor()
    {
        var emission = glow.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(hitPoints * 2);

        var renderer = GetComponent<SpriteRenderer>();

        // Assumes 10 hit points at start - adjust as necessary
        renderer.color = new Color(
            1.0f - (playerSpeedModifiers.Count * 0.05f),
            1.0f,
            1.0f,
            0.4f + 0.06f * hitPoints);
    }

    /// <summary>
    /// Movement for player using Arrow Keys and Numpad
    /// </summary>
    private void UpdateMovement()
    {
        if (playerSpeedModifiers.Count > 0
            && playerSpeedModifiers.Peek() < Time.time)
        {
            playerSpeedModifiers.Dequeue();
            ChangeColor();
        }

        var numpadMovement = new Vector2(Input.GetAxis(HORIZONTAL_NUMPAD), Input.GetAxis(VERTICAL_NUMPAD));
        var arrowMovment = new Vector2(Input.GetAxis(HORIZONTAL_ARROW), Input.GetAxis(VERTICAL_ARROW));

        numpadMovement.Normalize();
        arrowMovment.Normalize();

        // Stupidly enumerate through all cases first - we can get smart later
        if (numpadMovement != Vector2.zero
            && arrowMovment != Vector2.zero)
        {
            // Trivial case - can't have both inputs happen at the same time
            BadStuffHappens("Targeting", "Typing");
            return;
        }
        
        // TODO: Some kind of feedback so the player knows to use arrows or numpad
        // Case 1: Continue holding button that was held
        if (movementDirection != Vector2.zero
            && numpadMovement != Vector2.zero
            && wasUsingNumpad
            && numpadMovement == movementDirection)
        {
            // Legal move
        }
        else if (movementDirection != Vector2.zero
            && arrowMovment != Vector2.zero
            && !wasUsingNumpad
            && arrowMovment == movementDirection)
        {
            // Legal move
        }
        // Case 2: Switching movement directions
        else if (numpadMovement != Vector2.zero
            && !wasUsingNumpad)
        {
            movementDirection = numpadMovement;
            wasUsingNumpad = true;
            moveStopTime = Time.time + maxMoveDurationPerKeypress;
            moveStartTime = Time.time;
        }
        else if (arrowMovment != Vector2.zero
            && wasUsingNumpad)
        {
            movementDirection = arrowMovment;
            wasUsingNumpad = false;
            moveStopTime = Time.time + maxMoveDurationPerKeypress;
            moveStartTime = Time.time;
        }
        // Case 3: ANYTHING ELSE
        else if (numpadMovement != Vector2.zero
            || arrowMovment != Vector2.zero)
        {
            movementDirection = Vector2.zero;
            BadStuffHappens("Targeting", "Typing");
            return;
        }

        // Resolve movement
        if(numpadMovement == Vector2.zero && arrowMovment == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
        }
        else if ((moveStopTime > Time.time
            && (arrowMovment != Vector2.zero
            || numpadMovement != Vector2.zero))
            || Time.time < moveStartTime + minMovedurationPerKeypress)
        {

            var totalSlowdown = playerSpeedModifiers.Count * slowdownFactor;
            bool hardMode = false;
            if (playerSpeedModifiers.Count > 10)
            {
                hardMode = true;
                totalSlowdown = 0;
            }

            rb.velocity = movementDirection * (speed - totalSlowdown) * (hardMode ? -1 : 1);
        }
    }
}
