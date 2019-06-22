using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    #region Constants

    // Names and bindings are set in Unity's InputManager
    private const string HORIZONTAL_ARROW = "Horizontal";
    private const string VERTICAL_ARROW = "Vertical";

    private const string HORIZONTAL_NUMPAD = "HorizontalNumpad";
    private const string VERTICAL_NUMPAD = "VerticalNumpad";

    #endregion

    #region Fields

    [Header("Mouse")]
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

    #endregion

    #region Properties

    public Vector2 ReticleCenter
    {
        get
        {
            return Utils.MainCam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public bool LaserIsActive { get; private set; } = false;

    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        UpdateCursorVisuals(false);
        moveStopTime = Time.time;
    }

    private void Update()
    {
        // Mouse
        LaserIsActive = Input.GetMouseButton(0);
        if (LaserIsActive)
        {
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, ReticleCenter);
            laser.enabled = true;
        }
        else
        {
            laser.enabled = false;
        }
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

    #endregion

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
            Debug.LogError("BAD STUFF HAPPENS! Double Move.");
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
            Debug.LogError("BAD STUFF HAPPENS! Illegal Move.");
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
            // TODO: restrict movement to cam viewport only?
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
