using UnityEngine;
using Random = UnityEngine.Random;

public class DragonBossController : MonoBehaviour
{
    public int maxHealth = 1000;
    private int currentHealth;
    private const float PHASE_2_THRESHOLD = 0.5f;
    private const float PHASE_3_THRESHOLD = 0.05f;

    public float attackCooldownPhase1 = 3.0f;
    public float attackCooldownPhase2 = 1.5f;
    private float currentAttackCooldown;
    private float timeSinceLastAttack;

    public Animator animator;
    public Rigidbody playerRb;
    public Transform playerTransform;

    public float pushForce = 50.0f;
    public float pushRate = 0.2f;
    private float timeSinceLastPush = 0f;

    public enum BossPhase { Phase1_Normal, Phase2_Fast, Phase3_Roar }
    public BossPhase currentPhase;

    void Start()
    {
        currentHealth = maxHealth;
        currentPhase = BossPhase.Phase1_Normal;
        currentAttackCooldown = attackCooldownPhase1;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        CheckPhaseTransition();

        switch (currentPhase)
        {
            case BossPhase.Phase1_Normal:
            case BossPhase.Phase2_Fast:
                HandleMeleeAttackPattern();
                break;

            case BossPhase.Phase3_Roar:
                HandleRoarAttack();
                break;
        }
    }

    void CheckPhaseTransition()
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        BossPhase nextPhase = currentPhase;

        if (healthPercentage <= PHASE_3_THRESHOLD)
        {
            nextPhase = BossPhase.Phase3_Roar;
        }
        else if (healthPercentage <= PHASE_2_THRESHOLD)
        {
            nextPhase = BossPhase.Phase2_Fast;
        }

        if (nextPhase != currentPhase)
        {
            currentPhase = nextPhase;

            if (currentPhase == BossPhase.Phase2_Fast)
            {
                currentAttackCooldown = attackCooldownPhase2;
            }
            else if (currentPhase == BossPhase.Phase1_Normal)
            {
                currentAttackCooldown = attackCooldownPhase1;
            }
            else if (currentPhase == BossPhase.Phase3_Roar)
            {
                animator.SetTrigger("scream");
            }
        }
    }

    void HandleMeleeAttackPattern()
    {
        timeSinceLastAttack += Time.deltaTime;

        if (timeSinceLastAttack >= currentAttackCooldown)
        {
            timeSinceLastAttack = 0f;

            if (Random.value > 0.5f)
            {
                animator.SetTrigger("Basic Attack");
            }
            else
            {
                animator.SetTrigger("Claw Attack");
            }
        }
    }

    void HandleRoarAttack()
    {
        timeSinceLastPush += Time.deltaTime;

        if (timeSinceLastPush >= pushRate)
        {
            timeSinceLastPush = 0f;

            if (playerTransform != null && playerRb != null)
            {
                Vector3 directionToPlayer = playerTransform.position - transform.position;
                directionToPlayer.Normalize();

                // Pushes the player away from the dragon
                playerRb.AddForce(directionToPlayer * pushForce, ForceMode.Impulse);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            animator.SetTrigger("Die");
            enabled = false;
        }
    }
}