using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards")]
public class Card : ScriptableObject
{
    public Sprite icon;
    public int levelRequirement;

    //Private Variables
    [SerializeField]
    private float progress;
    [SerializeField]
    private int difficultySelected;
    [SerializeField]
    private int mainLevel;
    [SerializeField]
    private int subLevel;

    //Properties
    public float Progress { get { return progress; } set { progress = value; } }
    public int DifficultySelected { get { return difficultySelected; } set { difficultySelected = value; } }
    public int MainLevel { get { return mainLevel; } set { mainLevel = value; } }
    public int SubLevel { get { return subLevel; } set { subLevel = value; } }
}
