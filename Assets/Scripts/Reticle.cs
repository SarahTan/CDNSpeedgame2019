using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed;

    private void Update()
    {
        // Clamp it to ensure the position is always within the screen
        //Vector2 clampedMousePos = new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height));
        var targetPos = Utils.MainCam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * maxSpeed);
    }
}
