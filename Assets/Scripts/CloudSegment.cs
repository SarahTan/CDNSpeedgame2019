using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CloudSegment : MonoBehaviour
{
    #region Statics

    private static int MAX_COLLISIONS = 8;

    #endregion

    #region Enums

    // TODO: Might want to convert this into a proper state machine if stuff gets more complicated
    public enum CloudSegmentState
    {
        Disabled = 0,       // Not in play
        Spawning = -1,      // Immune to most forms of interaction with the player
        Inactive = 1,       // Has targetString but can't start typing
        Active = 2,         // Can start typing
        Completed = 3,      // Finished typing, waiting to be destroyed
        Destroyed = 4,      // Mouse has already right clicked it
        Collided = 5        // Was collided into - drop assumptions that it has been typed
    }

    #endregion

    #region Fields

    [SerializeField]
    private TextMeshPro text;
    [SerializeField]
    private new SpriteRenderer renderer;
    [SerializeField]
    private new BoxCollider2D collider;

    private int firstUnmarkedCharIndex = 0;
    private RectTransform rectTransform;

    private Collider2D[] hitColliders = new Collider2D[MAX_COLLISIONS];
    
    private string targetString = string.Empty;

    #endregion

    #region Properties
    
    private char FirstUnmarkedChar
    {
        get
        {
            if(targetString.Length > firstUnmarkedCharIndex)
            {
                return targetString[firstUnmarkedCharIndex];
            }
            else
            {
                Debug.LogError("ERROR: Index of next unmarked char exceeded target string length", gameObject);
                return '\0';
            }
        }
    }

    private CloudSegmentState _currentState = CloudSegmentState.Disabled;
    public CloudSegmentState CurrentState
    {
        get { return _currentState; }
        set
        {
            if(value != _currentState)
            {
                _currentState = value;
                UpdateVisuals();
                CloudSegmentStateChangeEvent?.Invoke(this);
            }
        }
    }

    #endregion

    #region Events

    public event Action<CloudSegment> CloudSegmentStateChangeEvent;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    private void FixedUpdate()
    {
        CheckForRightClick();
    }

    private void OnDisable()
    {
        CurrentState = CloudSegmentState.Disabled;
    }

    #endregion

    public void SetTargetString(string newTarget)
    {
        firstUnmarkedCharIndex = 0;
        targetString = newTarget;
        CurrentState = CloudSegmentState.Spawning;
    }

    public void ActivateSegment()
    {
        CurrentState = CloudSegmentState.Active;
    }

    public void TryMarkChar(char charToTry)
    {
        if (CurrentState == CloudSegmentState.Active && FirstUnmarkedChar == charToTry)
        {
            firstUnmarkedCharIndex++;
            if (firstUnmarkedCharIndex == targetString.Length)
            {
                CurrentState = CloudSegmentState.Completed;
            }
            else if(char.IsWhiteSpace(FirstUnmarkedChar))
            {
                firstUnmarkedCharIndex++;
            }

            UpdateVisuals();
        }
    }

    private void CheckForRightClick()
    {
        if (Input.GetMouseButtonDown(1) && CurrentState == CloudSegmentState.Completed)
        {
            Array.Clear(hitColliders, 0, hitColliders.Length);

            var mouseWorldPos = Utils.MainCam.ScreenToWorldPoint(Input.mousePosition);
            var numHits = Physics2D.OverlapPointNonAlloc(mouseWorldPos, hitColliders);
            if (numHits > 0)
            {
                foreach (var collider in hitColliders)
                {
                    if (collider != null)
                    {
                        var segment = collider.GetComponentInParent<CloudSegment>();
                        if (segment != null && segment == this)
                        {
                            DestroySegment();
                        }
                    }
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        switch (CurrentState)
        {
            case CloudSegmentState.Disabled:
                gameObject.SetActive(false);
                break;

            case CloudSegmentState.Spawning:
                text.SetText($"<color=#{EnemyManager.Instance.UnmarkedColorHex}>{targetString}");
                renderer.color = EnemyManager.Instance.MarkedColor;
                gameObject.SetActive(true);
                StartCoroutine(SetRendererAndCollider());
                IEnumerator SetRendererAndCollider()
                {
                    // Need to wait for the GUI to render first, so that the rect transform will have the updated bounds
                    yield return null;

                    renderer.size = rectTransform.sizeDelta + Vector2.one;
                    collider.size = rectTransform.sizeDelta;
                }
                break;

            case CloudSegmentState.Inactive:
                collider.enabled = true;
                break;

            case CloudSegmentState.Active:
                // No char has been marked
                if (firstUnmarkedCharIndex == 0)
                {
                    text.SetText($"<color=#{EnemyManager.Instance.UnmarkedColorHex}><u>{targetString[0]}</u>{targetString.Substring(1)}");
                }
                else
                {
                    text.SetText($"<color=#{EnemyManager.Instance.MarkedColorHex}>{targetString.Substring(0, firstUnmarkedCharIndex)}" +                      // Marked
                                 $"<color=#{EnemyManager.Instance.UnmarkedColorHex}><u>{targetString[firstUnmarkedCharIndex]}</u>" +       // First unmarked char is underlined
                                 $"{targetString.Substring(firstUnmarkedCharIndex + 1)}");                           // Remaining unmarked
                }
                break;

            case CloudSegmentState.Completed:
                text.SetText($"<color=#{EnemyManager.Instance.MarkedColorHex}>{targetString}");

                break;

            case CloudSegmentState.Destroyed:
                text.SetText($"<color=#{EnemyManager.Instance.DestroyedColorHex}>{targetString}");
                renderer.color = EnemyManager.Instance.DestroyedColor;
                break;

        }
    }

    public void DestroySegment()
    {
        // The player can now pass through this segment
        collider.enabled = false;

        CurrentState = CloudSegmentState.Destroyed;
    }
}
