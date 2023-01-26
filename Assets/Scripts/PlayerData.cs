using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int playerCash;
    public int playerEXP;
    public int playerLVL;

    public int[] seenMonsters;
    public int[] ownedTurrets;
    public int[] equippedTurrets;
    public int[] gunPartsOwned;
    public bool[] gunsUnlocked;

    public int selectedGunIndex;

    public float[] attackSpeeds;
    public float[] attackRanges;
    public float[] attackDamages;
    public float[] attackSpeedRates;
    public float[] attackRangeRates;
    public float[] attackDamageRates;
    public float[] exps;
    public int[] specialPower;

    public int[] difficultySelected;
    public int[] mainLevel;
    public int[] subLevel;
    public float[] progress;

    public int year, month, day, hour, minute, second;

    public PlayerData (MainMenu mainMenu)
    {
        try
        {
            CashAndExp cashAndExp = mainMenu.cashAndExp;
            Card[] cards = mainMenu.modeDetails.cards;
            List<Monster> monsters = mainMenu.monsters;
            List<Turret> turrets = mainMenu.purchasableTurrets;
            List<Turret> ownedTurretsM = mainMenu.ownedTurrets;
            List<Turret> equippedTurretsM = MainMenu.equippedTurrets;
            int[] gunPartsOwnedM = MainMenu.gunPartsOwned;
            bool[] gunsUnlockedM = mainMenu.gunsUnlocked;
            List<Monster> seenMonstersM = MainMenu.seenMonsters;

            playerCash = cashAndExp.PlayerCash;
            playerEXP = cashAndExp.PlayerEXP;
            playerLVL = cashAndExp.PlayerLVL;

            seenMonsters = new int[seenMonstersM.Count];

            selectedGunIndex = MainMenu.SELECTED_GUN_INDEX;

            for (int i = 0; i < seenMonsters.Length; i++)
            {
                seenMonsters[i] = monsters.IndexOf(seenMonstersM[i]);
            }

            ownedTurrets = new int[ownedTurretsM.Count];

            for (int i = 0; i < ownedTurrets.Length; i++)
            {
                ownedTurrets[i] = turrets.IndexOf(ownedTurretsM[i]);
            }

            equippedTurrets = new int[equippedTurretsM.Count];

            for (int i = 0; i < equippedTurrets.Length; i++)
            {
                equippedTurrets[i] = turrets.IndexOf(equippedTurretsM[i]);
            }

            gunPartsOwned = new int[gunPartsOwnedM.Length];

            for (int i = 0; i < gunPartsOwnedM.Length; i++)
            {
                gunPartsOwned[i] = gunPartsOwnedM[i];
            }

            gunsUnlocked = new bool[gunsUnlockedM.Length];

            for (int i = 0; i < gunsUnlockedM.Length; i++)
            {
                gunsUnlocked[i] = gunsUnlockedM[i];
            }

            int length = turrets.Count;

            attackSpeeds = new float[length];
            attackRanges = new float[length];
            attackDamages = new float[length];
            attackSpeedRates = new float[length];
            attackRangeRates = new float[length];
            attackDamageRates = new float[length];
            exps = new float[length];
            specialPower = new int[length];

            for (int i = 0; i < length; i++)
            {
                Turret turret = turrets[i];
                attackSpeeds[i] = turret.AttackSpeed;
                attackRanges[i] = turret.AttackRange;
                attackDamages[i] = turret.AttackDamage;
                attackSpeedRates[i] = turret.AtkSpeedRate;
                attackRangeRates[i] = turret.AtkRangeRate;
                attackDamageRates[i] = turret.AtkDamageRate;
                exps[i] = turret.EXP;
                specialPower[i] = turret.SpecialPower;
            }

            length = cards.Length;

            difficultySelected = new int[length];
            mainLevel = new int[length];
            subLevel = new int[length];
            progress = new float[length];

            for (int i = 0; i < length; i++)
            {
                Card card = cards[i];
                difficultySelected[i] = card.DifficultySelected;
                mainLevel[i] = card.MainLevel;
                subLevel[i] = card.SubLevel;
                progress[i] = card.Progress;
            }

            System.DateTime dateTime = mainMenu.rewardCollectedDateTime;

            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
        }
        catch
        {
            Debug.Log("Error occured while saving PlayerData");
        }       
    }
}
