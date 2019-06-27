using System;
using System.Collections;
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
    private GameObject reticlePrefab;
    [SerializeField]
    private Texture2D cursorTex;
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
    
    private bool wasUsingNumpad = false; // Tracks whether the LAST PRESSED MOVEMENT was using the numpad
    private Vector2 movementDirection = Vector2.zero; // Tracks the CURRENT MOVEMENT DIRECTION
    private float moveStopTime; // Tracks the time AFTER WHICH holding a button NO LONGER MOVES
    private float moveStartTime; // Tracks the time that movement started
    private float laserIncrement = 1f;

    private float invincibilityEndTime = 0f;

    // Tuple of Expiration Time and Multiplication Factor, to slow down movement
    private List<Tuple<float, float>> playerSpeedModifiers = new List<Tuple<float, float>>();

    private GameObject reticle;

    #endregion

    #region Properties

    public Vector2 ReticleCenter
    {
        get
        {
            return reticle.transform.position.ToVector2();
            //return Utils.MainCam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public bool LaserIsActive { get; private set; } = false;

    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        Cursor.visible = false;
        reticle = Instantiate(reticlePrefab, Utils.MainCam.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
        //UpdateCursorVisuals(false);
        moveStopTime = Time.time;

        // Start the laser
        laser.startWidth = 0.5f;
        laser.endWidth = 0.7f;
        laser.startColor = new Color(0.5f, 0, 0);
        laser.endColor = new Color(0, 0, 0.5f);
    }

    private void Update()
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
            laser.endColor = new Color(newR, newR - 0.5f, newR - 0.5f);
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
        // TODO: Probably wanna rate limit this?
        if (LaserIsActive && Event.current.isKey)
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

    #endregion

    public void GetHit()
    {
        hitPoints--;

        if (hitPoints <= 0)
        {
            // TODO: Lose the game
        }
    }

    /// <summary>
    /// Makes bad stuff happen
    /// </summary>
    /// <param name="BadStuffCategories">The categories of which bad stuff happens - "Typing", "Targeting", or "Moving"</param>
    private void BadStuffHappens(params string[] BadStuffCategories)
    {
        var chosenCategory = new System.Random().Next(0, BadStuffCategories.Length);
        switch (BadStuffCategories[chosenCategory])
        {
            case "Targeting":
            case "Typing":
            case "Moving":
                //this.speed = this.speed * 0.98f;
                Debug.Log("Make bad stuff happen to movement.");
                break;
            default:
                Debug.Log("Bad stuff happened while making bad stuff happen. Or just lose HP.");
                break;
        }
    }

    /// <summary>
    /// Movement for player using Arrow Keys and Numpad
    /// </summary>
    private void UpdateMovement()
    {
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
            rb.velocity = movementDirection * speed;
        }
    }

    private void UpdateCursorVisuals(bool reset)
    {
        // hotspot is the offset from the top left to the target point of the cursor
        var hotspot = Vector3.zero;
        if (!reset)
        {
            hotspot = new Vector2(cursorTex.width / 2f, cursorTex.height / 2f);
        }
        Cursor.SetCursor(reset ? null : cursorTex, hotspot, CursorMode.Auto);
    }
}
