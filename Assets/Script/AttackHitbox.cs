using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public Collider2D hitbox;
    public bool isPlayerOne;

    private PlayerCombat playerCombat;

    void Start()
    {
        hitbox.enabled = false;
        playerCombat = FindObjectOfType<PlayerCombat>();
    }

    public void EnableHitbox()
    {
        hitbox.enabled = true;
    }
    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitbox.enabled) return; // Ignore if hitbox is disabled

        if (other.CompareTag("Player")) // Ignore fists
        {
            if (isPlayerOne)
            {

                playerCombat.p1CheckHit();
            }
            else
            {
                playerCombat.p2CheckHit();
            }

            hitbox.enabled = false; // Disable hitbox immediately after hit
        }
    }
}
