using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager instance;

    public int GoldCount { get; set; }
    public int[] startGoldCount;
    public TextMeshProUGUI goldCountText;
    private RectTransform goldCountRT;

    public delegate void UpgradeReady();
    public static event UpgradeReady upgradeReadyEvent;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        goldCountRT = goldCountText.GetComponent<RectTransform>();

        UpdateGoldCount(startGoldCount[ModeDetails.currentCardOpen]);
    }

    public void UpdateGoldCount(int amount)
    {
        int initialGoldCount = GoldCount;
        //GoldCount += Mathf.RoundToInt(amount * (1 + ModeDetails.currentCardOpen * .25f));
        GoldCount += amount;

        LeanTween.value(initialGoldCount, GoldCount, .3f).setOnUpdate(x => goldCountText.text = Mathf.RoundToInt(x).ToString());
        goldCountRT.LeanScale(Vector3.one * 1.25f, .2f).setOnComplete(() => goldCountRT.LeanScale(Vector3.one, .2f));

        upgradeReadyEvent?.Invoke();
    }
}
