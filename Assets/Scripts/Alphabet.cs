using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Alphabet : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private float speed;
    [SerializeField]
    private TextMeshPro text;

    PlayerController player;
    int steps = 0;

    #endregion

    #region Properties

    private bool _isActive = false;
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            if(value != _isActive)
            {
                gameObject.SetActive(value);
                _isActive = value;
            }
        }
    }
    public char CurrentChar { get; private set; }

    #endregion

    private void Awake()
    {
        player = PlayerController.Instance;
    }

    public void Activate(char newChar)
    {
        // Update character
        CurrentChar = newChar;
        text.SetText(newChar.ToString());

        // Update position
        transform.position = player.transform.position;
        steps = 0;

        // Set active
        IsActive = true;
    }

    private void Update()
    {
        if (IsActive)
        {
            if (player.LaserIsActive)
            {
                // Use Time.fixedDeltaTime here so that the distance traveled calculation is consistent
                transform.position = Vector2.MoveTowards(player.transform.position, player.ReticleCenter, speed * Time.fixedDeltaTime * ++steps);

                if (Vector2.Distance(transform.position, player.ReticleCenter) < Mathf.Epsilon)
                {
                    // Reached the target without colliding into anything, just deactivate it
                    IsActive = false;
                }
            }
            else
            {
                IsActive = false;
            }
        }
    }

    // TODO: Collision detection
}
