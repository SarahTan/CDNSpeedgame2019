using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float forceMultiplier;
    [SerializeField]
    private float maxSpeed;

    // Prototype for unlocking cursor
    private bool cursorLocked = true;

    // Tuple of Expiration Time and Multiplication Factor, to slow down mouse movement
    public List<Tuple<float, float>> ReticleSpeedModifiers = new List<Tuple<float, float>>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cursorLocked = !cursorLocked;
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                rb.velocity = Vector2.zero;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if (cursorLocked)
        {
            // Clamp it to ensure the position is always within the screen
            var pos = Utils.MainCam.WorldToViewportPoint(rb.position);
            pos.x = Mathf.Clamp01(pos.x);
            pos.y = Mathf.Clamp01(pos.y);
            rb.transform.position = Utils.MainCam.ViewportToWorldPoint(pos);

            var forceDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            rb.AddForce(forceDirection * forceMultiplier);
        }
        else
        {
            var targetPos = Utils.MainCam.ScreenToWorldPoint(Input.mousePosition);
            rb.transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * maxSpeed);
        }
    }
}
