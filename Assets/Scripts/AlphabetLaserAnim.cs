using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphabetLaserAnim : MonoBehaviour
{
    [SerializeField]
    private LineRenderer laser;
    [SerializeField]
    private SpriteRenderer reticle;

    private float laserIncrement = 1f;
    private void Awake()
    {
        // Start the laser
        laser.startWidth = 0.5f;
        laser.endWidth = 0f; // Taper the laser so it vanishes smoothly
        laser.startColor = new Color(0.5f, 0, 0);
        laser.endColor = new Color(0, 0, 0.5f);
    }

    private void Update()
    {
        laser.SetPosition(0, transform.position);
        laser.SetPosition(1, reticle.transform.position);

        var newR = laser.endColor.r + Time.deltaTime * laserIncrement;

        //Since we're reversing polarity, we can't afford to have 2 cycles in the wrong polarity
        if (newR > 0.9)
        {
            newR = 0.9f;
            laserIncrement = -laserIncrement;
        }
        else if (newR < 0.5)
        {
            newR = 0.5f;
            laserIncrement = -laserIncrement;
        }
        var reticleColor = reticle.color;
        laser.endColor = new Color(reticleColor.r, reticleColor.g, reticleColor.b, 0.1f);
        laser.startColor = new Color(newR - 0.2f, newR - 0.2f, newR, 0.1f);
    }

}
