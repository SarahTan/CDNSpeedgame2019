using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingStar : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private new CircleCollider2D collider;

    private bool canBeDeactivated = false;

    private void Update()
    {
        if (Utils.IsVisibleInMainCam(collider.bounds))
        {
            canBeDeactivated = true;
        }
        else if(canBeDeactivated)
        { 
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        gameObject.SetActive(false);
        if (collision.gameObject.layer == (int)Layers.Player)
        {
            var player = collision.collider.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.GetHit(0.2f); // Get hit for a little bit
                player.BadStuffHappens("Typing", "Targeting");
                GameManager.Instance.OnPlayerHitEnemy();
            }
        }
    }

    public void ActivateStar(Vector3 position)
    {
        canBeDeactivated = false;
        transform.position = position;
        gameObject.SetActive(true);

        var direction = Utils.GetRandomPositionOnScreen() - position;
        rb.velocity = direction.normalized * EnemyManager.Instance.starSpeed;
    }
}
