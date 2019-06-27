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

    [SerializeField]
    private float slowdownDuration;
    [SerializeField]
    private float slowdownFactor;

    // Prototype for unlocking cursor
    private bool cursorLocked = true;
    private bool hardMode = false;

    // Expiration time of modifiers which slow down mouse movement - sorted
    private Queue<float> reticleSpeedModifiers = new Queue<float>();

    public void BadStuffHappens()
    {
        if (!hardMode)
        {
            Debug.Log("Make bad stuff happen to targeting.");
            reticleSpeedModifiers.Enqueue(Time.time + slowdownDuration);
        }
    }

    private void Update()
    {
        // Remove the first reticle speed modifier if it's expired
        if (reticleSpeedModifiers.Count > 0 
            && reticleSpeedModifiers.Peek() > Time.time)
        {
            reticleSpeedModifiers.Dequeue();
        }

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

            var totalSlowdown = reticleSpeedModifiers.Count * slowdownFactor;
            if (hardMode 
                || reticleSpeedModifiers.Count > 10)
            {
                hardMode = true;
                totalSlowdown = 0;
            }

            rb.AddForce(forceDirection * (forceMultiplier - totalSlowdown) * (hardMode ? -1 : 1));
        }
        else
        {
            var targetPos = Utils.MainCam.ScreenToWorldPoint(Input.mousePosition);
            rb.transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * maxSpeed);
        }
    }
}
