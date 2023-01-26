using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class MainMenu : MonoBehaviour
{
    public CashAndExp cashAndExp;
    public ModeDetails modeDetails;

    [Header("Lobby")]
    //Lobby
    public HorizontalSelector infoBar;
    public RectTransform selectorSlider;
    public Coroutine selectorCoroutine = null;
    public Button nextButton;
    public Image upperButton, lowerButton;
    public CanvasGroup lobby;

    //Monster Content
    public List<Monster> monsters;
    public static List<Monster> seenMonsters;
    public Transform monsterContent;
    private Animator bookAnimator;
    private CanvasGroup monsterBookCG;
    private int currentBookIndex;
    public TextMeshProUGUI monsterName, stat1, stat2, stat3, desc;
    public Image monsterIcon, monsterPower1, monsterPower2;

    [Header("Shop")]
    //Shop
    public Transform listObject;
    private List<RectTransform> turrets;
    public List<Turret> purchasableTurrets;
    private RectTransform currentSelected;
    public RectTransform displayInfo;
    private TextMeshProUGUI nameText, baseText,rateText, descText;
    public GameObject purchaseButton;

    [Header("Equipment")]
    //Equipment
    public List<Turret> ownedTurrets;
    public static List<Turret> equippedTurrets;
    public static int SELECTED_GUN_INDEX = 3;
    public HorizontalSelector turretSelector;
    public Image displayImage;
    public Transform turretsContent, gunsContent;
    public Image turretExp;
    public RectTransform power1, power2, power3;
    public CanvasGroup power1CG, power2CG, power3CG;
    public CanvasGroup powerIcon1CG, powerIcon2CG, powerIcon3CG;
    public TextMeshProUGUI info1, info2;
    public Image turretSwapButton, gunSwapButton;
    public Image[] selectedTurrets;
    public TextMeshProUGUI[] upgradeAmounts;
    public float[] upgradeValue = new float[6] { .25f, .5f, .25f, .025f, .05f, .025f };
    public Image[] gunItemBackground;
    public GameObject[] gunItems;

    [Header("Workshop")]
    //Workshop
    public Animator unlockables;
    public ButtonManager button;
    public GameObject noWorkshopContent;
    public GameObject workshopContent;
    public GameObject gunsWorkshop, towerWorkshop;
    public Image gunSwapWorkshopButton, towerSwapWorkshopButton;
    public static int[] gunPartsOwned;
    public GameObject[] gunPartsGO;
    public GameObject[] guns;
    public bool[] gunsUnlocked;
    public ButtonManager[] gunUnlockButtons;

    [Header("Settings")]
    //Settings
    public CanvasGroup settingsMenu;
    public CanvasGroup currencyShopMenu;
    public CanvasGroup blur;
    public SliderManager musicManager;
    public SliderManager sfxManager;
    bool menuOpen = false;
    bool currencyShopOpen = false;

    [Header("Tutorial")]
    //Tutorial
    public Tutorial tutorialManager;

    //Daily Rewards
    public System.DateTime rewardCollectedDateTime;
    public GameObject rewardIndicator;

    private bool SAVE_FILE = true;

    private void OnDisable()
    {
        if (SAVE_FILE)
        {
            SaveSettings();
            SaveInfo();
        }    
    }

    private void OnApplicationQuit()
    {
        if (SAVE_FILE)
        {
            SaveInfo();
        }      
    }

    //Google Play Services

    private void Awake()
    {
        PlayGamesPlatform.Activate();
    }

    private void SignInToGooglePlayServices()
    {
        PlayGamesPlatform.Instance.Authenticate(result => { });
    }

    private void CheckForGoogleAchievementCompletion()
    {
        if (PlayGamesPlatform.Instance.IsAuthenticated())
        {
            if (modeDetails.cards[0].Progress >= 98f)
                Social.ReportProgress(GPGSIds.achievement_biome_conqueror___grasslands, 100, null);
            if (modeDetails.cards[1].Progress >= 98f)
                Social.ReportProgress(GPGSIds.achievement_biome_conqueror___snowlands, 100, null);
            if (modeDetails.cards[2].Progress >= 98f)
                Social.ReportProgress(GPGSIds.achievement_biome_conqueror__dunes, 100, null);
            if (cashAndExp.PlayerLVL >= 10)
                Social.ReportProgress(GPGSIds.achievement_no_more_a_beginner, 100, null);
        }       
    }

    void Start()
    {
        SignInToGooglePlayServices();

        Input.backButtonLeavesApp = true;
        SAVE_FILE = true;

        rewardCollectedDateTime = System.DateTime.MinValue;

        cashAndExp = FindObjectOfType<CashAndExp>();

        //Lobby
        StartSelectorCoroutine();
        upperButton.alphaHitTestMinimumThreshold = 0.1f;
        lowerButton.alphaHitTestMinimumThreshold = 0.1f;

        //Monster Content
        if(seenMonsters == null)
            seenMonsters = new List<Monster>();

        bookAnimator = monsterContent.GetComponent<Animator>();
        monsterBookCG = monsterContent.GetComponent<CanvasGroup>();

        //Shop
        nameText = displayInfo.GetChild(0).GetComponent<TextMeshProUGUI>();
        baseText = displayInfo.GetChild(1).GetComponent<TextMeshProUGUI>();
        rateText = displayInfo.GetChild(2).GetComponent<TextMeshProUGUI>();
        descText = displayInfo.GetChild(3).GetComponent<TextMeshProUGUI>();

        turrets = new List<RectTransform>();
        for (int i = 0; i < listObject.childCount; i++)
        {
            RectTransform child = listObject.GetChild(i).GetComponent<RectTransform>();
            turrets.Add(child);
        }

        //Equipment
        ownedTurrets = new List<Turret>();
        equippedTurrets = new List<Turret>();
        SELECTED_GUN_INDEX = 3;

        //Workshop
        gunsUnlocked = new bool[3];

        //Load Information
        LoadInfo();
        
        LeanTween.delayedCall(.5f, () => LoadSettings());

        cashAndExp.SetupExpAndLvl();
        InitializeMonsterBook();
        InitializeShop();
        InitializeTurretEquipment();
        InitializeSelectedTurrets();
        InitializeWorkShopContent();
        modeDetails.SetupModeDetails();

        CheckIfDailyRewardIsReady();
        CheckForGoogleAchievementCompletion();
    }

    public bool CheckIfDailyRewardIsReady()
    {
        System.TimeSpan timeSpan = System.DateTime.UtcNow - rewardCollectedDateTime;

        bool ready = timeSpan.Days >= 1;

        if (ready)
            rewardIndicator.SetActive(true);
        else
            rewardIndicator.SetActive(false);

        return ready;
    }

    public void CollectDailyReward()
    {
        System.TimeSpan timeSpan = System.DateTime.UtcNow - rewardCollectedDateTime;

        if (rewardCollectedDateTime == System.DateTime.MinValue || CheckIfDailyRewardIsReady())
        {
            cashAndExp.ChangeInCashAmount(2500);

            rewardCollectedDateTime = System.DateTime.UtcNow;

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.SURPRISE);

            TextPopup.instance.GeneratePopup("Daily Reward collected! Don't forget to check back tomorrow!");
        }
        else
        {
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);

            TextPopup.instance.GeneratePopup("Daily Reward already collected! Come back tomorrow!");
        }

        CheckIfDailyRewardIsReady();
    }
    
    void SaveInfo()
    {
        SaveSystem.SaveInfo(this);
    }

    void LoadInfo()
    {
        PlayerData data = SaveSystem.LoadInfo();

        if (data == null)
        {
            if (!Tutorial.dontPlayTutorial)
            {
                Tutorial.tutorialOn = true;
                Tutorial.dontPlayTutorial = false;
                tutorialManager.PlayTutorial(1);
            }
            
            return;
        }

        try
        {
            rewardCollectedDateTime = new System.DateTime(data.year, data.month, data.day, data.hour, data.minute, data.second);
        }
        catch
        {
            Debug.Log("Error occured while loading RewardCollectedDateTime");
        }

        try
        {
            cashAndExp.PlayerCash = data.playerCash;
        }
        catch
        {
            Debug.Log("Error occured while loading PlayerCash");
        }

        try
        {
            cashAndExp.PlayerEXP = data.playerEXP;
            cashAndExp.PlayerLVL = data.playerLVL;
        }
        catch
        {
            Debug.Log("Error occured while loading PlayerEXP & PlayerLVL");
        }

        try
        {
            SELECTED_GUN_INDEX = data.selectedGunIndex;
        }
        catch
        {
            Debug.Log("Error occured while loading SelectedGunIndex");
        }

        if (seenMonsters.Count == 0)
        {
            try
            {
                foreach (var item in data.seenMonsters)
                {
                    seenMonsters.Add(monsters[item]);
                }            
            }
            catch
            {
                Debug.Log("Error occured while loading SeenMonsters");
            }          
        }

        try
        {
            foreach (var item in data.ownedTurrets)
            {
                ownedTurrets.Add(purchasableTurrets[item]);
            }      
        }
        catch
        {
            Debug.Log("Error occured while loading OwnedTurrets");
        }    

        try
        {
            foreach (var item in data.equippedTurrets)
            {
                equippedTurrets.Add(purchasableTurrets[item]);
            }       
        }
        catch
        {
            Debug.Log("Error occured while loading EquippedTurrets");
        }

        if (gunPartsOwned == null)
        {
            gunPartsOwned = new int[14];

            try
            {
                for (int i = 0; i < data.gunPartsOwned.Length; i++)
                {
                    gunPartsOwned[i] = data.gunPartsOwned[i];
                }
            }
            catch
            {
                Debug.Log("Error occured while loading GunPartsOwned");
            }
        }

        try
        {
            for (int i = 0; i < data.gunsUnlocked.Length; i++)
            {
                gunsUnlocked[i] = data.gunsUnlocked[i];
            }
        }
        catch
        {
            Debug.Log("Error occured while loading GunsUnlocked");
        }

        for (int i = 0; i < purchasableTurrets.Count; i++)
        {
            Turret turret = purchasableTurrets[i];

            try { turret.AttackSpeed = data.attackSpeeds[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.AttackRange = data.attackRanges[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.AttackDamage = data.attackDamages[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.AtkSpeedRate = data.attackSpeedRates[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.AtkRangeRate = data.attackRangeRates[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.AtkDamageRate = data.attackDamageRates[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.EXP = data.exps[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
            try { turret.SpecialPower = data.specialPower[i]; } catch { Debug.Log("Error occured while loading TurretInfo"); }
        }       

        Card[] cards = modeDetails.cards;

        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];

            try { card.DifficultySelected = data.difficultySelected[i]; } catch { Debug.Log("Error occured while loading ModeDetails"); }
            try { card.MainLevel = data.mainLevel[i]; } catch { Debug.Log("Error occured while loading ModeDetails"); }
            try { card.SubLevel = data.subLevel[i]; } catch { Debug.Log("Error occured while loading ModeDetails"); }
            try { card.Progress = data.progress[i]; } catch { Debug.Log("Error occured while loading ModeDetails"); }
        }
    }

    #region SHOP
    public void InitializeShop()
    {
        for (int i = 0; i < purchasableTurrets.Count; i++)
        {
            Turret purchasableT = purchasableTurrets[i];
            RectTransform rectT = turrets[i];

            int unlockableLevel = purchasableT.UnlockableLevel;
            rectT.GetChild(0).GetComponent<Image>().sprite = purchasableT.icon;

            Transform lockObject = rectT.GetChild(1);
            lockObject.GetComponentInChildren<TextMeshProUGUI>().text = unlockableLevel.ToString();
            
            if (cashAndExp.PlayerLVL >= unlockableLevel)
                lockObject.gameObject.SetActive(false);
        }
    }

    public void ExpandTurretShop(int turret)
    {
        RectTransform turretRT = turrets[turret];

        if (currentSelected == turretRT)
        {
            TweenPositionsWhenClosed();
            currentSelected = null;
            return;
        }

        Turret purchasableTurret = purchasableTurrets[turret];

        if (cashAndExp.PlayerLVL < purchasableTurret.UnlockableLevel)
        {
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
            return;
        }

        if (ownedTurrets.Contains(purchasableTurret))
            purchaseButton.SetActive(false);
        else
        {
            purchaseButton.SetActive(true);
            purchaseButton.transform.localScale = Vector3.one;
        }

        nameText.text = purchasableTurret.name;
        baseText.text = "<u>Attack Speed</u>: " + purchasableTurret.baseAttackSpeed +
            "\n<u>Attack Range</u>: " + purchasableTurret.baseAttackRange +
            "\n<u>Attack Damage</u>: " + purchasableTurret.baseAttackDamage +
            "\n\n<u>Target Type</u>:   " + purchasableTurret.targetType +
            "\n<u>Build Cost</u>:   " + purchasableTurret.COST +
            "\n\n<color=red><size=125%>Cost: " + purchasableTurret.PurchasePrice;
        rateText.text = "<u>Rate</u>: " + purchasableTurret.baseAtkSpeedRate +
            "\n<u>Rate</u>: " + purchasableTurret.baseAtkRangeRate +
            "\n<u>Rate</u>: " + purchasableTurret.baseAtkDamageRate;
        descText.text = purchasableTurret.description;
      
        displayInfo.SetParent(turretRT, false);

        if (currentSelected != null)
        {
            TweenPositionsWhenClosed();
        }

        currentSelected = turretRT;
        TweenPositionsWhenOpened();      
    }

    public void TweenPositionsWhenOpened()
    {
        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_OPEN_TURRET);

        LeanTween.size(currentSelected, new Vector2(currentSelected.sizeDelta.x, 1050f), .3f);
        LeanTween.scale(currentSelected.GetChild(0).gameObject, Vector3.one * .75f, 0.3f);
        LeanTween.moveLocalY(currentSelected.GetChild(0).gameObject, 300f, .3f);
        LeanTween.delayedCall(.3f, TurnOnDisplayInfo);
    }

    void TurnOnDisplayInfo()
    {
        LeanTween.alphaCanvas(displayInfo.GetComponent<CanvasGroup>(), 1, 0.3f);
    }

    public void TweenPositionsWhenClosed()
    {
        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_CLOSE_TURRET);

        LeanTween.reset();
        LeanTween.size(currentSelected, new Vector2(currentSelected.sizeDelta.x, 150f), .3f);
        LeanTween.scale(currentSelected.GetChild(0).gameObject, Vector3.one, 0.3f);
        LeanTween.moveLocalY(currentSelected.GetChild(0).gameObject, 0f, .3f);
        displayInfo.GetComponent<CanvasGroup>().alpha = 0f;
    }   

    public void PurchaseTurret()
    {
        int index = turrets.IndexOf(currentSelected);
        Turret purchasableTurret = purchasableTurrets[index];
        int purchasePrice = purchasableTurret.PurchasePrice;

        if (!ownedTurrets.Contains(purchasableTurret))
        {
            if (cashAndExp.PlayerCash >= purchasePrice)
            {
                cashAndExp.ChangeInCashAmount(-purchasePrice);
                ownedTurrets.Add(purchasableTurret);
                RefreshTurretEquipment(purchasableTurret.name);
                purchaseButton.LeanScale(Vector3.zero, .3f);

                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PURCHASE_BTN);
            }
            else
            {
                //Indicate that player doesn't have enough cash 
                TextPopup.instance.GeneratePopup("Not enough cash to purchase turret\n<size=75%> Tip: play few games to earn cash");

                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
            }
        }              
    }
    #endregion

    #region EQUIPMENT
    public void InitializeTurretEquipment()
    {
        if (ownedTurrets.Count == 0)
        {
            turretsContent.GetChild(0).gameObject.SetActive(false);
            turretsContent.GetChild(1).gameObject.SetActive(true);
            return;
        }
        else
        {
            turretsContent.GetChild(0).gameObject.SetActive(true);
            turretsContent.GetChild(1).gameObject.SetActive(false);
        }

        UpdateGunDisplay();

        foreach (var item in ownedTurrets)
        {
            turretSelector.CreateNewItem(item.name);
            turretSelector.itemList[turretSelector.itemList.Count - 1].onValueChanged.AddListener(UpdateEquipmentDisplay);
        }

        turretSelector.saveValue = false;
        turretSelector.SetupSelector();
    }

    public void UpdateGunDisplay()
    {
        bool anyGunsUnlocked = false;

        for (int i = 0; i < gunsUnlocked.Length; i++)
        {
            if (gunsUnlocked[i])
            {
                anyGunsUnlocked = true;

                gunItems[i].SetActive(true);
            }
        }

        gunsContent.GetChild(0).gameObject.SetActive(anyGunsUnlocked ? true : false);
        gunsContent.GetChild(1).gameObject.SetActive(anyGunsUnlocked ? false : true);

        foreach (var item in gunItemBackground)
        {
            item.color = new Color32(255, 255, 255, 15);
        }

        if (SELECTED_GUN_INDEX != 3)
            gunItemBackground[SELECTED_GUN_INDEX].color = new Color32(255, 255, 255, 45);
    }

    public void RefreshTurretEquipment(string name)
    {
        if (!turretsContent.GetChild(0).gameObject.activeSelf)
        {
            turretsContent.GetChild(0).gameObject.SetActive(true);
            turretsContent.GetChild(1).gameObject.SetActive(false);
        }
           
        turretSelector.CreateNewItem(name);
        turretSelector.saveValue = false;
        turretSelector.SetupSelector();
        turretSelector.itemList[turretSelector.itemList.Count - 1].onValueChanged.AddListener(UpdateEquipmentDisplay);
    }

    public void UpgradeTurretStats(int stat)
    {
        Turret selectedTurret = ownedTurrets[turretSelector.index];
        string upgradeAmountText = upgradeAmounts[stat].text;
        int cashAmount = int.Parse(upgradeAmountText.Substring(0, upgradeAmountText.Length - 1));
        float expPoints;
            
        if (stat <= 2)
            expPoints = .0333f;     
        else
            expPoints = .00333f;

        if (cashAndExp.PlayerCash >= cashAmount)
        {
            cashAndExp.ChangeInCashAmount(-cashAmount);

            if (stat == 0)
                selectedTurret.AttackSpeed += upgradeValue[stat];
            else if (stat == 1)
                selectedTurret.AttackRange += upgradeValue[stat];
            else if (stat == 2)
                selectedTurret.AttackDamage += upgradeValue[stat];
            else if (stat == 3)
                selectedTurret.AtkSpeedRate += upgradeValue[stat];
            else if (stat == 4)
                selectedTurret.AtkRangeRate += upgradeValue[stat];
            else if (stat == 5)
                selectedTurret.AtkDamageRate += upgradeValue[stat];

            selectedTurret.EXP += expPoints;

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.UPGRADE_BTN);

            UpdateEquipmentDisplay();
        }
        else
        {
            //Indicate that player doesn't have enough cash 
            TextPopup.instance.GeneratePopup("Not enough cash to purchase upgrade\n<size=75%> Tip: play few games to earn cash");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
        }      
    }

    public void SelectTurretSpecialPower(int index)
    {
        Turret selectedTurret = ownedTurrets[turretSelector.index];

        if (index == selectedTurret.SpecialPower)
            return;

        selectedTurret.SpecialPower = index;

        UpdateEquipmentDisplay();
    }

    public void UpdateEquipmentDisplay()
    {
        if (turretSelector == null || turretSelector.itemList.Count == 0 || turretSelector.itemList.Count != ownedTurrets.Count)
            return;

        // Remove in case of performance issues
        displayImage.rectTransform.LeanScale(Vector3.one, .6f).setFrom(Vector3.zero).setEaseInOutElastic();
        power1.LeanScale(Vector3.one, .6f).setFrom(Vector3.zero).setEaseInOutElastic();
        power2.LeanScale(Vector3.one, .6f).setFrom(Vector3.zero).setEaseInOutElastic();
        power3.LeanScale(Vector3.one, .6f).setFrom(Vector3.zero).setEaseInOutElastic();

        info1.GetComponent<CanvasGroup>().LeanAlpha(1, .3f).setFrom(0).setEaseInBack();
        info2.GetComponent<CanvasGroup>().LeanAlpha(1, .3f).setFrom(0).setEaseInBack();      
        // Remove in case of performance issues

        int i = turretSelector.index;
        Turret turret = ownedTurrets[i];
        displayImage.sprite = ownedTurrets[i].icon;
        displayImage.SetNativeSize();
        info1.text = turret.AttackSpeed +
            "\n\n  " + turret.AttackRange +
            "\n\n     " + turret.AttackDamage;

        info2.text = turret.AtkSpeedRate +
            "\n\n  " + turret.AtkRangeRate +
            "\n\n     " + turret.AtkDamageRate;

        LeanTween.value(turretExp.fillAmount, turret.EXP, .3f).setOnUpdate(x => turretExp.fillAmount = x);

        float[] statsArray = new float[6]
        {
            turret.AttackSpeed,
            turret.AttackRange,
            turret.AttackDamage,
            turret.AtkSpeedRate,
            turret.AtkRangeRate,
            turret.AtkDamageRate
        };
        float[] baseStatsArray = new float[6]
        {
            turret.baseAttackSpeed,
            turret.baseAttackRange,
            turret.baseAttackDamage,
            turret.baseAtkSpeedRate,
            turret.baseAtkRangeRate,
            turret.baseAtkDamageRate
        };

        int cashAmount;
        for (int j = 0; j < 6; j++)
        {
            int multiplyFactor = Mathf.RoundToInt((statsArray[j] - baseStatsArray[j]) / upgradeValue[j]);

            if (j <= 2)
                cashAmount = 250 + 250 * multiplyFactor;
            else
                cashAmount = 100 + 75 * multiplyFactor;

            upgradeAmounts[j].text = cashAmount + "g";
        }

        float turretEXP = turret.EXP;

        if (turretEXP >= .998f)
        {
            CanvasGroup power3CG = power3.GetComponent<CanvasGroup>();
            power3CG.alpha = 1f;
            power3CG.interactable = true;
            CanvasGroup power2CG = power2.GetComponent<CanvasGroup>();
            power2CG.alpha = 1f;
            power2CG.interactable = true;
            CanvasGroup power1CG = power1.GetComponent<CanvasGroup>();
            power1CG.alpha = 1f;
            power1CG.interactable = true;
        }
        else if (turretEXP >= .666f)
        {
            CanvasGroup power3CG = power3.GetComponent<CanvasGroup>();
            power3CG.alpha = .1f;
            power3CG.interactable = false;
            CanvasGroup power2CG = power2.GetComponent<CanvasGroup>();
            power2CG.alpha = 1f;
            power2CG.interactable = true;
            CanvasGroup power1CG = power1.GetComponent<CanvasGroup>();
            power1CG.alpha = 1f;
            power1CG.interactable = true;
        }
        else if (turretEXP >= .333f)
        {
            CanvasGroup power3CG = power3.GetComponent<CanvasGroup>();
            power3CG.alpha = .1f;
            power3CG.interactable = false;
            CanvasGroup power2CG = power2.GetComponent<CanvasGroup>();
            power2CG.alpha = .1f;
            power2CG.interactable = false;
            CanvasGroup power1CG = power1.GetComponent<CanvasGroup>();
            power1CG.alpha = 1f;
            power1CG.interactable = true;
        }
        else
        {
            CanvasGroup power3CG = power3.GetComponent<CanvasGroup>();
            power3CG.alpha = .1f;
            power3CG.interactable = false;
            CanvasGroup power2CG = power2.GetComponent<CanvasGroup>();
            power2CG.alpha = .1f;
            power2CG.interactable = false;
            CanvasGroup power1CG = power1.GetComponent<CanvasGroup>();
            power1CG.alpha = .1f;
            power1CG.interactable = false;
        }

        powerIcon1CG.alpha = power1CG.interactable ? .1f : 1f;
        powerIcon2CG.alpha = power2CG.interactable ? .1f : 1f;
        powerIcon3CG.alpha = power3CG.interactable ? .1f : 1f;

        int turretSpecialPower = turret.SpecialPower;

        if (turretSpecialPower == 0)
            powerIcon1CG.alpha = 1f;
        else if (turretSpecialPower == 1)
            powerIcon2CG.alpha = 1f;
        else if (turretSpecialPower == 2)
            powerIcon3CG.alpha = 1f;     
    }

    public void ButtonColorChangeOnClick(int i)
    {
        ColorUtility.TryParseHtmlString("#16222E", out Color color1);
        ColorUtility.TryParseHtmlString("#DCDDE1", out Color color2);

        turretsContent.gameObject.SetActive(i == 0 ? true : false);
        gunsContent.gameObject.SetActive(i == 0 ? false : true);
        turretSwapButton.color = i == 0 ? color1 : color2;
        turretSwapButton.transform.GetChild(0).GetComponent<Image>().color = i == 0 ? color1 : color2;
        turretSwapButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = i == 0 ? Color.white : Color.black;
        gunSwapButton.color = i == 0 ? color2 : color1;
        gunSwapButton.transform.GetChild(0).GetComponent<Image>().color = i == 0 ? color2 : color1;
        gunSwapButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = i == 0 ? Color.black : Color.white;
    }

    public void EquipTurret()
    {
        if (turretSelector == null || turretSelector.itemList.Count == 0)
            return;

        if (equippedTurrets.Count >= 5)
            return;

        Turret selectedTurret = ownedTurrets[turretSelector.index];    

        if (equippedTurrets.Contains(selectedTurret))
        {
            TextPopup.instance.GeneratePopup("Turret already equipped");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);

            return;
        }

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_OPEN_TURRET);

        equippedTurrets.Add(selectedTurret);
        int i = equippedTurrets.Count - 1;
        selectedTurrets[i].sprite = equippedTurrets[equippedTurrets.Count - 1].icon;
        selectedTurrets[i].gameObject.SetActive(true);
        //Remove in case of performance issues
        selectedTurrets[i].rectTransform.LeanScale(Vector3.one, .5f).setFrom(Vector3.zero).setEaseInOutElastic();
        //
    }

    public void InitializeSelectedTurrets()
    {
        for (int i = 0; i < equippedTurrets.Count; i++)
        {
            selectedTurrets[i].sprite = equippedTurrets[i].icon;
            selectedTurrets[i].gameObject.SetActive(true);
        }
    }

    public void UnequipTurret()
    {
        if (turretSelector == null || turretSelector.itemList.Count == 0)
            return;

        if (equippedTurrets.Count <= 0)
            return;

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_CLOSE_TURRET);

        int i = equippedTurrets.Count - 1;
        equippedTurrets.RemoveAt(i);
        //Remove in case of performance issues
        selectedTurrets[i].rectTransform.LeanScale(Vector3.zero, .3f).setFrom(Vector3.one).setOnComplete(() => { selectedTurrets[i].gameObject.SetActive(false); selectedTurrets[i].sprite = null; }).setEaseInOutElastic();
        //And enable this
        //selectedTurrets[i].gameObject.SetActive(false);
        //selectedTurrets[i].sprite = null;
    }

    public void EquipGun(int index)
    {
        SELECTED_GUN_INDEX = index;

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_OPEN_TURRET);

        foreach (var item in gunItemBackground)
        {
            item.color = new Color32(255, 255, 255, 15);
        }

        gunItemBackground[index].color = new Color32(255, 255, 255, 45);     
    }
    #endregion

    #region LOBBY
    
    IEnumerator SelectorAutomatic()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            
            if (lobby.alpha == 1)
                nextButton.onClick.Invoke();
        }
    }

    public void StartSelectorCoroutine()
    {
        selectorCoroutine = StartCoroutine(SelectorAutomatic());
    }

    public void StopSelectorCoroutine(bool temp)
    {
        if (selectorCoroutine == null)
            return;

        StopCoroutine(selectorCoroutine);

        if (temp)
            StartSelectorCoroutine();
    }

    public void SelectorMoveRight()
    {
        if (LeanTween.tweensRunning != 0)
            return;

        StopSelectorCoroutine(true);

        float posX = selectorSlider.anchoredPosition.x;
        int count = infoBar.itemList.Count;

        infoBar.ForwardClick();

        if (infoBar.index != 0)
            selectorSlider.LeanMoveLocalX(posX - 700, .5f);
        else
            selectorSlider.LeanMoveLocalX(posX + 700 * (count - 1), .5f).setFrom(posX + 700 * count);       
    }

    public void SelectorMoveLeft()
    {
        if (LeanTween.tweensRunning != 0)
            return;

        StopSelectorCoroutine(true);

        float posX = selectorSlider.anchoredPosition.x;
        int count = infoBar.itemList.Count;

        infoBar.PreviousClick();

        if (infoBar.index != count - 1)
            selectorSlider.LeanMoveLocalX(posX + 700, .5f);
        else
            selectorSlider.LeanMoveLocalX(posX - 700 * (count - 1), .5f).setFrom(posX - 700 * count);
    }

    //Monster Content

    public void InitializeMonsterBook()
    {     
        monsterContent.GetChild(1).gameObject.SetActive(seenMonsters.Count == 0 ? false : true);
        monsterContent.GetChild(2).gameObject.SetActive(seenMonsters.Count == 0 ? true : false);

        currentBookIndex = 0;

        if (seenMonsters.Count > 0)
            UpdateMonsterBook();
    }

    void UpdateMonsterBook()
    {
        Monster monster = seenMonsters[currentBookIndex];

        monsterName.text = monster.name;
        monsterIcon.sprite = monster.icon;
        monsterIcon.SetNativeSize();
        stat1.text = "<u>Attack Speed</u>: " + monster.AttackSpeed +
            "\n<u>Attack Range</u>: " + monster.AttackRange +
            "\n<u>Attack Damage</u>: " + monster.AttackDamage;
        stat2.text = "<u>Attack Type</u>: " + monster.attackType +
            "\n<u>Target Type</u>: " + monster.targetType +
            "\n<u>Unit Type</u>: " + monster.unitType;
        stat3.text = "<u>Hit Points</u>:  " + monster.HitPoints +
            "\n<u>Move Speed</u>: " + monster.Speed +
            "\n<u>Gold Drop</u>: " + monster.GoldDrop;
        desc.text = "\"" + monster.description + "\"";
        monsterPower1.sprite = monster.icon;
        monsterPower2.sprite = monster.icon;
    }

    public void LeftBtnMonsterBook()
    {
        if (seenMonsters.Count == 0 || currentBookIndex == 0)
            return;

        currentBookIndex--;
        UpdateMonsterBook();
    }

    public void RightBtnMonsterBook()
    {
        if (seenMonsters.Count == 0 || currentBookIndex == seenMonsters.Count - 1)
            return;

        currentBookIndex++;
        UpdateMonsterBook();
    } 

    public void OpenMonsterBook()
    {
        if (monsterBookCG.alpha == 1)
            return;

        bookAnimator.Play("Book Open");
    }

    public void CloseMonsterBook()
    {
        if (monsterBookCG.alpha == 0)
            return;

        bookAnimator.Play("Book Close");
    }
    #endregion

    #region WORKSHOP

    public void InitializeWorkShopContent()
    {
        bool contentUnlocked = cashAndExp.PlayerLVL >= 50;

        noWorkshopContent.SetActive(contentUnlocked ? false : true);
        workshopContent.SetActive(contentUnlocked ? true : false);

        if (contentUnlocked)
        {
            if (gunPartsOwned == null)
                gunPartsOwned = new int[14];

            for (int i = 0; i < gunPartsOwned.Length; i++)
            {
                int gunPartsOwnedCount = gunPartsOwned[i];

                if (gunPartsOwnedCount > 0)
                {
                    GameObject gunPartsGOItem = gunPartsGO[i];
                    gunPartsGOItem.SetActive(true);
                    gunPartsGOItem.GetComponentInChildren<TextMeshProUGUI>().text = "x" + gunPartsOwnedCount;
                }                    
            }

            for (int i = 0; i < gunsUnlocked.Length; i++)
            {
                if (gunsUnlocked[i])
                {
                    guns[i].SetActive(true);
                    gunUnlockButtons[i].buttonText = "UNLOCKED";
                    gunUnlockButtons[i].GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    public void OnSneakPeekButtonClick(bool OG)
    {
        if (OG && button.buttonText == "SNEAK PEEK")
        {
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_OPEN_TURRET);
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.SURPRISE);

            unlockables.Play("OpenAnim");
            button.buttonText = "CLOSE";
            button.UpdateUI();
        }
        else if (button.buttonText == "CLOSE")
        {
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_CLOSE_TURRET);

            unlockables.Play("CloseAnim");
            button.buttonText = "SNEAK PEEK";
            button.UpdateUI();
        }
    }

    public void SwapBetweenGunAndTower(int i)
    {
        ColorUtility.TryParseHtmlString("#16222E", out Color color1);
        ColorUtility.TryParseHtmlString("#DCDDE1", out Color color2);

        gunsWorkshop.SetActive(i == 0 ? true : false);
        towerWorkshop.SetActive(i == 0 ? false : true);
        gunSwapWorkshopButton.color = i == 0 ? color1 : color2;
        gunSwapWorkshopButton.transform.GetChild(0).GetComponent<Image>().color = i == 0 ? color1 : color2;
        gunSwapWorkshopButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = i == 0 ? Color.white : Color.black;
        towerSwapWorkshopButton.color = i == 0 ? color2 : color1;
        towerSwapWorkshopButton.transform.GetChild(0).GetComponent<Image>().color = i == 0 ? color2 : color1;
        towerSwapWorkshopButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = i == 0 ? Color.black : Color.white;
    }

    public void UnlockGun(int index)
    {
        if (gunsUnlocked[index])
        {
            TextPopup.instance.GeneratePopup("Gun already unlocked, you can instead equip it from EQUIPMENT");
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
            return;
        }

        bool allGunPartsOwned = true;

        if (index == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                allGunPartsOwned = gunPartsOwned[i] > 0;

                if (!allGunPartsOwned)
                    break;
            }

            if (allGunPartsOwned)
            {             
                for (int i = 0; i < 4; i++)
                {
                    gunPartsOwned[i] -= 1;
                }
            }          
        }
        else if (index == 1)
        {
            for (int i = 4; i < 8; i++)
            {
                allGunPartsOwned = gunPartsOwned[i] > 0;

                if (!allGunPartsOwned)
                    break;
            }

            if (allGunPartsOwned)
            {
                for (int i = 4; i < 8; i++)
                {
                    gunPartsOwned[i] -= 1;
                }
            }
        }
        else if (index == 2)
        {
            for (int i = 8; i < 14; i++)
            {
                allGunPartsOwned = gunPartsOwned[i] > 0;

                if (!allGunPartsOwned)
                    break;
            }

            if (allGunPartsOwned)
            {
                for (int i = 8; i < 14; i++)
                {
                    gunPartsOwned[i] -= 1;
                }
            }
        }

        if (allGunPartsOwned)
        {
            guns[index].SetActive(true);
            gunsUnlocked[index] = true;
            TextPopup.instance.GeneratePopup("Unlocked a new Gun, you can now equip it from EQUIPMENT");
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.SURPRISE);
            InitializeWorkShopContent();
            UpdateGunDisplay();
        }
        else
        {
            TextPopup.instance.GeneratePopup("Collect all Gun Parts in order to unlock this Gun");
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
        }
    }

    #endregion

    #region SETTINGS

    public void SaveSettings()
    {
        PlayerSettings.music = musicManager.mainSlider.value;
        PlayerSettings.sfx = sfxManager.mainSlider.value;
    }

    public void LoadSettings()
    {
        musicManager.mainSlider.value = PlayerSettings.music;
        sfxManager.mainSlider.value = PlayerSettings.sfx;
    }

    public void ResetEntireProgress()
    {
        SaveSystem.ResetInfo();
        SAVE_FILE = false;
        Tutorial.dontPlayTutorial = true;
        modeDetails.ResetModeDetails();
        CashAndExp.AddCashAndEXP();

        LoadingScript.instance.LoadMenu();
    }

    public void SettingsMenuToggle()
    {
        if (currencyShopOpen)
            return;

        if (!menuOpen)
        {
            //pauseMenu.GetComponent<RectTransform>().LeanScale(Vector3.one, .3f).setFrom(Vector3.zero).setIgnoreTimeScale(true);
            settingsMenu.LeanAlpha(1, .3f).setIgnoreTimeScale(true);
            settingsMenu.interactable = true;
            settingsMenu.blocksRaycasts = true;
            blur.alpha = 1;
            blur.blocksRaycasts = true;
        }
        else
        {
            //pauseMenu.GetComponent<RectTransform>().LeanScale(Vector3.zero, .3f).setFrom(Vector3.one).setIgnoreTimeScale(true);
            settingsMenu.LeanAlpha(0, .3f).setIgnoreTimeScale(true);
            settingsMenu.interactable = false;
            settingsMenu.blocksRaycasts = false;
            blur.alpha = 0;
            blur.blocksRaycasts = false;
        }

        SFX.instance.PlaySoundClip(0);

        menuOpen = !menuOpen;
    }

    public void CurrencyShopToggle()
    {
        if (!currencyShopOpen)
        {
            //pauseMenu.GetComponent<RectTransform>().LeanScale(Vector3.one, .3f).setFrom(Vector3.zero).setIgnoreTimeScale(true);
            currencyShopMenu.LeanAlpha(1, .3f).setIgnoreTimeScale(true);
            currencyShopMenu.interactable = true;
            currencyShopMenu.blocksRaycasts = true;
            blur.alpha = 1;
            blur.blocksRaycasts = true;
        }
        else
        {
            //pauseMenu.GetComponent<RectTransform>().LeanScale(Vector3.zero, .3f).setFrom(Vector3.one).setIgnoreTimeScale(true);
            currencyShopMenu.LeanAlpha(0, .3f).setIgnoreTimeScale(true);
            currencyShopMenu.interactable = false;
            currencyShopMenu.blocksRaycasts = false;
            blur.alpha = 0;
            blur.blocksRaycasts = false;
        }

        SFX.instance.PlaySoundClip(0);

        currencyShopOpen = !currencyShopOpen;
    }

    #endregion
}
