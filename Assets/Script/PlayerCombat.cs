using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCombat : MonoBehaviour
{
    #region Variables
    [Header("For Both Players")]
    public float speed = 20f;
    public float attackForce = -45f;
    public float knockbackForce = 140f;
    public double amountOfHealth = 20;
    public float stunDuration = 1.5f;
    public float whenBlockedStunDuration = 2f;

    [Header("Rigidbody2Ds")]
    public Rigidbody2D player1;
    public Rigidbody2D player2;

    [Header("Animators")]
    public Animator p1Animator;
    public Animator p2Animator;
    public Animator p1FistAnimator;
    public Animator p2FistAnimator;
    public Animator cameraAnimator;

    [Header("UI Stuff")]
    public GameObject p1CanAttack;
    public GameObject p2CanAttack;
    public TMP_Text p1HealthDisplay;
    public TMP_Text p2HealthDisplay;

    [Header("Particles")]
    public GameObject shieldParticles;
    public ParticleSystem p1DamageParticles;
    public ParticleSystem p2DamageParticles;

    [Header("Check if...")]
    [HideInInspector] public bool p1Stunned = false;
    [HideInInspector] public bool p2Stunned = false;
    [HideInInspector] public bool p1Attacking = false;
    [HideInInspector] public bool p2Attacking = false;
    [HideInInspector] public bool p1IsKnockedBack = false;
    [HideInInspector] public bool p2IsKnockedBack = false;

    [Header("Health")]
    private double p1Life;
    private double p2Life;

    [Header("Fists")]
    private GameObject p1Fist;
    private GameObject p2Fist;

    [Header("Cooldowns")]
    private float p1Cooldown = 0f;
    private float p2Cooldown = 0f;

    [Header("Movement Inputs")]
    private float p1DirInput;
    private float p2DirInput;
    private float p1HorizontalInput;
    private float p2HorizontalInput;

    [Header("Combat Inputs")]
    private bool p1AttackPressed;
    private bool p2AttackPressed;
    private bool p1BlockPressed;
    private bool p2BlockPressed;

    [Header("Attack Bools")]
    private bool p1AttackHigh;
    private bool p1AttackMid;
    private bool p1AttackLow;
    private bool p2AttackHigh;
    private bool p2AttackMid;
    private bool p2AttackLow;

    [Header("Block Bools")]
    private bool p1BlockHigh;
    private bool p1BlockMid;
    private bool p1BlockLow;
    private bool p2BlockHigh;
    private bool p2BlockMid;
    private bool p2BlockLow;

    [Header("Cooldown Damage")]
    private float p1CooldownDamage;
    private float p2CooldownDamage;

    [Header("Scripts")]
    private AttackHitbox attackHitbox;

    #endregion

    void Start()
    {
        p1Life = amountOfHealth;
        p2Life = amountOfHealth;

        p1Fist = p1FistAnimator.gameObject;
        p2Fist = p2FistAnimator.gameObject;

        p1DamageParticles.Stop();
        p2DamageParticles.Stop();

        attackHitbox = FindObjectOfType<AttackHitbox>();
    }

    void Update()
    {
        #region Inputs
        //Gets horizontal, vertical, block, and attack inputs from both players
        p1HorizontalInput = Input.GetAxisRaw("P1_Horizontal");
        p2HorizontalInput = Input.GetAxisRaw("P2_Horizontal");
        p1DirInput = Input.GetAxisRaw("P1_Vertical");
        p2DirInput = Input.GetAxisRaw("P2_Vertical");

        p1AttackPressed = Input.GetKey(KeyCode.X);
        p2AttackPressed = Input.GetKey(KeyCode.M);
        p1BlockPressed = Input.GetKey(KeyCode.C);
        p2BlockPressed = Input.GetKey(KeyCode.Comma);
        #endregion

        #region DecrementCooldowns
        //Makes both player's attack cooldowns decrease with respect to time, while
        //making sure they do not drop below zero
        if (p1Cooldown > 0f)
        {
            p1Cooldown -= Time.deltaTime;
            //Makes sure the cooldown symbol at least appears so even if you spam you still see it
            if (p1Cooldown < 0.1f)
            {
                p1CanAttack.SetActive(true);
            }
        }
        else
        {
            p1Cooldown = 0;
        }
        if (p2Cooldown > 0f)
        {
            p2Cooldown -= Time.deltaTime;
            if (p2Cooldown < 0.1f)
            {
                p2CanAttack.SetActive(true);
            }
        }
        else
        {
            p2Cooldown = 0;
        }
        #endregion

        #region DisplayHealth
        //Health display
        p1HealthDisplay.SetText("Health " + p1Life + "/" + amountOfHealth);
        p2HealthDisplay.SetText("Health " + p2Life + "/" + amountOfHealth);
        #endregion

        //Everything players can do if not stunned. Where HandleAttack is called.
        //Doing it this way just in case I want to add something that you can't do while stunned
        if (!p1Stunned)
        {
            //If cooldown's over and you press the attack button, attack
            if (p1Cooldown == 0 && p1AttackPressed)
            {
                HandleAttack(p1DirInput, true);
            }
        } 
        if (!p2Stunned)
        {
            if (p2Cooldown == 0 && p2AttackPressed)
            {
                HandleAttack(p2DirInput, false);
            }
        }

        if (p1BlockPressed && p1Cooldown == 0)
        {
            HandleBlock(p1DirInput, true);
        }
        if (p2BlockPressed && p2Cooldown == 0)
        {
            HandleBlock(p2DirInput, false);
        }

        #region FistAndBlockAnimations
        //Indicators for if a player is holding up or down
        //Basically the fist doesn't go back to the middle. I might have to make some changes in the editor
        //like a transition from BlueHandUp/Down back to idle if "isBlueHandUp"/down is false
        if (!p1Attacking)
        {
            //First make sure that theres no direction, so that it doesn't get in the way of blocking
            p2FistAnimator.SetBool("isRedHandUp", false);
            p2FistAnimator.SetBool("isRedHandDown", false);

            //Check all the blocks
            if (p1BlockHigh)
            {
                p1FistAnimator.SetTrigger("BHighB");
            }
            else if (p1BlockLow)
            {
                p1FistAnimator.SetTrigger("BLowB");
            }
            else if (p1BlockMid)
            {
                p1FistAnimator.SetTrigger("BMidB");
            }

            else //Only change fist position if not blocking
            {
                if (p1DirInput > 0)
                {
                    p1FistAnimator.SetBool("isBlueHandUp", true);
                    p1FistAnimator.SetBool("isBlueHandDown", false);
                }
                else if (p1DirInput < 0)
                {
                    p1FistAnimator.SetBool("isBlueHandDown", true);
                    p1FistAnimator.SetBool("isBlueHandUp", false);
                }
                else
                {
                    p1FistAnimator.Play("BlueIdle", 0, 0f);
                }
            }
        }
        else //If p1 is attacking, make sure the hand isn't up or down
        {
            p1FistAnimator.SetBool("isBlueHandUp", false);
            p1FistAnimator.SetBool("isBlueHandDown", false);
        }

        if (!p2Attacking)
        {
            p2FistAnimator.SetBool("isRedHandUp", false);
            p2FistAnimator.SetBool("isRedHandDown", false);
            if (p2BlockHigh)
            {
                p2FistAnimator.SetTrigger("RHighB");
            }
            else if (p2BlockLow)
            {
                p2FistAnimator.SetTrigger("RLowB");
            }
            else if (p2BlockMid)
            {
                p2FistAnimator.SetTrigger("RMidB");
            }
            else
            {
                if (p2DirInput > 0)
                {
                    p2FistAnimator.SetBool("isRedHandUp", true);
                    p2FistAnimator.SetBool("isRedHandDown", false);
                }
                else if (p2DirInput < 0)
                {
                    p2FistAnimator.SetBool("isRedHandDown", true);
                    p2FistAnimator.SetBool("isRedHandUp", false);
                }
                else
                {
                    p2FistAnimator.Play("RedIdle", 0, 0f);
                }
            }
        }
        else
        {
            p2FistAnimator.SetBool("isRedHandUp", false);
            p2FistAnimator.SetBool("isRedHandDown", false);
        }
        #endregion

        #region ResetBlocks
        if (!p1BlockPressed)
        {
            p1BlockHigh = false;
            p1BlockMid = false;
            p1BlockLow = false;

            p1FistAnimator.ResetTrigger("BHighB");
            p1FistAnimator.ResetTrigger("BMidB");
            p1FistAnimator.ResetTrigger("BLowB");

            p1Fist.GetComponent<Collider2D>().isTrigger = true;
        }
        if (!p2BlockPressed)
        {
            p2BlockHigh = false;
            p2BlockMid = false;
            p2BlockLow = false;

            p2FistAnimator.ResetTrigger("RHighB");
            p2FistAnimator.ResetTrigger("RMidB");
            p2FistAnimator.ResetTrigger("RLowB");

            p2Fist.GetComponent<Collider2D>().isTrigger = true;
        }
        #endregion
    }
    //Sends direction of attack to p1/p2ExecuteAttack() function
    void HandleAttack(float direction, bool isPlayerOne)
    {
        if (isPlayerOne)
        {
            p1AttackHigh = p1AttackMid = p1AttackLow = false; 
            if (direction > 0) // High Attack
            {
                p1AttackHigh = true;
                p1ExecuteAttack("BHighP", 2f, -45f);
            }
            else if (direction < 0) // Low Attack
            {
                p1AttackLow = true;
                p1ExecuteAttack("BLowP", 1f, -35f);
            }
            else // Mid Attack
            {
                p1AttackMid = true;
                p1ExecuteAttack("BMidP", 1.5f, -40f);
            }
        }
        else
        {
            p2AttackHigh = p2AttackMid = p2AttackLow = false;
            if (direction > 0) // High Attack
            {
                p2AttackHigh = true;
                p2ExecuteAttack("RHighP", 2f, -45f);
            }
            else if (direction < 0) // Low Attack
            {
                p2AttackLow = true;
                p2ExecuteAttack("RLowP", 1f, -35f);
            }
            else // Mid Attack
            {
                p2AttackMid = true;
                p2ExecuteAttack("RMidP", 1.5f, -40f);
            }
        }
    }

    //These functions active animations for attacker's fist, then activate cooldown and icon,
    //and check if the punch landed. If it did, the victim takers damage and turns red for a sec
    //and is knocked back
    void p1ExecuteAttack(string animationTrigger, float cooldown, float force)
    {
        p1Attacking = true;
        p1FistAnimator.ResetTrigger("BHighP");
        p1FistAnimator.ResetTrigger("BMidP");
        p1FistAnimator.ResetTrigger("BLowP");
        p1FistAnimator.SetTrigger(animationTrigger);
        p1CanAttack.SetActive(false);
        p1Cooldown = cooldown;
        p1CooldownDamage = cooldown;

        StartCoroutine(ResetAttackState(true, cooldown - 0.25f));
    }
    void p2ExecuteAttack(string animationTrigger, float cooldown, float force)
    {
        p2Attacking = true;
        p2FistAnimator.SetTrigger(animationTrigger);
        p2CanAttack.SetActive(false);
        p2Cooldown = cooldown;
        p2CooldownDamage = cooldown;

        StartCoroutine(ResetAttackState(false, cooldown - 0.25f));

    }

    public void p1CheckHit()
    {
        if (p1LandsHit())
        {
            cameraAnimator.SetTrigger("shake");
            p2DamageParticles.Play();

            p1AttackHigh = p1AttackMid = p1AttackLow = false;
            p2Life -= p1CooldownDamage; // Damage scales with cooldown
            p2Animator.SetTrigger("RedDamageTaken");
            StartCoroutine(StunPlayer(false, stunDuration));

            if (p2Life <= 0)
            {
                SceneManager.LoadScene("victory1");
            }
        }
    }
    public void p2CheckHit()
    {
        if (p2LandsHit())
        {
            cameraAnimator.SetTrigger("shake");
            p1DamageParticles.Play();

            p2AttackHigh = p2AttackMid = p2AttackLow = false;
            p1Life -= p2CooldownDamage; // Damage scales with cooldown
            p1Animator.SetTrigger("BlueDamageTaken");
            StartCoroutine(StunPlayer(true, stunDuration));

            if (p1Life <= 0)
            {
                SceneManager.LoadScene("victory2");
            }
        }
    }

    void HandleBlock(float direction, bool isPlayerOne)
    {
        if (isPlayerOne)
        {
            p1Fist.GetComponent<Collider2D>().isTrigger = false;

            p1BlockHigh = false;
            p1BlockMid = false;
            p1BlockLow = false;

            if (direction > 0)
            {
                p1BlockHigh = true;
            }            
            else if (direction < 0)
            {
                p1BlockLow = true;
            }            
            else
            {
                p1BlockMid = true;
            }
        }
        else
        {
            p2Fist.GetComponent<Collider2D>().isTrigger = false;

            p2BlockHigh = false; 
            p2BlockMid = false; 
            p2BlockLow = false;

            if (direction > 0)
            {
                p2BlockHigh = true;
            }
            else if (direction < 0)
            {
                p2BlockLow = true;
            }
            else
            {
                p2BlockMid = true;
            }
        }
    }

    void p1ExecuteBlock(string animationTrigger)
    {
        p1FistAnimator.SetTrigger(animationTrigger);
    }    
    void p2ExecuteBlock(string animationTrigger)
    {
        p2FistAnimator.SetTrigger(animationTrigger);
    }

    //temporary returning true; two things it needs to check for 
    //1. collision with the players hitbox (not the fist)
    //     a. also make sure the hitbox only hits accurately to the animation
    public bool p1LandsHit()
    {
        bool noBlock = false; 

        if (p1AttackHigh && !p2BlockHigh) //Checks high attack vs not blocking high
        {
            noBlock = true;
        }
        if (p1AttackMid && !p2BlockMid) //Checks mid attack vs not blocking mid
        {
            noBlock = true;
        }
        if (p1AttackLow && !p2BlockLow) //Checks low attack vs not blocking low
        {
            noBlock = true;
        }

        if (!noBlock)
        {
            SpawnParticles(shieldParticles, p2Fist.transform.position, 2f);
            StartCoroutine(StunPlayer(true, whenBlockedStunDuration));
        }

        return noBlock;
    }
    public bool p2LandsHit()
    {
        bool noBlock = false;

        if (p2AttackHigh && !p1BlockHigh) //Checks high attack vs not blocking high
        {
            noBlock = true;
        }
        if (p2AttackMid && !p1BlockMid) //Checks mid attack vs not blocking mid
        {
            noBlock = true;
        }
        if (p2AttackLow && !p1BlockLow) //Checks low attack vs not blocking low
        {
            noBlock = true;
        }

        if (!noBlock)
        {
            SpawnParticles(shieldParticles, p1Fist.transform.position, 2f);
            StartCoroutine(StunPlayer(false, whenBlockedStunDuration));
        }

        return noBlock;
    }

    void SpawnParticles(GameObject particles, Vector3 position, float duration)
    {
        GameObject spawner = Instantiate(particles, position, Quaternion.identity);
        cameraAnimator.SetTrigger("shake");
        Destroy(spawner, duration);
    }

    #region Coroutines
    //Stuns player1/player2 (true/false) for a certain amount of seconds and turns them gray to indicate
    IEnumerator StunPlayer(bool isPlayerOne, float stunTime)
    {
        if (isPlayerOne)
        {
            p1Stunned = true;
            p1Animator.SetBool("BlueIsStunned", true);
            p1FistAnimator.SetBool("BlueIsStunned", true);
            StartCoroutine(ApplyKnockback(player1, knockbackForce, 0.25f, true));
        }
        else
        {
            p2Stunned = true;
            p2Animator.SetBool("RedIsStunned", true);
            p2FistAnimator.SetBool("RedIsStunned", true);
            StartCoroutine(ApplyKnockback(player2, knockbackForce, 0.25f, false));
        }

        //Making them gray out like this highkey doesn't work just make new animation for it

        yield return new WaitForSeconds(stunTime);

        if (isPlayerOne)
        {
            p1Stunned = false;
            p1Animator.SetBool("BlueIsStunned", false);
            p1FistAnimator.SetBool("BlueIsStunned", false);
        }
        else
        {
            p2Stunned = false;
            p2Animator.SetBool("RedIsStunned", false);
            p2FistAnimator.SetBool("RedIsStunned", false);
        }
    }

    IEnumerator ResetAttackState(bool isPlayerOne, float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        if (isPlayerOne)
        {
            p1Attacking = false;
        }
        else
        {
            p2Attacking = false;
        }
    }

    IEnumerator ApplyKnockback(Rigidbody2D playerRb, float force, float duration, bool isPlayerOne)
    {
        if (isPlayerOne)
        {
            p1IsKnockedBack = true;
        }
        else
        {
            p2IsKnockedBack = true;
        }

        float direction = isPlayerOne ? -1f : 1f;
        playerRb.AddForce(new Vector2(direction * force, 0), ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        playerRb.velocity = Vector2.zero;

        if (isPlayerOne)
        {
            p1IsKnockedBack = false;
        }
        else
        {
            p2IsKnockedBack = false;
        }

    }
    #endregion
}
