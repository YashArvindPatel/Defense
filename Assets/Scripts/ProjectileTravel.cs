using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTravel : MonoBehaviour
{
    public bool WEAPON_BULLET = false;
    public bool ARC = false;

    private Vector3 startPoint;
    private Vector3 midPoint;
    private Vector3 targetPoint;

    private float arcSpeed = 0.5f;
    private float nonArcSpeed = 30f;
    private float time;

    //Damage Info
    public GameObject target;
    public float damagePerAttack;

    //Particle
    public GameObject impactParticle;
    public ParticleSystem myParticle;

    //Delay
    public float delayTimer;

    //Special Power (In Case of Turrets)
    public int specialPower = -1;

    private void Start()
    {
        if (WEAPON_BULLET)
        {
            int gunIndex = MainMenu.SELECTED_GUN_INDEX;

            if (gunIndex == 1)
            {
                nonArcSpeed = 45f;
            }
            else if (gunIndex == 2)
            {
                nonArcSpeed = 60f;
            }
        }
        
        myParticle = GetComponent<ParticleSystem>();
        myParticle.Stop(true);

        targetPoint = target.transform.position;

        if (!ARC)
            targetPoint = new Vector3(targetPoint.x, targetPoint.y + 2, targetPoint.z);

        startPoint = transform.position;
        midPoint = Vector3.Lerp(startPoint, targetPoint, 0.5f);
        midPoint = new Vector3(midPoint.x, midPoint.y + Vector3.Distance(startPoint, targetPoint) / 2, midPoint.z);
    }

    void Update()
    {
        delayTimer -= Time.deltaTime;

        if (delayTimer > 0)
            return;

        if (myParticle.isStopped)
            myParticle.Play();

        if (ARC)
        {
            time += Time.deltaTime * arcSpeed;

            transform.position = Vector3.Lerp(Vector3.Lerp(startPoint, midPoint, time), Vector3.Lerp(midPoint, targetPoint, time), time);

            if (time >= 1)
                DamageAndDestroy();
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPoint) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, Time.deltaTime * nonArcSpeed);
            }
            else
            {
                DamageAndDestroy();
            }
        }
    }

    void DamageAndDestroy()
    {
        //ARC LOGIC remaining

        if (target != null && target.TryGetComponent<IDamageHandler>(out IDamageHandler damageHandler))
        {
            damageHandler.DamageCheck(damagePerAttack);

            if (specialPower != -1 && Random.Range(0f, 1f) <= .33f)
            {
                if (specialPower == 0)
                {
                    target.GetComponent<MonsterController>().ApplyPoisonDoT();
                }
                else if (specialPower == 1)
                {
                    target.GetComponent<MonsterController>().TemporarySpeedReduction();
                }
                else if (specialPower == 2 && Random.Range(0f, 1f) <= .33f)
                {
                    target.GetComponent<MonsterController>().InstantDeath();
                }
            }
        }

        GameObject particle = Instantiate(impactParticle, targetPoint, Quaternion.identity);

        Destroy(particle, particle.GetComponent<ParticleSystem>().main.duration);
        Destroy(gameObject);
    }
}
