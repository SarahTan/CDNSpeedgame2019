using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EnemySegment : MonoBehaviour
{
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

    public bool IsComplete { get; private set; } = false;
    public bool IsDestroyed { get; private set; } = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        markedColorHex = ColorUtility.ToHtmlStringRGB(markedColor);
        unmarkedColorHex = ColorUtility.ToHtmlStringRGB(unmarkedColor);

        SetTargetString("TESTING");
    }

    private void OnMouseOver()
    {
        // Right click
        if (IsComplete && !IsDestroyed && Input.GetMouseButtonDown(1))
        {
            DestroySegment();
        }
    }
    
    #endregion

    public void SetTargetString(string newTarget)
    {
        IsComplete = false;
        IsDestroyed = false;
        firstUnmarkedCharIndex = 0;
        TargetString = newTarget;

        UpdateVisuals();
    }

    private IEnumerator UpdateColliderSize()
    {
        // Need to wait for the GUI to render first, so that the text will have the updated bounds
        yield return new WaitForEndOfFrame();

        collider.size = text.mesh.bounds.size;
    }

    public void TryMarkChar(char charToTry)
    {
        if (!IsComplete && FirstUnmarkedChar == charToTry)
        {
            firstUnmarkedCharIndex++;
            if (firstUnmarkedCharIndex == TargetString.Length)
            {
                IsComplete = true;
            }

            UpdateVisuals();
        }
        else
        {
            // TODO: Bad stuff happens
            Debug.LogError("BAD STUFF HAPPENS!");
        }
    }

    private void UpdateVisuals()
    {
        if (IsDestroyed)
        {
            // TODO: Something
        }
        else if (IsComplete)
        {
            // TODO: More visual feedback
            text.SetText($"<color=#{markedColorHex}>{TargetString}");
        }
        else
        {
            // No char has been marked
            if (firstUnmarkedCharIndex == 0)
            {
                text.SetText($"<color=#{unmarkedColorHex}><u>{TargetString[0]}</u>{TargetString.Substring(1)}");
            }
            else
            {
                text.SetText($"<color=#{markedColorHex}>{TargetString.Substring(0, firstUnmarkedCharIndex)}" +   // Marked
                             $"<color=#{unmarkedColorHex}><u>{TargetString[firstUnmarkedCharIndex]}</u>" +       // First unmarked char is underlined
                             $"{TargetString.Substring(firstUnmarkedCharIndex+1)}");                             // Remaining unmarked
            }
        }
    }

    private void DestroySegment()
    {
        Debug.LogError($"DESTROYED {this.name}!", gameObject);
        // TODO: What exactly does destroying this entail?

        IsDestroyed = true;
    }
}
