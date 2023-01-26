using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Tower : MonoBehaviour, IDamageHandler
{
    public float hitPoints;
    public float currentHP;
    public Transform towerHealth;
    public GameManager gameManager;
    
    void Start()
    {
        SetupHealthPoints();
    }

    void SetupHealthPoints()
    {
        currentHP = hitPoints;

        towerHealth = transform.Find("HealthBar/Health");
        ConstraintSource source = new ConstraintSource()
        {
            sourceTransform = Camera.main.transform,
            weight = 1
        };
        towerHealth.parent.GetComponent<LookAtConstraint>().SetSource(0, source);
    }

    void Update()
    {
        towerHealth.localScale = new Vector3(currentHP / hitPoints, 1);
    }

    public void DamageCheck(float amount)
    {
        if (currentHP <= 0)
            return;

        towerHealth.parent.gameObject.SetActive(true);

        currentHP = Mathf.Clamp(currentHP - amount, 0, hitPoints);

        if (currentHP <= 0)
        {
            Destroy(gameObject);
            gameManager.GameOver();
        }        
    }
}
