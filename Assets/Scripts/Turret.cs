using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Turret", menuName = "Turrets")]
public class Turret : ScriptableObject
{
    //GameObjects
    public GameObject prefab;
    public GameObject bulletFire;
    public GameObject impactParticle;

    public TargetType targetType;
    public Sprite icon;

    public enum TargetType
    {
        Ground, Air, Both
    }

    //Private Variables
    [SerializeField]
    private float attackSpeed;
    [SerializeField]
    private float attackRange;
    [SerializeField]
    private float attackDamage;

    [SerializeField]
    private float atkSpeedRate;
    [SerializeField]
    private float atkRangeRate;
    [SerializeField]
    private float atkDamageRate;

    [SerializeField]
    private bool arc;
    [SerializeField]
    private bool siege;
    [SerializeField]
    private bool tower;

    [SerializeField]
    private int specialPower = -1;

    [SerializeField]
    private int cost = 200;
    [SerializeField]
    private int unlockableLevel;

    //Private Variable for Setting
    [SerializeField]
    private int purchasePrice;
    [SerializeField]
    private float exp;

    //Public Variable for Setting
    public string description;

    public float baseAttackSpeed;
    public float baseAttackRange;
    public float baseAttackDamage;

    public float baseAtkSpeedRate;
    public float baseAtkRangeRate;
    public float baseAtkDamageRate;

    //Properties
    public float AttackSpeed { get { return attackSpeed; } set { attackSpeed = Mathf.Round(value * 100) / 100; } }
    public float AttackRange { get { return attackRange; } set { attackRange = Mathf.Round(value * 100) / 100; } }
    public float AttackDamage { get { return attackDamage; } set { attackDamage = Mathf.Round(value * 100) / 100; } }

    public float AtkSpeedRate { get { return atkSpeedRate; } set { atkSpeedRate = Mathf.Round(value * 100) / 100; } }
    public float AtkRangeRate { get { return atkRangeRate; } set { atkRangeRate = Mathf.Round(value * 100) / 100; } }
    public float AtkDamageRate { get { return atkDamageRate; } set { atkDamageRate = Mathf.Round(value * 100) / 100; } }

    public bool ARC { get { return arc; } set { arc = value; } }
    public bool SIEGE { get { return siege; } set { siege = value; } }
    public bool TOWER { get { return tower; } set { tower = value; } }

    public int SpecialPower { get { return specialPower; } set { specialPower = value; } }

    public int COST { get { return cost; } set { cost = value; } }
    public int UnlockableLevel { get { return unlockableLevel; } set { unlockableLevel = value; } }

    public int PurchasePrice { get { return purchasePrice; } set { purchasePrice = value; } }
    public float EXP { get { return exp; } set { exp = Mathf.Clamp01(value); } }
}
