using System.Collections;
using UnityEngine;

public class PlayerTwoMovement : MonoBehaviour
{
    [Header("Speed")]
    public float speed = 20f;
    public float ultSpeed = 23f;
    public float dashSpeedMultiplier = 3f;
    public float speedWhileStunned = 2f;

    [Header("Dash")]
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;

    [Header("Components")]
    public Rigidbody2D rb;
    public GameObject p2CanDash;
    public TrailRenderer dashTrail;

    private float nextDashTime = 0f;
    private float moveInput;
    private bool isDashing = false;

    private PlayerCombat playerCombat;

    void Start()
    {
        playerCombat = FindObjectOfType<PlayerCombat>();
        dashTrail.emitting = false;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("P2_Horizontal");
        bool dash = Input.GetButtonDown("P2_Dash");

        if (playerCombat.p2Stunned && speed != speedWhileStunned)
        {
            StartCoroutine(stunSlowDown(speedWhileStunned, playerCombat.stunDuration));
        }

        if (!isDashing && !playerCombat.p2IsKnockedBack)

        {
            if (playerCombat.p2UltActivated)
            {
                rb.velocity = new Vector2(moveInput * ultSpeed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
            }
        }

        if (Time.time >= nextDashTime && dash && moveInput != 0 && !playerCombat.p2Stunned)
        {
            StartCoroutine(Dash(moveInput));
        }

    }

    IEnumerator stunSlowDown(float stunSpeed, float stunDuration)
    {
        float temp = speed;
        speed = stunSpeed;
        yield return new WaitForSeconds(stunDuration);
        speed = temp;
    }

    IEnumerator Dash(float direction)
    {
        isDashing = true;
        p2CanDash.SetActive(false);
        dashTrail.emitting = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.velocity = new Vector2(direction * speed * dashSpeedMultiplier, rb.velocity.y);
            yield return null;
        }

        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
        dashTrail.emitting = false;

        yield return new WaitForSeconds(dashCooldown);
        p2CanDash.SetActive(true);
    }
}