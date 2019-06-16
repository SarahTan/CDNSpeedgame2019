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
    private float canSwitchMoveInputDelay;
    [SerializeField]
    private float speed;
    [SerializeField]
    private CharacterController charController;


    private Camera mainCam;

    private bool canSwitchMoveInput = false;
    private bool usingNumpad = false;
    private bool hasReleasedPreviousMoveInputs = true;
    private float canSwitchMoveInputStartTime;

    #endregion

    #region Properties

    public Vector2 ReticleCenter
    {
        get
        {
            return mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public bool LaserIsActive { get; private set; } = false;


    private Vector2 CurrentMovementDirection
    {
        get
        {
            return new Vector2(Input.GetAxis(usingNumpad ? HORIZONTAL_NUMPAD : HORIZONTAL_ARROW),
                               Input.GetAxis(usingNumpad ? VERTICAL_NUMPAD : VERTICAL_ARROW));
        }
    }

    private Vector2 AltMovementDirection
    {
        get
        {
            return new Vector2(Input.GetAxis(usingNumpad ? HORIZONTAL_ARROW : HORIZONTAL_NUMPAD),
                               Input.GetAxis(usingNumpad ? VERTICAL_ARROW : VERTICAL_NUMPAD));
        }
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        UpdateCursorVisuals(false);
        canSwitchMoveInputStartTime = Time.time;
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

        // Mover
        if (hasReleasedPreviousMoveInputs)
        {
            if (AltMovementDirection != Vector2.zero)
            {
                // TODO: Bad stuff happens. Probably wanna give a grace period though.
                Debug.LogError("BAD STUFF HAPPENS!");
            }
            else if (CurrentMovementDirection != Vector2.zero)
            {
                charController.Move(CurrentMovementDirection * speed * Time.deltaTime);
                // TODO: restrict movement to cam viewport only?

                if (!canSwitchMoveInput)
                {
                    canSwitchMoveInputStartTime = Time.time;
                    canSwitchMoveInput = true;
                }
            }
        }
        else
        {
            hasReleasedPreviousMoveInputs = AltMovementDirection == Vector2.zero;
        }

        if (canSwitchMoveInput && Time.time - canSwitchMoveInputStartTime > canSwitchMoveInputDelay)
        {
            hasReleasedPreviousMoveInputs = false;
            canSwitchMoveInput = false;
            usingNumpad = !usingNumpad;

            // TODO: Some kind of feedback so the player knows to use arrows or numpad
        }
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
                AlphabetManager.Instance.ActivateAlphabet(downChar);
            }
        }
    }

    #endregion

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
