using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    #region Fields

    [Header("Mouse")]
    [SerializeField]
    private Texture2D cursorTex;
    [SerializeField]
    private LineRenderer laser;

    [Header("Mover")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private CharacterController charController;


    private Camera mainCam;


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


    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Start()
    {
        UpdateCursorVisuals(false);
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
        // TODO: Figure out what behavior we want with numpad
        var moveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        charController.Move(moveDir * speed * Time.deltaTime);
        // TODO: restrict movement to cam viewport only?
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
