using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurretSpawn : MonoBehaviour
{
    private const float RADIUS_DIVISION_VALUE = 2.42105f;

    public Turret[] turrets;
    public bool turretSpawnClicked = false;
    public GameObject turretSelectUI;
    public GameObject turretStatsUI;
    public GameObject turretIndicator, turretRange;
    public Vector3 point;
    public GameObject[] turretSpawnTurrets;
    public float selectionCircleRadius = 125f;

    public GameObject statsDisplayWindow;
    public TextMeshProUGUI statsTitle, statsDetail;

    public GameObject[] options;
    public GameObject[] optionConfirmations;

    public TurretController turret;
    public RopeMaker ropeMaker;

    public ParticleSystem levelUpParticle;
    public ParticleSystem turretSetupDust;

    private GoldManager goldManager;

    public List<TurretController> turretControllers;

    public Tutorial tutorialManager;

    void Start()
    {
        turretControllers = new List<TurretController>();

        List<Turret> EQTurrets = MainMenu.equippedTurrets;
        turrets = new Turret[EQTurrets.Count];
        for (int i = 0; i < turrets.Length; i++)
        {
            turrets[i] = EQTurrets[i];
        }

        SetupSpawnUI();
        ropeMaker = FindObjectOfType<RopeMaker>();
        goldManager = GoldManager.instance;
    }

    public void SpawnTurret(int i)
    {
        int cost = turrets[i].COST;
        if (goldManager.GoldCount - cost >= 0)
        {
            goldManager.UpdateGoldCount(-cost);

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
        }
        else
        {
            //Indicate player no gold left to use ability
            TextPopup.instance.GeneratePopup("Not enough coins to purchase turret");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);

            return;
        }          

        GameObject turret = Instantiate(turrets[i].prefab, point, Quaternion.identity);

        TurretController controller = turret.AddComponent<TurretController>();    
        turretControllers.Add(controller);
        controller.turret = turrets[i];
        turretSelectUI.SetActive(false);
        turretIndicator.SetActive(false);

        //Spawn Particle Update Position & On
        turretSetupDust.transform.position = point;
        turretSetupDust.gameObject.SetActive(true);
        turretSetupDust.Play();

        if (Tutorial.tutorialOn)
            tutorialManager.PlayGameTutorial(7);
    }

    public void SpawnUI(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer != 8)
            return;

        point = hit.point;
        turretSelectUI.transform.position = Input.mousePosition;                    //For testing on PC
        //turretSelectUI.transform.position = Input.GetTouch(0).position;             //For testing on Mobile
        turretSelectUI.SetActive(true);
        turretIndicator.transform.position = new Vector3(hit.point.x, turretIndicator.transform.position.y, hit.point.z);
        turretIndicator.SetActive(true);
    }

    public void SetupSpawnUI()
    {
        foreach (var item in turretSpawnTurrets)
        {
            item.SetActive(false);
        }

        for (int i = 0; i < turrets.Length; i++)
        {
            turretSpawnTurrets[i].SetActive(true);
            turretSpawnTurrets[i].transform.GetChild(0).GetComponent<Image>().sprite = turrets[i].icon;
        }

        float setScale = 1 - 0.05f * (turrets.Length - 1);
        float degree = 360 / turrets.Length;
        float r = selectionCircleRadius;
        turretSpawnTurrets[0].GetComponent<RectTransform>().localPosition = new Vector3(0, r);
        float firstDegree = Mathf.Asin(turretSpawnTurrets[0].GetComponent<RectTransform>().localPosition.y / r) * Mathf.Rad2Deg;

        for (int i = 0; i < turrets.Length; i++)
        {
            if (i == 0)
            {
                turretSpawnTurrets[i].GetComponent<RectTransform>().localScale = new Vector3(setScale, setScale, 1);
                continue;
            }
            else
                turretSpawnTurrets[i].GetComponent<RectTransform>().localScale = new Vector3(setScale, setScale, 1);

            firstDegree -= degree;
            float x = r * Mathf.Cos(firstDegree * Mathf.Deg2Rad);
            float y = r * Mathf.Sin(firstDegree * Mathf.Deg2Rad);
            turretSpawnTurrets[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
        }
    }

    public void TurretSpawnClicked()
    {
        turretSpawnClicked = true;

        if (ropeMaker.makeRope)
            ropeMaker.makeRope = false;
    }

    public void TurretStats(TurretController turretController)
    {
        turret = turretController;
        turretStatsUI.transform.position = Camera.main.WorldToScreenPoint(turret.transform.position);
        turretStatsUI.SetActive(true);
        turretRange.transform.position = new Vector3(turret.transform.position.x, turretRange.transform.position.y, turret.transform.position.z);
        turretRange.transform.localScale = new Vector3(turret.attackRange / RADIUS_DIVISION_VALUE, turret.attackRange / RADIUS_DIVISION_VALUE, 1);
        turretRange.SetActive(true);

        //Level Up Particle Update Position & On
        levelUpParticle.transform.position = turretRange.transform.position + Vector3.up;
        levelUpParticle.gameObject.SetActive(true);

        if (Tutorial.tutorialOn)
            tutorialManager.PlayGameTutorial(8);
    }

    public void TurretOptions(int type)
    {
        ResetTurretOptions();

        if (!statsDisplayWindow.activeSelf)
            statsDisplayWindow.SetActive(true);    

        options[type].SetActive(false);
        optionConfirmations[type].SetActive(true);

        if (type == 0)
        {
            statsTitle.text = "Attack Speed";

            if (turret != null)
                statsDetail.text = "Current Attack Speed of the turret is: " + Math.Round(turret.attackSpeed, 2) + " >> " + Math.Round(turret.attackSpeed + turret.atkSpeedRate, 2) + "\n"
                    + "Cost for Upgrade is: " + turret.SCost;
        }
        else if (type == 1)
        { 
            statsTitle.text = "Attack Range";

            if (turret != null)
                statsDetail.text = "Current Attack Range of the turret is: " + Math.Round(turret.attackRange, 2) + " >> " + Math.Round(turret.attackRange + turret.atkRangeRate, 2) + "\n"
                    + "Cost for Upgrade is: " + turret.RCost;
        }
        else if (type == 2)
        {
            statsTitle.text = "Attack Damage";

            if (turret != null)
                statsDetail.text = "Current Attack Damage of the turret is: " + Math.Round(turret.damagePerAttack, 2) + " >> " + Math.Round(turret.damagePerAttack + turret.atkDamageRate, 2) + "\n"
                    + "Cost for Upgrade is: " + turret.DCost;
        }
        else if (type == 3)
        {
            statsTitle.text = "Sell Unit";

            statsDetail.text = "Sell this unit for Base cost of: " + turret.cost / 2;
        }

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);

        if (Tutorial.tutorialOn)
            tutorialManager.PlayGameTutorial(9);
    }

    public void ResetTurretOptions()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].SetActive(true);
            optionConfirmations[i].SetActive(false);
        }

        statsDisplayWindow.SetActive(false);
    }

    public void TurretChanges(int type)
    {
        if (type == 3)
        {
            goldManager.UpdateGoldCount(turret.cost / 2);

            turret.gameObject.SetActive(false);
            turretStatsUI.SetActive(false);
            turretRange.SetActive(false);
            Destroy(turret.gameObject, 10f);

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PURCHASE_BTN);
        }
        else
        {
            int amount = 25;
            if (type == 0) amount = turret.SCost;
            else if (type == 1) amount = turret.RCost;
            else if (type == 2) amount = turret.DCost;

            if (goldManager.GoldCount - amount >= 0)
            {
                goldManager.UpdateGoldCount(-amount);

                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PURCHASE_BTN);
            }
            else
            {
                //Indicate player no gold left to use ability
                TextPopup.instance.GeneratePopup("Not enough coins to purchase upgrade");

                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);

                return;
            }          

            //Level Up Particle Play
            levelUpParticle.Play();

            turret.LevelUp(type);
            ResetTurretOptions();

            if (type == 1)
                turretRange.transform.localScale = new Vector3(turret.attackRange / RADIUS_DIVISION_VALUE, turret.attackRange / RADIUS_DIVISION_VALUE, 1);

            if (Tutorial.tutorialOn)
                tutorialManager.PlayGameTutorial(10);
        }
    }

    public void TurnOffTurretUI()
    {
        turretSelectUI.SetActive(false);
        turretStatsUI.SetActive(false);
        ResetTurretOptions();
        turretIndicator.SetActive(false);
        turretRange.SetActive(false);
        //Level Up Particle Off
        levelUpParticle.gameObject.SetActive(false);
    }
}
