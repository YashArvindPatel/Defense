using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public float damagePerTick = 3f;
    public GameObject lightningParticle;

    private void Start()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            if (other != null && other.transform.parent.TryGetComponent<IDamageHandler>(out IDamageHandler damageHandler))
            {
                damageHandler.DamageCheck(damagePerTick);
                other.transform.parent.GetComponent<MonsterController>().TemporarySpeedReduction();
                GameObject particle = Instantiate(lightningParticle, other.bounds.center, Quaternion.Euler(-90f, 0, 0));
                particle.LeanValue(0, 1, 2f).setDestroyOnComplete(true);
            }
        }
    }
}
