using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject[] guns;
    public ParticleSystem[] muzzleFlashes;
    public GameObject bulletFire;
    public GameObject impactParticle;
    private int selectedGunIndex = 3;

    public float[] attackSpeeds;
    public float[] attackRanges;
    public float[] damagePerAttacks;
    private SphereCollider rangeCollider;

    private Animator animator;

    public List<Transform> targets = new List<Transform>();

    private Transform player;

    public float attackTimer = 0f;
    public float extraRot = 53.43f;
    
    void Start()
    {
        selectedGunIndex = MainMenu.SELECTED_GUN_INDEX;

        player = transform.parent;

        guns[selectedGunIndex].SetActive(false);

        SetupStats();
    }

    void SetupStats()
    {
        rangeCollider = GetComponent<SphereCollider>();
        rangeCollider.radius = attackRanges[selectedGunIndex];

        animator = GetComponentInParent<Animator>();
        animator.SetFloat("AttackSpeed", attackSpeeds[selectedGunIndex] / 5);
    }

    void Update()
    {
        if (targets.Count > 0 && !animator.GetBool("Move"))
        {
            guns[selectedGunIndex].SetActive(true);

            attackTimer += Time.deltaTime;
           
            player.LookAt(targets[0]);
            Vector3 extraRotation = new Vector3(player.localEulerAngles.x, player.localEulerAngles.y + extraRot, player.localEulerAngles.z);
            player.transform.localRotation = Quaternion.Euler(extraRotation);

            if (attackTimer >= 1 / attackSpeeds[selectedGunIndex])
            {
                AttackTarget(targets[0]);
            }

            if (!animator.GetBool("Attack"))
                animator.SetBool("Attack", true);
        }
        else
        {
            if (animator.GetBool("Attack"))
            {
                animator.SetBool("Attack", false);

                guns[selectedGunIndex].SetActive(false);
            }

            attackTimer = 0f;
        }
    }

    //Check collision for monsters with Gun's Target type
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(guns[selectedGunIndex].tag))
        {
            targets.Add(other.transform.parent);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(guns[selectedGunIndex].tag))
        {
            targets.Remove(other.transform.parent);
        }
    }

    void AttackTarget(Transform target)
    {
        attackTimer = 0f;
        ParticleSystem muzzleFlash = muzzleFlashes[selectedGunIndex];

        if (muzzleFlash != null && bulletFire != null)
        {
            muzzleFlash.Play();
            GameObject bullet = Instantiate(bulletFire, muzzleFlash.transform.position, muzzleFlash.transform.rotation);
            ProjectileTravel PT = bullet.AddComponent<ProjectileTravel>();
            PT.WEAPON_BULLET = true;
            PT.target = target.gameObject;
            PT.damagePerAttack = damagePerAttacks[selectedGunIndex];
            PT.impactParticle = impactParticle;
        }
        else
        {
            if (target != null && target.TryGetComponent<IDamageHandler>(out IDamageHandler damageHandler))
            {
                damageHandler.DamageCheck(damagePerAttacks[selectedGunIndex]);
            }
        }
    }
}
