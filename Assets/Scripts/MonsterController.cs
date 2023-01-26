using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class MonsterController : MonoBehaviour, IDamageHandler
{
    public Monster monster;
    
    public Monster.AttackType attackType;
    public Monster.TargetType targetType;
    public Monster.UnitType unitType;
    public float damagePerAttack;
    public float attackRange;
    public float attackSpeed;
    public float hitPoints;
    public float currentHP;
    public Transform monsterHealth;
    public SpriteRenderer statusEffect;

    public float speed;

    public int goldDrop;

    public Vector3 capturePoint;

    private NavMeshAgent agent;
    private SphereCollider rangeCollider;
    private CapsuleCollider detectionCollider;
    private Animator animator;

    public bool attackMode = false;
    public float attackTimer = 0f;
    public Transform target;

    public bool poisoned = false;
    public bool slowed = false;
    public bool dead = false;

    public bool immuneToInstantDeath = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Move", true);
        SetupMonsterStats();
        ModeDifficultyMultiplier();
        GAME_BALANCE_MULTIPLIER();
        SetupHealthPoints();
        SetupNavMeshAgent();
        SetupCollider();
    }

    void ModeDifficultyMultiplier()
    {
        float multiplier = 1f + .5f * ModeDetails.difficultySelected + .25f * ModeDetails.currentCardOpen;

        damagePerAttack *= multiplier;
        attackRange *= multiplier;
        attackSpeed *= multiplier;
        hitPoints *= multiplier;
        speed *= multiplier;
        goldDrop = Mathf.RoundToInt(goldDrop * multiplier);
    }

    void GAME_BALANCE_MULTIPLIER()
    {
        float goldMultiplier = 2f;
        float hpMultiplier = 2f;

        hitPoints *= hpMultiplier;
        goldDrop = Mathf.RoundToInt(goldDrop * goldMultiplier);
    }

    void SetupMonsterStats()
    {
        attackType = monster.attackType;
        targetType = monster.targetType;
        unitType = monster.unitType;
        damagePerAttack = monster.AttackDamage;
        attackRange = monster.AttackRange;
        attackSpeed = monster.AttackSpeed;
        hitPoints = monster.HitPoints;
        speed = monster.Speed;
        goldDrop = monster.GoldDrop;
    }

    void SetupHealthPoints()
    {
        currentHP = hitPoints;

        monsterHealth = transform.Find("HealthBar/Health");
        statusEffect = transform.Find("HealthBar/Status Effect").GetComponent<SpriteRenderer>();
        ConstraintSource source = new ConstraintSource()
        {
            sourceTransform = Camera.main.transform,
            weight = 1
        };

        Transform monsterHealthParent = monsterHealth.parent;

        monsterHealthParent.GetComponent<LookAtConstraint>().SetSource(0, source);

        monsterHealthParent.gameObject.AddComponent<HealthBar>();
        monsterHealthParent.localScale = monsterHealthParent.localScale / transform.localScale.x;
        monsterHealthParent.gameObject.SetActive(false);
    }

    void SetupCollider()
    {
        rangeCollider = gameObject.AddComponent<SphereCollider>();
        rangeCollider.radius = attackRange/transform.localScale.z;
        rangeCollider.isTrigger = true;
        gameObject.layer = 2;

        GameObject colliderObject = new GameObject("Detection Collider", new System.Type[] { typeof(CapsuleCollider) })
        {
            tag = unitType.ToString(),
            layer = 2,                   
        };
        colliderObject.transform.SetParent(transform, false);
        detectionCollider = colliderObject.GetComponent<CapsuleCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.center = new Vector3(0, 1f, 0);
    }

    void SetupNavMeshAgent()
    {
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.agentTypeID = 0;
        agent.speed = speed;
        agent.angularSpeed = 1080;
        agent.acceleration = 100;
        agent.stoppingDistance = 0.2f;
        agent.radius = transform.localScale.x / 4;
        agent.height = transform.localScale.y / 2;
        agent.avoidancePriority = 30;
        agent.SetDestination(capturePoint);
    }

    //Currently monsters focus on destroying only Tower. Can update to check Target type to attack accordingly.
    private void OnTriggerEnter(Collider other)
    {
        if (!dead && other.CompareTag("Tower"))
        {
            if (!agent.isStopped)
                agent.isStopped = true;

            attackMode = true;
            target = other.transform;
            animator.SetBool("Move", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!dead && other.CompareTag("Tower"))
        {
            if (agent.isStopped)
                agent.isStopped = false;

            attackMode = false;
            target = null;
            animator.SetBool("Move", true);
        }
    }

    void Update()
    {
        monsterHealth.localScale = new Vector3(currentHP / hitPoints, 1);

        if (dead)
            return;   

        if (attackMode)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= 1 / attackSpeed)
            {
                AttackTarget();
            }

            transform.LookAt(target);
        }
    }

    void AttackTarget()
    {
        animator.SetTrigger("Attack");

        if (target != null && target.TryGetComponent<IDamageHandler>(out IDamageHandler damageHandler))
        {
            damageHandler.DamageCheck(damagePerAttack);
        }

        attackTimer = 0f;
    }

    public void ApplyPoisonDoT()
    {
        if (poisoned)
            return;

        poisoned = true;
        statusEffect.sprite = GameManager.instance.poisonIcon;
        GameObject particle = Instantiate(GameManager.instance.poisonParticle, transform);
        statusEffect.color = new Color(0, 255, 0, 255);
        statusEffect.gameObject.SetActive(true);
        LeanTween.delayedCall(1f, () => {
            if (this == null)
                return;
            DamageCheck(hitPoints / 50f);
        }).setLoopCount(10).setOnComplete(() =>
        {
            if (this == null)
                return;

            statusEffect.gameObject.SetActive(false);
            Destroy(particle);
            poisoned = false;
        });
    }

    public void TemporarySpeedReduction()
    {
        if (slowed)
            return;

        slowed = true;
        statusEffect.sprite = GameManager.instance.freezeIcon;
        GameObject particle = Instantiate(GameManager.instance.freezeParticle, transform);
        statusEffect.color = new Color(69, 214, 212, 255);
        statusEffect.gameObject.SetActive(true);
        agent.speed = speed / 2;
        LeanTween.delayedCall(5f, () => {
            if (agent != null)
            {
                agent.speed = speed;
                statusEffect.gameObject.SetActive(false);
                Destroy(particle);
                slowed = false;
            }
        });
    }

    public void InstantDeath()
    {
        if (dead || immuneToInstantDeath)
            return;

        statusEffect.sprite = GameManager.instance.instantDeathIcon;
        GameObject particle = Instantiate(GameManager.instance.deathParticle, transform);
        Destroy(particle, particle.GetComponent<ParticleSystem>().main.duration);
        statusEffect.color = new Color(255, 255, 255, 255);
        statusEffect.gameObject.SetActive(true);
        DamageCheck(1000000f);
    }

    public void DamageCheck(float amount)
    {
        if (currentHP <= 0)
            return;

        monsterHealth.transform.parent.gameObject.SetActive(true);

        currentHP = Mathf.Clamp(currentHP - amount, 0f, hitPoints);
        if (currentHP <= 0)
        {
            TriggerDeath();
        }
    }

    public void TriggerDeath()
    {
        GoldManager.instance.UpdateGoldCount(goldDrop);
        GameManager.instance.ReduceMonsterCount();

        if (ModeDetails.THEPLAYERLEVEL >= 50)
            GameManager.instance.DiscoverGunPart();

        LeanTween.delayedCall(.5f, () =>
        {
            statusEffect.gameObject.SetActive(false);
            monsterHealth.parent.gameObject.SetActive(false);
        });

        dead = true;
        agent.isStopped = true;
        agent.radius = agent.height = 0;
        agent.avoidancePriority = 0;
        animator.SetTrigger("Dead");
        detectionCollider.center = new Vector3(0, 1000, 0);
        Destroy(gameObject, 1.7f);
    }
}
