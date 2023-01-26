using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;

public class CashAndExp : MonoBehaviour
{
    static int EXPGained = 0;
    static int CashGained = 0;

    [SerializeField]
    private int playerCash = 500;
    public int PlayerCash
    {
        get
        {
            return playerCash;
        }
        set
        {          
            playerCash = value;
        }
    }

    [SerializeField]
    private int playerEXP = 0;
    public int PlayerEXP
    {
        get
        {
            return playerEXP;
        }
        set
        {
            playerEXP = value;
        }
    }

    [SerializeField]
    private int playerLVL = 1;
    public int PlayerLVL
    {
        get
        {
            return playerLVL;
        }
        set
        {
            playerLVL = value;
        }
    }

    [SerializeField]
    private int EXPMultiplier = 100;

    public Image EXPBar;
    public TextMeshProUGUI cashAmount;
    public TextMeshProUGUI lvlText;

    public MainMenu mainMenu;

    void Start()
    {
        mainMenu.InitializeShop(); // <===== Testing / Check later!
        cashAmount.text = PlayerCash.ToString();
    }

    public static void AddCashAndEXP(bool reset = false, int cash = 0, int exp = 0)
    {    
        if (reset)
        {
            CashGained = 0;
            EXPGained = 0;
        }
        else
        {
            CashGained += cash;
            EXPGained += exp;
        }
    }

    public void SetupExpAndLvl()
    {
        PlayerEXP += EXPGained;

        lvlText.text = PlayerLVL.ToString();

        int EXPtoLVLUp = PlayerLVL * EXPMultiplier;
        int count = -1;
        while (PlayerEXP >= EXPtoLVLUp)
        {
            PlayerLVL += 1;
            PlayerEXP -= EXPtoLVLUp;
            EXPtoLVLUp = PlayerLVL * EXPMultiplier;
            count++;
        }

        float percent = (float)PlayerEXP / EXPtoLVLUp;

        ChangeInCashAmount(CashGained);

        if (count == -1)
            LeanTween.value(EXPBar.fillAmount, percent, percent - EXPBar.fillAmount).setOnUpdate(x => EXPBar.fillAmount = x);
        else if (count == 0)
            LeanTween.value(EXPBar.fillAmount, 1, 1 - EXPBar.fillAmount).setOnUpdate(x => EXPBar.fillAmount = x).setOnComplete(() => { lvlText.text = PlayerLVL.ToString(); LeanTween.value(0, percent, percent).setOnUpdate(x => EXPBar.fillAmount = x); });
        else if (count > 0)
            LeanTween.value(EXPBar.fillAmount, 1, 1 - EXPBar.fillAmount).setOnUpdate(x => EXPBar.fillAmount = x).setOnComplete(() => { lvlText.text = (System.Convert.ToInt32(lvlText.text) + 1).ToString(); LeanTween.value(0, 1, 1).setLoopCount(count).setOnUpdate(x => { if (x == 1) { lvlText.text = (System.Convert.ToInt32(lvlText.text) + 1).ToString(); }; EXPBar.fillAmount = x; }).setOnComplete(() => { lvlText.text = PlayerLVL.ToString(); LeanTween.value(0, percent, percent).setOnUpdate(x => EXPBar.fillAmount = x); }); });

        CashGained = 0;
        EXPGained = 0;
    }

    public void ChangeInCashAmount(int amount)
    {
        int initialPlayerCash = PlayerCash;
        PlayerCash += amount;
        
        LeanTween.value(initialPlayerCash, PlayerCash, 1f).setOnUpdate(x => cashAmount.text = Mathf.RoundToInt(x).ToString());
    }

    //In-App Purchase Methods
    private static string cashsTier1 = "5000cashs";
    private static string cashsTier2 = "10000cashs";
    private static string cashsTier3 = "50000cashs";

    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == cashsTier1)
            ChangeInCashAmount(5000);
        else if (product.definition.id == cashsTier2)
            ChangeInCashAmount(10000);
        else if (product.definition.id == cashsTier3)
            ChangeInCashAmount(50000);

        TextPopup.instance.GeneratePopup("PURCHASE SUCCESSFUL");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        TextPopup.instance.GeneratePopup("PURCHASE FAILED");
    }
}
