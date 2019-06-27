using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AlphabetManager : Singleton<AlphabetManager>
{
    #region Fields

    [SerializeField]
    private Alphabet alphabetPrefab;

    public float AlphabetSpeed;

    [SerializeField]
    private float slowdownSpeed;

    [SerializeField]
    private float slowdownDuration;

    private List<Alphabet> alphabets = new List<Alphabet>();

    private float lastBadStuffTime = 0;

    // Expiration time of modifiers to slow down alphabet movement - sorted
    private Queue<float> alphabetSpeedModifiers = new Queue<float>();

    #endregion

    public float ModifiedAlphabetSpeed => AlphabetSpeed - alphabetSpeedModifiers.Count * slowdownSpeed;

    public void BadStuffHappens()
    {
        if (lastBadStuffTime < Time.time - 0.1f
            && alphabetSpeedModifiers.Count < 10) // For some reason, this fires like, 3 times at once
        {
            alphabetSpeedModifiers.Enqueue(Time.time + slowdownDuration);
            lastBadStuffTime = Time.time;
            Debug.Log("Alphabet slowdowns: " + alphabetSpeedModifiers.Count);
            ChangeColor();
        }
    }

    private void ChangeColor()
    {
        foreach (var alphabet in alphabets)
        {
            var text = alphabet.GetComponent<TextMeshPro>();
            text.color = new Color(1.0f - alphabetSpeedModifiers.Count * 0.07f,
                1.0f - alphabetSpeedModifiers.Count * 0.07f,
                1.0f - alphabetSpeedModifiers.Count * 0.07f);
        }
    }

    private void FixedUpdate()
    {
        if (alphabetSpeedModifiers.Count > 0
            && alphabetSpeedModifiers.Peek() < Time.time)
        {
            alphabetSpeedModifiers.Dequeue();
            ChangeColor();
        }
    }

    public void ActivateAlphabet(char newChar)
    {
        var alphabet = GetInactiveAlphabet();
        alphabet.Activate(newChar);
        var text = alphabet.GetComponent<TextMeshPro>();
        text.color = new Color(1.0f - alphabetSpeedModifiers.Count * 0.07f,
            1.0f - alphabetSpeedModifiers.Count * 0.07f,
            1.0f - alphabetSpeedModifiers.Count * 0.07f);
    }
    
    private Alphabet GetInactiveAlphabet()
    {
        var alphabet = alphabets.Where(a => !a.IsActive).FirstOrDefault();
        if (alphabet == null)
        {
            alphabet = Instantiate(alphabetPrefab);
            alphabet.transform.SetParent(transform);
            alphabets.Add(alphabet);
        }
        return alphabet;
    }
}
