using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkCloudAnim : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshPro text;

    private int firstUnmarkedCharIndex = 0;

    private Color unmarkedColor = Color.white;
    private Color markedColor = Color.white;

    private string unmarkedColorHex;
    private string markedColorHex;

    private string targetString = "CLOUD";

    private void Awake()
    {
        markedColor = new Color(unmarkedColor.r - 0.8f, unmarkedColor.g - 0.8f, unmarkedColor.b - 0.8f, 0.2f);
        markedColorHex = ColorUtility.ToHtmlStringRGBA(markedColor);
        unmarkedColorHex = ColorUtility.ToHtmlStringRGBA(unmarkedColor);
    }

    public void UpdateText()
    {
        if (firstUnmarkedCharIndex == 0)
        {
            text.SetText($"<color=#{unmarkedColorHex}><u>{targetString[0]}</u>{targetString.Substring(1)}");
        }
        else if(firstUnmarkedCharIndex < targetString.Length)
        {
            text.SetText($"<color=#{markedColorHex}>{targetString.Substring(0, firstUnmarkedCharIndex)}" +                      // Marked
                         $"<color=#{unmarkedColorHex}><u>{targetString[firstUnmarkedCharIndex]}</u>" +       // First unmarked char is underlined
                         $"{targetString.Substring(firstUnmarkedCharIndex + 1)}");                           // Remaining unmarked
        }
        else
        {
            text.SetText($"<color=#{markedColorHex}>{targetString}");
        }
        firstUnmarkedCharIndex++;
    }

    public void Reset()
    {
        firstUnmarkedCharIndex = 0;
        UpdateText();
    }
}
