﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlphabetManager : Singleton<AlphabetManager>
{
    #region Fields

    [SerializeField]
    private Alphabet alphabetPrefab;

    private List<Alphabet> alphabets = new List<Alphabet>();

    #endregion
    
    public void ActivateAlphabet(char newChar)
    {
        var alphabet = GetInactiveAlphabet();
        alphabet.Activate(newChar);
    }
    
    private Alphabet GetInactiveAlphabet()
    {
        // TODO: Should we limit the max number of active alphabets?
        var alphabet = alphabets.Where(a => !a.IsActive).FirstOrDefault();
        if (alphabet == null)
        {
            alphabet = Instantiate(alphabetPrefab);
            alphabet.transform.parent = transform;
            alphabets.Add(alphabet);
        }
        return alphabet;
    }
}