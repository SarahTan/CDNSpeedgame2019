using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField]
    private new SpriteRenderer renderer;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private float maxSpeed;

    [SerializeField]
    private float slowdownDuration;
    [SerializeField]
    private float slowdownFactor;
    [SerializeField]
    private int maxSlowdownLimit;

    public float ForceMultiplier;

    // Expiration time of modifiers which slow down mouse movement - sorted
    public Queue<float> ReticleSpeedModifiers = new Queue<float>();
    private float lastBadStuffTime = 0;

    #region Unity Lifecycle
    
    private void FixedUpdate()
    {
        UpdateReticlePosition();
    }
    #endregion

    private void UpdateReticlePosition()
    {
        // Remove the first reticle speed modifier if it's expired
        if (ReticleSpeedModifiers.Count > 0
            && ReticleSpeedModifiers.Peek() < Time.time)
        {
            ReticleSpeedModifiers.Dequeue();
            ChangeColor();
        }

        // Clamp it to ensure the position is always within the screen
        var pos = Utils.MainCam.WorldToViewportPoint(rb.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        rb.position = Utils.MainCam.ViewportToWorldPoint(pos);

        var forceDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        var totalSlowdown = ReticleSpeedModifiers.Count * slowdownFactor;
        bool hardMode = false;
        if (ReticleSpeedModifiers.Count > maxSlowdownLimit)
        {
            hardMode = true;
            totalSlowdown = 0;
        }

        rb.AddForce(forceDirection * (ForceMultiplier - totalSlowdown) * (hardMode ? -1 : 1));
    }

    public void BadStuffHappens()
    {
        if (lastBadStuffTime < Time.time - 0.1f) // For some reason, this fires like, 3 times at once
        {
            ReticleSpeedModifiers.Enqueue(Time.time + slowdownDuration);
            lastBadStuffTime = Time.time;
            Debug.Log("Reticle slowdowns: " + ReticleSpeedModifiers.Count);
            ChangeColor();
        }
    }

    private void ChangeColor()
    {
        renderer.color = new Color(1.0f - (ReticleSpeedModifiers.Count * 0.8f)/maxSlowdownLimit,
            0f + (ReticleSpeedModifiers.Count * 0.6f)/maxSlowdownLimit,
            0f + (ReticleSpeedModifiers.Count * 0.4f)/maxSlowdownLimit);
    }
}
