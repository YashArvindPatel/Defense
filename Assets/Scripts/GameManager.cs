using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //Input.backButtonLeavesApp = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject ropeParticle;

    public Sprite poisonIcon, freezeIcon, instantDeathIcon;
    public GameObject poisonParticle, freezeParticle, deathParticle;

    private Vector3 capturePoint;
    private Vector3[] spawnPoints;

    [SerializeField]
    private int waveCount = 1;
    [SerializeField]
    private int monsterDifficultyMul = 1;
    private int currentWave;
    private int monsterCount;
    private int currentMonsterCount;

    [SerializeField]
    private float timeUntilNextWave = 10f;
    private float timer;
    private bool STOP = true;

    //Cash and Exp Gain
    [SerializeField]
    private int EXPGained = 0;
    [SerializeField]
    private int CashGained = 0;

    //Wave Icons Info
    public GameObject[] waveIcons;
    public ProgressBar progressBar;

    //Monsters & Boss
    public Monster[] monsters;
    private float spawnTimeInterval = 1f;

    //Monster Book
    public GameObject monsterDiscoveredPopup;

    //Workshop Parts
    private int[] gunPartsDiscovered;
    private readonly int[] gachaProbability = new int[26] { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 9, 10, 11, 12, 13 };

    //Options - Time Scale Settings & More
    public RectTransform optionsBar;
    private bool barOut = false;
    public TextMeshProUGUI timeScaleMultiplier;

    //Pause Menu
    public CanvasGroup pauseMenu;
    private Animator pauseMenuAnimator;
    public AnimatedIconHandler iconHandler;
    bool menuOpen = false;
    bool optionExt = false;

    //Settings
    public SliderManager musicManager, sfxManager;
    public CanvasGroup blur;
    public PlayerAttack playerAttack;
    public TurretSpawn turretSpawn;

    [Header("End Screen")]
    //End Screen
    public CanvasGroup endScreen;
    public Image currentHealth;
    public TextMeshProUGUI titleText, healthText;
    public RectTransform skull1, skull2, skull3;
    public TextMeshProUGUI cashText, expText, gunPartsText;
    Tower tower;
    public CanvasGroup coinObj, optionObj, pauseButtonObj;
    public RectTransform stagesObj;
    public GameObject continueButton, gunPartIcon;

    [Header("Tutorial")]
    //Tutorial
    public Tutorial tutorialManager;

    //Level
    private int currentLevel = 1;
    private int[] monsterToSpawn;
    private bool VICTORY = false;

    private void OnEnable()
    {
        LoadSettings();
    }

    private void OnDisable()
    {
        SaveSettings();
        Tutorial.tutorialOn = false;
    }

    void Start()
    {
        //Level Details
        currentLevel = (ModeDetails.mainLevel - 1) * 10 + ModeDetails.subLevel;
        monsterToSpawn = LevelData.level[currentLevel - 1];
        waveCount = monsterToSpawn.Length;

        if (ModeDetails.difficultySelected == 2)
        {         
            if (currentLevel % 3 == 0)
                waveCount = waveCount * 2 - 1;          
            else
                waveCount = waveCount * 2;

            int[] temp = monsterToSpawn;
            monsterToSpawn = new int[waveCount];

            for (int i = 0, j = 0; i < waveCount; i++)
            {
                monsterToSpawn[i] = temp[j];

                if (i % 2 != 0)
                    j++;
            }
        }

        monsterDifficultyMul = (ModeDetails.difficultySelected + 1) * 4 + Mathf.RoundToInt(currentLevel / 3f) + ModeDetails.currentCardOpen * 2;       

        SetupWaveIcons();
        timer = timeUntilNextWave;

        capturePoint = GameObject.FindGameObjectWithTag("Capture").transform.position;
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("Spawn");
        spawnPoints = new Vector3[spawnPointObjects.Length];
        
        for (int i = 0; i < spawnPointObjects.Length; i++)
        {
            spawnPoints[i] = spawnPointObjects[i].transform.position;
        }

        currentWave = 0;
        STOP = false;

        gunPartsDiscovered = new int[14];

        pauseMenuAnimator = pauseMenu.GetComponent<Animator>();
        tower = FindObjectOfType<Tower>();
        tower.gameManager = this;

        ManageUIView(true, 0f);
        UpdateAllAudioSources();

        //Tutorial
        if (Tutorial.tutorialOn)
            tutorialManager.PlayGameTutorial(1);
    }

    public void PauseMenuToggle()
    {
        if (!menuOpen)
        {
            //pauseMenu.GetComponent<RectTransform>().LeanScale(Vector3.one, .3f).setFrom(Vector3.zero).setIgnoreTimeScale(true);
            pauseMenu.LeanAlpha(1, .3f).setIgnoreTimeScale(true);
            pauseMenu.interactable = true;
            pauseMenu.blocksRaycasts = true;
            blur.alpha = 1;
            blur.blocksRaycasts = true;
            PauseGame();
        }
        else
        {
            //pauseMenu.GetComponent<RectTransform>().LeanScale(Vector3.zero, .3f).setFrom(Vector3.one).setIgnoreTimeScale(true);
            pauseMenu.LeanAlpha(0, .3f).setIgnoreTimeScale(true);
            pauseMenu.interactable = false;
            pauseMenu.blocksRaycasts = false;
            blur.alpha = 0;
            blur.blocksRaycasts = false;
            PlayOrSpeedUpGame();

            if (optionExt)
            {
                pauseMenuAnimator.Play("Off");
                optionExt = false;
            }

            UpdateAllAudioSources();
        }

        menuOpen = !menuOpen;
    }

    public void UpdateAllAudioSources()
    {
        try
        {
            float volume = SFX.instance.audioSource.volume;

            playerAttack.bulletFire.GetComponent<AudioSource>().volume = volume;
            playerAttack.impactParticle.GetComponent<AudioSource>().volume = volume;

            foreach (var item in turretSpawn.turretControllers)
            {
                item.bulletFire.GetComponent<AudioSource>().volume = volume;
                item.impactParticle.GetComponent<AudioSource>().volume = volume;
            }

            ropeParticle.GetComponent<AudioSource>().volume = volume;
        }
        catch
        {
            Debug.Log("No Audio Source");
        }
    }

    public void PauseMenuOptions(int index)
    {
        if (index == 1)
        {
            PauseMenuToggle();
            iconHandler.ClickEvent();
        }
        else if (index == 2)
        {
            if (optionExt)
            {
                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.REVERSE_WINDOW);
                pauseMenuAnimator.Play("Off");
            }        
            else
            {
                SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
                pauseMenuAnimator.Play("On");
            }           

            optionExt = !optionExt;
        }
        else if (index == 3)
        {
            RestartLevel();            
        }
        else if (index == 4)
        {
            Time.timeScale = 1f;
            CashAndExp.AddCashAndEXP(true);
            SFX.instance.ReduceMusicOverTime(1f);
            LoadingScript.instance.LoadMenu();
        }
    }

    public void PauseGame()
    {   
        Time.timeScale = 0;
        timeScaleMultiplier.text = "x0";
    }

    public void PlayOrSpeedUpGame()
    {
        if (Time.timeScale == 0 || Time.timeScale == 3)
        {
            Time.timeScale = 1;
            timeScaleMultiplier.text = "x1";
        }
        else if (Time.timeScale == 1)
        {
            Time.timeScale = 2;
            timeScaleMultiplier.text = "x2";
        }         
        else if (Time.timeScale == 2)
        {
            Time.timeScale = 3;
            timeScaleMultiplier.text = "x3";
        }           
    }

    public void MoveInOut()
    {
        float x = optionsBar.anchoredPosition.x;

        if (!barOut)
        {
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_OPEN_TURRET);

            LeanTween.value(optionsBar.anchoredPosition.y, 820f, .5f).setOnUpdate(y => optionsBar.anchoredPosition = new Vector2(x, y)).setIgnoreTimeScale(true);
        }
        else
        {
            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_CLOSE_TURRET);

            LeanTween.value(optionsBar.anchoredPosition.y, 125f, .5f).setOnUpdate(y => optionsBar.anchoredPosition = new Vector2(x, y)).setIgnoreTimeScale(true);
        }

        barOut = !barOut;
    }

    void SetupWaveIcons()
    {
        progressBar.gameObject.SetActive(true);
        progressBar.speed = 0;

        foreach (var item in waveIcons)
        {
            item.SetActive(false);
            item.GetComponent<ProgressBar>().speed = 0;
        }

        for (int i = 0; i < (waveCount>5?5:waveCount); i++)
        {
            waveIcons[i].SetActive(true);        
        }

        waveIcons[currentWave].transform.GetChild(4).GetComponent<Image>().sprite = monsters[monsterToSpawn[currentWave]].icon;

        if (waveCount == 1)
            return;

        float x = -500f;
        float y = -50;

        float xDisplacement = 1000 / ((waveCount > 5 ? 5 : waveCount) - 1);

        for (int i = 0; i < (waveCount > 5 ? 5 : waveCount); i++)
        {
            waveIcons[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y);
            x += xDisplacement;
        }
    }

    IEnumerator GenerateMonsterWave()
    {
        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.INCOMING_WAVE);

        yield return new WaitForSeconds(.5f);

        currentWave++;

        monsterCount = currentWave + monsterDifficultyMul;
        currentMonsterCount = monsterCount;

        AddToSeenMonsters(monsterToSpawn[currentWave - 1]);

        if (currentWave == waveCount && currentLevel % 3 == 0)
        {
            for (int i = 0; i < monsterCount - 1; i++)
            {
                GameObject monster = Instantiate(monsters[monsterToSpawn[0]].prefab, spawnPoints[Random.Range(0, spawnPoints.Length)], Quaternion.identity);
                MonsterController controller = monster.AddComponent<MonsterController>();
                controller.monster = monsters[monsterToSpawn[0]];
                controller.capturePoint = capturePoint;

                yield return new WaitForSeconds(spawnTimeInterval);
            }

            yield return new WaitForSeconds(2.5f);  //  <-- Boss Timer 

            GameObject monsterB = Instantiate(monsters[monsterToSpawn[currentWave - 1]].prefab, spawnPoints[Random.Range(0, spawnPoints.Length)], Quaternion.identity);
            MonsterController controllerB = monsterB.AddComponent<MonsterController>();
            controllerB.immuneToInstantDeath = true;
            controllerB.monster = monsters[monsterToSpawn[currentWave - 1]];
            controllerB.capturePoint = capturePoint;
        }
        else
        {
            for (int i = 0; i < monsterCount; i++)
            {
                GameObject monster = Instantiate(monsters[monsterToSpawn[currentWave - 1]].prefab, spawnPoints[Random.Range(0, spawnPoints.Length)], Quaternion.identity);
                MonsterController controller = monster.AddComponent<MonsterController>();
                controller.monster = monsters[monsterToSpawn[currentWave - 1]];
                controller.capturePoint = capturePoint;

                yield return new WaitForSeconds(spawnTimeInterval);
            }
        }         
    }

    public void AddToSeenMonsters(int index)
    {
        Monster seenMonster = monsters[index];
        string seenMonsterName = seenMonster.name;
        List<Monster> listOfSeenMonsters = MainMenu.seenMonsters;

        if (!listOfSeenMonsters.Contains(seenMonster))
        {
            MainMenu.seenMonsters.Add(seenMonster);

            if (seenMonster.unitType == Monster.UnitType.Air)
                TextPopup.instance.GeneratePopup("These Monster Units are AIR Type, only a Turret with Target Type 'Air'/'Both' can detect them");

            //Indicate about new monster seen
            monsterDiscoveredPopup.SetActive(true);
            monsterDiscoveredPopup.LeanDelayedCall(monsterDiscoveredPopup.GetComponent<Animation>().clip.length + 0.5f, () => monsterDiscoveredPopup.SetActive(false));

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.SURPRISE);
        }
    }

    public void DiscoverGunPart()
    {
        if (Random.Range(0f,1f) <= .1f)
        {
            gunPartsDiscovered[gachaProbability[Random.Range(0, gachaProbability.Length)]] += 1;
        }
    }

    public void ReduceMonsterCount()
    {
        float multiplyFactor = 1 + (monsterDifficultyMul / 100f);
     
        EXPGained += Mathf.RoundToInt(50 * multiplyFactor);
        CashGained += Mathf.RoundToInt(25 * multiplyFactor);

        if (currentMonsterCount > 1)
        {
            currentMonsterCount -= 1;
            waveIcons[(currentWave > 4) ? 4 : currentWave - 1].GetComponent<ProgressBar>().currentPercent = (monsterCount - currentMonsterCount) * waveIcons[(currentWave > 4) ? 4 : currentWave - 1].GetComponent<ProgressBar>().maxValue / monsterCount;
        }
        else
        {
            currentMonsterCount = 0;
            waveIcons[(currentWave > 4) ? 4 : currentWave - 1].GetComponent<ProgressBar>().currentPercent =  waveIcons[(currentWave > 4) ? 4 : currentWave - 1].GetComponent<ProgressBar>().maxValue;

            if (currentWave < waveCount)
            {
                if (currentWave > 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        waveIcons[i].transform.GetChild(4).GetComponent<Image>().sprite = waveIcons[i + 1].transform.GetChild(4).GetComponent<Image>().sprite;
                        waveIcons[i].transform.localScale = waveIcons[i + 1].transform.localScale;
                    }

                    waveIcons[4].GetComponent<ProgressBar>().currentPercent = 0;
                    progressBar.currentPercent = 74.71805f;
                }

                waveIcons[(currentWave > 4) ? 4 : currentWave].transform.GetChild(4).GetComponent<Image>().sprite = monsters[monsterToSpawn[currentWave]].icon;

                if (currentLevel % 3 == 0 && currentWave == waveCount - 1)
                    waveIcons[(currentWave > 4) ? 4 : currentWave].transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                else
                    waveIcons[(currentWave > 4) ? 4 : currentWave].transform.localScale = new Vector3(1f, 1f, 1f);

                progressBar.speed = timeUntilNextWave / ((waveCount > 5 ? 5 : waveCount) - 1);
                STOP = false;
            }          
            else
            {
                GameOver();
            }
        }
    }

    public void GameOver()
    {
        StartCoroutine(LevelCompleted());
    }

    public IEnumerator LevelCompleted()
    {
        Time.timeScale = 1f;

        ManageUIView(false, 1f);

        SFX.instance.ReduceMusicOverTime(2f, 0.1f);
        
        yield return new WaitForSecondsRealtime(3f);

        float multiplier = 1f + .5f * ModeDetails.difficultySelected + .25f * ModeDetails.currentCardOpen;
        CashAndExp.AddCashAndEXP(cash: Mathf.RoundToInt(CashGained * multiplier), exp: Mathf.RoundToInt(EXPGained * multiplier));

        int totalGunPartsDiscovered = 0;

        if (ModeDetails.THEPLAYERLEVEL >= 50)
        {
            for (int i = 0; i < gunPartsDiscovered.Length; i++)
            {
                int gunPartCount = gunPartsDiscovered[i];
                MainMenu.gunPartsOwned[i] += gunPartCount;
                totalGunPartsDiscovered += gunPartCount;
            }

            gunPartIcon.SetActive(true);
        }
        else
        {
            gunPartIcon.SetActive(false);
        }

        float totalHP = tower.hitPoints;
        float currentHP = tower.currentHP;

        if (currentHP <= 0)
        {
            titleText.text = "DEFEAT";
            VICTORY = false;
            continueButton.SetActive(false);
        }      
        else
        {
            titleText.text = "VICTORY";
            VICTORY = true;
            continueButton.SetActive(true);

            if (ModeDetails.mainLevel == 5 && ModeDetails.subLevel == 10)
            {
                TextPopup.instance.GeneratePopup("Congratulations! You have cleared this Biome, Unlock ??? in future!");
            }
            else if (ModeDetails.mainLevel >= ModeDetails.maxMainLevel && ModeDetails.subLevel >= ModeDetails.maxSubLevel)
            {
                if (ModeDetails.subLevel == 10)
                {
                    ModeDetails.mainLevel += 1;
                    ModeDetails.subLevel = 1;
                }
                else
                    ModeDetails.subLevel += 1;

                ModeDetails.changeLevel = true;
            }                                           
        }          

        blur.alpha = 1;
        blur.blocksRaycasts = true;

        float perc = currentHP / totalHP;

        //Show Level Complete Screen

        AudioSource endScreenAudioSource = endScreen.GetComponent<AudioSource>();
        endScreenAudioSource.volume = SFX.instance.audioSource.volume;
        endScreen.interactable = true;
        endScreen.blocksRaycasts = true;
        endScreen.LeanAlpha(1, .3f).setOnComplete(() => {
            //Play Sound
            if (currentHP < totalHP) endScreenAudioSource.Play();
            //Deduce the amount of health lost on tower
            LeanTween.value(totalHP, currentHP, 1.5f * (1 - perc) + .5f).setOnUpdate(
                x =>
                {
                    if (currentHealth != null && healthText != null)
                    {
                        currentHealth.fillAmount = x / totalHP * .42f;
                        healthText.text = Mathf.RoundToInt(x / totalHP * 100) + "%";
                    }                        
                }
            ).setOnComplete(() =>
            {
                //End Sound
                if (endScreenAudioSource != null)
                    endScreenAudioSource.Stop();
                //Skulls awarded depending on amount of health left
                if (perc >= .66f)
                {
                    skull1?.gameObject.SetActive(true);
                    SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
                    skull1?.LeanScale(Vector3.one, .3f).setOnComplete(() =>
                    {
                        skull2?.gameObject.SetActive(true);
                        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
                        skull2?.LeanScale(Vector3.one, .3f).setOnComplete(() =>
                        {
                            skull3?.gameObject.SetActive(true);
                            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
                            skull3?.LeanScale(Vector3.one, .3f);
                        }
                        );
                    }
                    );
                }
                else if (perc >= .33f)
                {
                    skull1?.gameObject.SetActive(true);
                    SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
                    skull1?.LeanScale(Vector3.one, .5f).setOnComplete(() =>
                    {
                        skull2?.gameObject.SetActive(true);
                        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
                        skull2?.LeanScale(Vector3.one, .5f);
                    }
                    );
                }
                else
                {
                    skull1?.gameObject.SetActive(true);
                    SFX.instance.PlaySingleSoundClip((int)SoundIndexes.PLACE_TURRET);
                    skull1?.LeanScale(Vector3.one, .5f);
                }
                //Cash and Exp gained shown over time
                LeanTween.value(0, CashGained, 1f).setOnUpdate(x => cashText.text = Mathf.RoundToInt(x).ToString());
                LeanTween.value(0, EXPGained, 1f).setOnUpdate(x => expText.text = Mathf.RoundToInt(x).ToString());

                if (ModeDetails.THEPLAYERLEVEL >= 50 && gunPartIcon.activeSelf)
                    LeanTween.value(0, totalGunPartsDiscovered, 1f).setOnUpdate(x => gunPartsText.text = Mathf.RoundToInt(x).ToString());
            }
            );
        });
    }

    public void ManageUIView(bool turnOn, float delay)
    {
        coinObj.interactable = optionObj.interactable = pauseButtonObj.interactable = turnOn;
        coinObj.blocksRaycasts = optionObj.blocksRaycasts = pauseButtonObj.blocksRaycasts = turnOn;

        coinObj.LeanAlpha(turnOn?1:0, .6f).setDelay(delay);
        optionObj.LeanAlpha(turnOn?1:0, .6f).setDelay(delay);
        pauseButtonObj.LeanAlpha(turnOn?1:0, .6f).setDelay(delay);
        LeanTween.value(turnOn ? 200 : 0, turnOn ? 0 : 200, .6f).setDelay(delay).setOnUpdate(y => { if (stagesObj == null) { LeanTween.cancel(stagesObj); return; } stagesObj.anchoredPosition = new Vector2(0, y); });
    }

    public void RestartLevel()
    {
        if (ModeDetails.changeLevel && VICTORY)
        {
            if (ModeDetails.subLevel == 1)
            {
                ModeDetails.mainLevel -= 1;
                ModeDetails.subLevel = 10;
            }
            else
            ModeDetails.subLevel -= 1;

            ModeDetails.changeLevel = false;            
        }         

        Time.timeScale = 1f;
        SFX.instance.ReduceMusicOverTime(1f);
        LoadingScript.instance.LoadGame();
    }

    public void LoadNextLevel()
    {
        if (!ModeDetails.changeLevel)
        {
            if (ModeDetails.subLevel == 10)
            {
                ModeDetails.mainLevel += 1;
                ModeDetails.subLevel = 1;
            }
            else
                ModeDetails.subLevel += 1;
        }

        Time.timeScale = 1f;
        SFX.instance.ReduceMusicOverTime(1f);
        LoadingScript.instance.LoadGame();
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SFX.instance.ReduceMusicOverTime(1f);
        LoadingScript.instance.LoadMenu();
    }

    void Update()
    {
        if (STOP || Tutorial.tutorialOn)
            return;

        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            STOP = true;
            progressBar.speed = 0;
            timer = timeUntilNextWave;
            StartCoroutine(GenerateMonsterWave());
        }
    }

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
}
