using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monsters")]
public class Monster : ScriptableObject
{
    //GameObject
    public GameObject prefab;
    
    //Other Variables
    public AttackType attackType;
    public TargetType targetType;
    public UnitType unitType;
    public Sprite icon;

    public enum AttackType
    {
        Melee, Ranged
    }

    public enum TargetType
    {
        Tower, Player, Both, None
    }

    public enum UnitType
    {
        Ground, Air
    }

    //Private Variables
    [SerializeField]
    private float attackSpeed;
    [SerializeField]
    private float attackRange;
    [SerializeField]
    private float attackDamage;

    [SerializeField]
    private float hitPoints;
    [SerializeField]
    private float speed;

    [SerializeField]
    private int goldDrop;

    //Public Variable for Setting
    public string description;

    //Properties
    public float AttackSpeed { get { return attackSpeed; } set { attackSpeed = Mathf.Round(value * 100) / 100; } }
    public float AttackRange { get { return attackRange; } set { attackRange = Mathf.Round(value * 100) / 100; } }
    public float AttackDamage { get { return attackDamage; } set { attackDamage = Mathf.Round(value * 100) / 100; } }

    public float HitPoints { get { return hitPoints; } set { hitPoints = Mathf.Round(value * 100) / 100; } }
    public float Speed { get { return speed; } set { speed = Mathf.Round(value * 100) / 100; } }

    public int GoldDrop { get { return goldDrop; } set { goldDrop = value; } }
}



