using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EnemySegment : MonoBehaviour
{
    #region Enums

    // TODO: Might want to convert this into a proper state machine if stuff gets more complicated
    public enum EnemySegmentState
    {
        Disabled = 0,       // Not in play
        Inactive = 1,       // Has targetString but can't start typing
        Active = 2,         // Can start typing
        Completed = 3,      // Finished typing
        Destroyed = 4       // Mouse has already right clicked it
    }

    #endregion

    #region Fields

    [SerializeField]
    private TextMeshPro text;
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

                // Adjust collider size
                StartCoroutine(UpdateColliderSize());
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
                UpdateVisuals();
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
    }

    private void OnMouseOver()
    {
        // Right click
        if (CurrentState == EnemySegmentState.Completed && Input.GetMouseButtonDown(1))
        {
            DestroySegment();
        }
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

    private IEnumerator UpdateColliderSize()
    {
        // Need to wait for the GUI to render first, so that the text will have the updated bounds
        yield return new WaitForEndOfFrame();

        collider.size = text.mesh.bounds.size;
    }

    public void TryMarkChar(char charToTry)
    {
        // TODO: Handle spaces
        if (CurrentState == EnemySegmentState.Active && FirstUnmarkedChar == charToTry)
        {
            firstUnmarkedCharIndex++;
            if (firstUnmarkedCharIndex == TargetString.Length)
            {
                CurrentState = EnemySegmentState.Completed;
            }

            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
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
                // TODO: Something
                break;

        }
    }

    private void DestroySegment()
    {
        Debug.LogError($"DESTROYED {this.name}!", gameObject);
        // TODO: What exactly does destroying this entail?

        CurrentState = EnemySegmentState.Destroyed;
    }
}
