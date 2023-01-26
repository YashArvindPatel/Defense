using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Turret turret;

    public float attackSpeed;
    public float attackRange;
    public float damagePerAttack;

    public float atkSpeedRate;
    public float atkRangeRate;
    public float atkDamageRate;

    public int cost;
    public int SCost = 25, RCost = 25, DCost = 25;

    public Turret.TargetType targetType;

    private SphereCollider rangeCollider;
    //private BoxCollider detectionCollider;

    public float attackTimer = 0f;
    public List<Transform> targets = new List<Transform>();

    private Animator animator;
    private ParticleSystem[] muzzleFlashes;
    public GameObject bulletFire;
    public GameObject impactParticle;
    private bool ARC;
    private bool SIEGE = false;
    private bool TOWER = false;

    private GameObject upgradeGO;
    private SpriteRenderer upgradeGOSpriteRenderer;
    private bool readyToUpgrade = false;
    private bool loop1 = true, loop2 = false;
    private float elapsedTime = 0;

    private int specialPower;

    private void OnDisable()
    {
        GoldManager.upgradeReadyEvent -= ShowUpgradeReadyOrNot;
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        muzzleFlashes = GetComponentsInChildren<ParticleSystem>();
        foreach (var item in GetComponentsInChildren<ParticleSystem>(true))
        {
            if (!item.gameObject.activeSelf)
                item.gameObject.SetActive(true);
        }
        
        SetupTurretStats();
        SetupCollider();
        SetupUpgradeReady();
    }

    void SetupUpgradeReady()
    {
        GoldManager.upgradeReadyEvent += ShowUpgradeReadyOrNot;
        upgradeGO = transform.Find("Upgrade Ready").gameObject;
        upgradeGOSpriteRenderer = upgradeGO.GetComponent<SpriteRenderer>();
        ShowUpgradeReadyOrNot();
    }

    void ShowUpgradeReadyOrNot()
    {
        int goldCount = GoldManager.instance.GoldCount;

        if (goldCount >= SCost || goldCount >= RCost || goldCount >= DCost)
            readyToUpgrade = true;
        else
            readyToUpgrade = false;

        upgradeGO.SetActive(readyToUpgrade);
    }

    void UpgradeReadyUI()
    {
        if (readyToUpgrade)
        {
            elapsedTime += Time.deltaTime;

            if (loop1)
            {
                upgradeGOSpriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(.5f, 0f, elapsedTime / .5f));

                if (upgradeGOSpriteRenderer.color.a == 0f)
                {
                    loop1 = false;
                    loop2 = true;
                    elapsedTime = 0;
                }
            }
            else if (loop2)
            {
                upgradeGOSpriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(0f, .5f, elapsedTime / .5f));

                if (upgradeGOSpriteRenderer.color.a == .5f)
                {
                    loop2 = false;
                    loop1 = true;
                    elapsedTime = 0;
                }
            }
        }
        else if (elapsedTime != 0)
            elapsedTime = 0;
    }

    void SetupTurretStats()
    {
        attackSpeed = turret.AttackSpeed;
        attackRange = turret.AttackRange;
        damagePerAttack = turret.AttackDamage;

        if (animator != null)
            animator.speed = attackSpeed;

        atkSpeedRate = turret.AtkSpeedRate;
        atkRangeRate = turret.AtkRangeRate;
        atkDamageRate = turret.AtkDamageRate;

        cost = turret.COST;

        targetType = turret.targetType;
        bulletFire = turret.bulletFire;
        impactParticle = turret.impactParticle;
        ARC = turret.ARC;
        SIEGE = turret.SIEGE;
        TOWER = turret.TOWER;

        specialPower = turret.SpecialPower;

        try
        {
            float volume = SFX.instance.audioSource.volume;
            bulletFire.GetComponent<AudioSource>().volume = volume;
            impactParticle.GetComponent<AudioSource>().volume = volume;
        }
        catch
        {
            Debug.Log("No Audio Source");
        }      
    }

    void SetupCollider()
    {
        rangeCollider = gameObject.AddComponent<SphereCollider>();
        rangeCollider.radius = attackRange / transform.localScale.x;
        rangeCollider.isTrigger = true;
        gameObject.layer = 2;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        //Setup for Detection Collider current done manually, use in case many more turrets.

        //GameObject colliderObject = new GameObject("Detection Collider", new System.Type[] { typeof(BoxCollider) })
        //{
        //    tag = "Turret"
        //};
        //colliderObject.transform.SetParent(transform, false);
        //detectionCollider = colliderObject.GetComponent<BoxCollider>();
        //detectionCollider.size = new Vector3(2, 1, 2);
    }

    //Check collision for monsters with Turret's Target type
    private void OnTriggerEnter(Collider other)
    {    
        if (targetType == Turret.TargetType.Both)
        {
            if (other.CompareTag(Turret.TargetType.Ground.ToString()) || other.CompareTag(Turret.TargetType.Air.ToString()))
            {
                targets.Add(other.transform.parent);
            }
        }
        else if (other.CompareTag(targetType.ToString()))
        {
            targets.Add(other.transform.parent);
        }      
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetType == Turret.TargetType.Both)
        {
            if (other.CompareTag(Turret.TargetType.Ground.ToString()) || other.CompareTag(Turret.TargetType.Air.ToString()))
            {
                targets.Remove(other.transform.parent);
            }
        }
        else if (other.CompareTag(targetType.ToString()))
        {
            targets.Remove(other.transform.parent);
        }
    }

    void Update()
    {
        UpgradeReadyUI();

        if (targets.Count > 0)
        {
            attackTimer += Time.deltaTime;

            if (!TOWER)
            {
                transform.LookAt(targets[0]);

                if (SIEGE)
                    transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 90, transform.localEulerAngles.z);
                else
                    transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }

            if (attackTimer >= 1 / attackSpeed)
            {
                AttackTarget(targets[0]);
            }

            if (animator != null && !animator.GetBool("Attack"))
                animator.SetBool("Attack", true);
        }
        else
        {
            if (animator != null && animator.GetBool("Attack"))
            {
                animator.SetBool("Attack", false);
            }

            attackTimer = 0f;
        }
    }

    public void LevelUp(int type)
    {
        if (type == 0)
        {          
            attackSpeed += atkSpeedRate;
            if (animator != null)
                animator.speed = attackSpeed;
            SCost += 25;
        }
        else if (type == 1)
        {         
            attackRange += atkRangeRate;
            rangeCollider.radius = attackRange / transform.localScale.x;
            RCost += 25;
        }
        else if (type == 2)
        {
            damagePerAttack += atkDamageRate;
            DCost += 25;
        }
    }

    void AttackTarget(Transform target)
    {
        attackTimer = 0f;

        if (muzzleFlashes.Length > 0 && bulletFire != null)
        {
            float delay = 0f;

            foreach (var muzzleFlash in muzzleFlashes)
            {
                muzzleFlash.Play();
                GameObject bullet = Instantiate(bulletFire, muzzleFlash.transform.position, muzzleFlash.transform.rotation);
                ProjectileTravel PT = bullet.AddComponent<ProjectileTravel>();
                PT.ARC = ARC;
                PT.target = target.gameObject;
                PT.damagePerAttack = damagePerAttack;
                PT.impactParticle = impactParticle;
                PT.delayTimer = delay;
                PT.specialPower = specialPower;

                delay += 1f / muzzleFlashes.Length;
            }          
        }
        else
        {
            if (target != null && target.TryGetComponent<IDamageHandler>(out IDamageHandler damageHandler))
            {
                damageHandler.DamageCheck(damagePerAttack);

                if (specialPower != -1 && Random.Range(0f,1f) < .33f)
                {
                    if (specialPower == 0)
                    {
                        target.GetComponent<MonsterController>().ApplyPoisonDoT();
                    }
                    else if (specialPower == 1)
                    {
                        target.GetComponent<MonsterController>().TemporarySpeedReduction();
                    }
                    else if (specialPower == 2)
                    {
                        target.GetComponent<MonsterController>().InstantDeath();
                    }
                }              
            }
        }
    }
}
