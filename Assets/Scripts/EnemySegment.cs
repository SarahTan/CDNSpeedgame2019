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

    // TODO: Move to enemy manager class
    [SerializeField]
    private Color markedColor;
    [SerializeField]
    private Color unmarkedColor;

    private string targetString = "TES";// string.Empty;
    private int firstUnmarkedCharIndex = 0;

    private string markedColorHex;
    private string unmarkedColorHex;

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

    public bool IsComplete { get; private set; } = false;
    public bool IsDestroyed { get; private set; } = false;

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
        targetString = newTarget;
        firstUnmarkedCharIndex = 0;

        UpdateVisuals();
    }

    public void TryMarkChar(char charToTry)
    {
        if (!IsComplete)
        {
            if (FirstUnmarkedChar == charToTry)
            {
                firstUnmarkedCharIndex++;
                if (firstUnmarkedCharIndex == targetString.Length)
                {
                    IsComplete = true;
                }

                UpdateVisuals();
            }
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
            text.SetText($"<color=#{markedColorHex}>{targetString}");
        }
        else
        {
            // No char has been marked
            if (firstUnmarkedCharIndex == 0)
            {
                text.SetText($"<color=#{unmarkedColorHex}><u>{targetString[0]}</u>{targetString.Substring(1)}");
            }
            else
            {
                text.SetText($"<color=#{markedColorHex}>{targetString.Substring(0, firstUnmarkedCharIndex)}" +   // Marked
                             $"<color=#{unmarkedColorHex}><u>{targetString[firstUnmarkedCharIndex]}</u>" +       // First unmarked char is underlined
                             $"{targetString.Substring(firstUnmarkedCharIndex+1)}");                             // Remaining unmarked
            }
        }
    }

    private void DestroySegment()
    {
        Debug.LogError($"DESTROYED {this.name}!", gameObject);
        // TODO: What exactly does destroying this entail?
    }
}
