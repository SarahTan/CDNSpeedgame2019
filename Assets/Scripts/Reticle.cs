using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField]
    private new SpriteRenderer renderer;
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
    [SerializeField]
    private int maxSlowdownLimit;
    
    // Expiration time of modifiers which slow down mouse movement - sorted
    private Queue<float> reticleSpeedModifiers = new Queue<float>();
    private float lastBadStuffTime = 0;

    #region Unity Lifecycle

    private void Awake()
    {
        GameManager.Instance.GamePausedEvent += OnGamePaused;
    }

    private void FixedUpdate()
    {
        UpdateReticlePosition();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GamePausedEvent -= OnGamePaused;
        }
    }

    #endregion

    private void OnGamePaused(bool isPaused)
    {
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void UpdateReticlePosition()
    {
        // Remove the first reticle speed modifier if it's expired
        if (reticleSpeedModifiers.Count > 0
            && reticleSpeedModifiers.Peek() < Time.time)
        {
            reticleSpeedModifiers.Dequeue();
            ChangeColor();
        }

        // Clamp it to ensure the position is always within the screen
        var pos = Utils.MainCam.WorldToViewportPoint(rb.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        rb.position = Utils.MainCam.ViewportToWorldPoint(pos);

        var forceDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        var totalSlowdown = reticleSpeedModifiers.Count * slowdownFactor;
        bool hardMode = false;
        if (reticleSpeedModifiers.Count > maxSlowdownLimit)
        {
            hardMode = true;
            totalSlowdown = 0;
        }

        rb.AddForce(forceDirection * (forceMultiplier - totalSlowdown) * (hardMode ? -1 : 1));
    }

    public void BadStuffHappens()
    {
        if (lastBadStuffTime < Time.time - 0.1f) // For some reason, this fires like, 3 times at once
        {
            reticleSpeedModifiers.Enqueue(Time.time + slowdownDuration);
            lastBadStuffTime = Time.time;
            Debug.Log("Reticle slowdowns: " + reticleSpeedModifiers.Count);
            ChangeColor();
        }
    }

    private void ChangeColor()
    {
        renderer.color = new Color(1.0f - (reticleSpeedModifiers.Count * 0.8f)/maxSlowdownLimit,
            0f + (reticleSpeedModifiers.Count * 0.6f)/maxSlowdownLimit,
            0f + (reticleSpeedModifiers.Count * 0.4f)/maxSlowdownLimit);
    }
}
