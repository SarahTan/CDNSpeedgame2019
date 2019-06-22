using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EnemySegment : MonoBehaviour
{
    #region Statics

    private static int MAX_COLLISIONS = 8;

    #endregion

    #region Enums

    // TODO: Might want to convert this into a proper state machine if stuff gets more complicated
    public enum EnemySegmentState
    {
        Disabled = 0,       // Not in play
        Inactive = 1,       // Has targetString but can't start typing
        Active = 2,         // Can start typing
        Completed = 3,      // Finished typing, waiting to be destroyed
        Destroyed = 4       // Mouse has already right clicked it
    }

    #endregion

    #region Fields

    [SerializeField]
    private TextMeshPro text;
    [SerializeField]
    private SpriteRenderer backgroundRenderer;
    [SerializeField]
    private BoxCollider2D collider;

    // TODO: Move to enemy manager class
    [SerializeField]
    private Color markedColor;
    [SerializeField]
    private Color unmarkedColor;

    private int firstUnmarkedCharIndex = 0;

    private string markedColorHex;
    private string unmarkedColorHex;

    private RectTransform rectTransform;

    private Camera mainCam;
    private Collider2D[] hitColliders = new Collider2D[MAX_COLLISIONS];

    #endregion

    #region Properties

    private string _targetString = string.Empty;
    private string TargetString
    {
        get { return _targetString; }
        set
        {
            if(value != _targetString)
            {
                _targetString = value;
                StartCoroutine(SetBackgroundRenderer());

                IEnumerator SetBackgroundRenderer()
                {
                    // Need to wait for the GUI to render first, so that the rect transform will have the updated bounds
                    yield return null;

                    backgroundRenderer.size = rectTransform.sizeDelta + Vector2.one;
                    collider.size = rectTransform.sizeDelta;
                }
            }
        }
    }
    
    private char FirstUnmarkedChar
    {
        get
        {
            if(TargetString.Length > firstUnmarkedCharIndex)
            {
                return TargetString[firstUnmarkedCharIndex];
            }
            else
            {
                Debug.LogError("ERROR: Index of next unmarked char exceeded target string length", gameObject);
                return '\0';
            }
        }
    }

    private EnemySegmentState _currentState = EnemySegmentState.Disabled;
    public EnemySegmentState CurrentState
    {
        get { return _currentState; }
        set
        {
            if(value != _currentState)
            {
                _currentState = value;
                UpdateTextVisuals();
                EnemySegmentStateChangeEvent?.Invoke(this);
            }
        }
    }

    #endregion

    #region Events

    public event Action<EnemySegment> EnemySegmentStateChangeEvent;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        markedColorHex = ColorUtility.ToHtmlStringRGB(markedColor);
        unmarkedColorHex = ColorUtility.ToHtmlStringRGB(unmarkedColor);

        rectTransform = (RectTransform)transform;

        mainCam = Camera.main;
    }

    private void FixedUpdate()
    {
        CheckForRightClick();
    }
    
    #endregion

    public void SetTargetString(string newTarget)
    {
        firstUnmarkedCharIndex = 0;
        TargetString = newTarget;
        CurrentState = EnemySegmentState.Inactive;
    }

    public void ActivateSegment()
    {
        CurrentState = EnemySegmentState.Active;
    }

    public void TryMarkChar(char charToTry)
    {
        if (CurrentState == EnemySegmentState.Active && FirstUnmarkedChar == charToTry)
        {
            firstUnmarkedCharIndex++;
            if (firstUnmarkedCharIndex == TargetString.Length)
            {
                CurrentState = EnemySegmentState.Completed;
            }
            else if(char.IsWhiteSpace(FirstUnmarkedChar))
            {
                firstUnmarkedCharIndex++;
            }

            UpdateTextVisuals();
        }
    }

    private void CheckForRightClick()
    {
        if (Input.GetMouseButtonDown(1) && CurrentState == EnemySegmentState.Completed)
        {
            Array.Clear(hitColliders, 0, hitColliders.Length);

            var mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var numHits = Physics2D.OverlapPointNonAlloc(mouseWorldPos, hitColliders);
            if (numHits > 0)
            {
                foreach (var collider in hitColliders)
                {
                    if (collider != null)
                    {
                        var segment = collider.GetComponent<EnemySegment>();
                        if (segment != null && segment == this)
                        {
                            DestroySegment();
                        }
                    }
                }
            }
        }
    }

    private void UpdateTextVisuals()
    {
        switch (CurrentState)
        {
            case EnemySegmentState.Disabled:
                gameObject.SetActive(false);
                break;

            case EnemySegmentState.Inactive:
                text.SetText($"<color=#{unmarkedColorHex}>{TargetString}");
                gameObject.SetActive(true);
                break;

            case EnemySegmentState.Active:
                // No char has been marked
                if (firstUnmarkedCharIndex == 0)
                {
                    text.SetText($"<color=#{unmarkedColorHex}><u>{TargetString[0]}</u>{TargetString.Substring(1)}");
                }
                else
                {
                    text.SetText($"<color=#{markedColorHex}>{TargetString.Substring(0, firstUnmarkedCharIndex)}" +   // Marked
                                 $"<color=#{unmarkedColorHex}><u>{TargetString[firstUnmarkedCharIndex]}</u>" +       // First unmarked char is underlined
                                 $"{TargetString.Substring(firstUnmarkedCharIndex + 1)}");                             // Remaining unmarked
                }
                break;

            case EnemySegmentState.Completed:
                // TODO: More visual feedback
                text.SetText($"<color=#{markedColorHex}>{TargetString}");

                break;

            case EnemySegmentState.Destroyed:
                // TODO: Something. Not this, it's terrible and only for debugging
                text.SetText($"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>{TargetString}");
                break;

        }
    }

    private void DestroySegment()
    {
        // TODO: What exactly does destroying this entail?

        CurrentState = EnemySegmentState.Destroyed;
    }
}
