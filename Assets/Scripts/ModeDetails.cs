using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using TMPro;

public class ModeDetails : MonoBehaviour
{
    //Selection Screen
    public Animator selectionScreen;
    public Card[] cards;
    public Animator[] cardAnimators;
    public CustomDropdown[] dropDowns;
    public Image[] card1, card2, card3;
    public GameObject[] locks;
    public Image[] types;
    public TextMeshProUGUI[] progress;
    public CustomDropdown[] levelSelector;
    public CashAndExp cashAndExp;
    public Sprite selectionIcon;

    //Settings
    public static int currentCardOpen = -1;
    public static int difficultySelected = 0;
    public static int mainLevel = 1;
    public static int subLevel = 1;
    public static bool changeLevel = false;

    public static int maxMainLevel = 1;
    public static int maxSubLevel = 1;

    //Testing
    public static int THEPLAYERLEVEL = 1;

    public void ResetModeDetails()
    {
        currentCardOpen = -1;
        difficultySelected = 0;
        mainLevel = 1;
        subLevel = 1;
        changeLevel = false;

        maxMainLevel = 1;
        maxSubLevel = 1;

        foreach (Card card in cards)
        {
            card.Progress = 0f;
            card.DifficultySelected = 0;
            card.MainLevel = 1;
            card.SubLevel = 1;
        }
    }

    public void SetupModeDetails()
    {
        if (changeLevel && currentCardOpen != -1)
        {
            cards[currentCardOpen].MainLevel = mainLevel;
            cards[currentCardOpen].SubLevel = subLevel;
            cards[currentCardOpen].Progress = (((mainLevel - 1) * 10 + (subLevel - 1)) / 50f) * 100f;
        }          
        
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];

            if (cashAndExp.PlayerLVL >= card.levelRequirement)
            {
                locks[i].SetActive(false);
                types[i].sprite = card.icon;
                types[i].GetComponent<Outline>().enabled = true;
                types[i].GetComponent<RectTransform>().sizeDelta = new Vector2(400f, 350f);

                progress[i].text = "Progress: " + card.Progress + "%";

                CustomDropdown customDropdown = levelSelector[i];

                int mainLevelIndex = card.MainLevel;
                int subLevelIndex = card.SubLevel;

                for (int j = mainLevelIndex; j > 0; j--)
                {
                    for (int k = subLevelIndex; k > 0; k--)
                    {
                        customDropdown.CreateNewItemFast(j + "-" + k, selectionIcon);
                        customDropdown.dropdownItems[customDropdown.dropdownItems.Count - 1].OnItemSelection.AddListener(PlayLevelSelectorSound);
                    }

                    subLevelIndex = 10;
                }

                customDropdown.saveSelected = false;
                customDropdown.SetupDropdown();
            }
        }

        changeLevel = false;
        currentCardOpen = -1;
        difficultySelected = 0;
    }

    public void PlayLevelSelectorSound()
    {
        SFX.instance.PlaySingleSoundClip(4);
    }

    public void CardOpen(int index)
    {
        if (locks[index].activeSelf)
        {
            //Indicate that player needs to level up to unlock selected mode
            TextPopup.instance.GeneratePopup("Not high enough level to unlock selected mode");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);

            return;
        }
            
        if (currentCardOpen != -1)
            cardAnimators[currentCardOpen].Play("SelectClose");
        else if (currentCardOpen == index)
            return;

        currentCardOpen = index;
        cardAnimators[currentCardOpen].Play("SelectOpen");     

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
    }

    public void LoadLevel()
    {
        if (currentCardOpen == -1)
        {
            //Indicate that player needs to equip a turret
            TextPopup.instance.GeneratePopup("Please select a mode to play before starting");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
            return;
        }

        difficultySelected = dropDowns[currentCardOpen].selectedItemIndex;

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);

        //Testing
        THEPLAYERLEVEL = cashAndExp.PlayerLVL;

        mainLevel = int.Parse(levelSelector[currentCardOpen].selectedText.text.Split('-')[0]);
        subLevel = int.Parse(levelSelector[currentCardOpen].selectedText.text.Split('-')[1]);

        maxMainLevel = cards[currentCardOpen].MainLevel;
        maxSubLevel = cards[currentCardOpen].SubLevel;

        SFX.instance.ReduceMusicOverTime(1f);
        LoadingScript.instance.LoadGame();
    }

    public void LoadSelectionScreen()
    {
        if (MainMenu.equippedTurrets.Count == 0)
        {
            //Indicate that player needs to equip a turret
            TextPopup.instance.GeneratePopup("You need atleast one turret equipped before continuing\n<size=75%> Tip: equip them from the equipment tab");

            SFX.instance.PlaySingleSoundClip((int)SoundIndexes.ERROR_BTN);
            return;
        }

        int count = MainMenu.equippedTurrets.Count;

        for (int i = 0; i < 5; i++)
        {
            if (i > count - 1)
            {
                card1[i].gameObject.SetActive(false);
                card2[i].gameObject.SetActive(false);
                card3[i].gameObject.SetActive(false);
            }
            else
            {
                Turret turret = MainMenu.equippedTurrets[i];

                card1[i].sprite = turret.icon;
                card2[i].sprite = turret.icon;
                card3[i].sprite = turret.icon;

                card1[i].gameObject.SetActive(true);
                card2[i].gameObject.SetActive(true);
                card3[i].gameObject.SetActive(true);
            }         
        }

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);

        selectionScreen.Play("ScreenIn");
    }

    public void UnloadSelectionScreen()
    {
        selectionScreen.Play("ScreenOut");

        if (currentCardOpen != -1)
            cardAnimators[currentCardOpen].Play("SelectClose");

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.WINDOW_BTN);
    }
}
