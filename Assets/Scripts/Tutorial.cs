using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Transform tutorialPanel;
    //Main Menu Part
    public RectTransform pointer;
    public RectTransform playButton;
    public RectTransform equipmentButton;
    public RectTransform purchaseButton;
    public RectTransform turretBanner;
    public RectTransform buyButton;
    public RectTransform addButton;
    public RectTransform lobbyButton;
    public RectTransform windowManager;
    //Game Part
    public RectTransform stages;
    public RectTransform loadButton;
    public RectTransform ropeSetup;
    public RectTransform turretSetup;

    public int tutorialCount = 1;
    public static bool tutorialOn = false;
    public static bool dontPlayTutorial = false;
    private Transform initialParent;
    public GameManager gameManager;
    
    public void PlayGameTutorial(int value)
    {
        if (!tutorialOn)
            return;

        if (tutorialCount == 1 && value == 1)
            TutorialV1();
        else if (tutorialCount == 3 && value == 3)
            TutorialV3();
        else if (tutorialCount == 4 && value == 4)
            TutorialV4();
        else if (tutorialCount == 5 && value == 5)
            TutorialV5();
        else if (tutorialCount == 6 && value == 6)
            TutorialV6();
        else if (tutorialCount == 7 && value == 7)
            TutorialV7();
        else if (tutorialCount == 8 && value == 8)
            TutorialV8();
        else if (tutorialCount == 9 && value == 9)
            TutorialV9();
        else if (tutorialCount == 10)
            TutorialVEnd();
    }

    private void TutorialV1()
    {
        gameManager.optionObj.interactable = false;
        pointer.gameObject.SetActive(true);
        pointer.position = stages.position + new Vector3(570, -270, 0);
        TextPopup.instance.GeneratePopup("You can view the type of Monsters in the upcoming Wave from here");
        tutorialCount = 2;
        LeanTween.value(0, 1, 6f).setOnComplete(() => TutorialV2());
    }

    private void TutorialV2()
    {
        gameManager.optionObj.interactable = true;
        pointer.position = loadButton.position + new Vector3(220, 0, 0);
        TextPopup.instance.GeneratePopup("Click the button to bring up a List of possible ACTIONS");
        tutorialCount = 3;
    }

    private void TutorialV3()
    {
        pointer.gameObject.SetActive(false);
        LeanTween.value(0, 1, 1f).setOnComplete(() => { if (pointer == null) return; pointer.gameObject.SetActive(true); pointer.position = ropeSetup.position + new Vector3(220, 0, 0); });
        TextPopup.instance.GeneratePopup("The first two are the PAUSE & PLAY buttons, lets focus on the third one, its a ROPE TRAP");
        tutorialCount = 4;
    }

    private void TutorialV4()
    {
        pointer.gameObject.SetActive(false);
        TextPopup.instance.GeneratePopup("Click on any Flat surface EXCEPT the Road and drag your finger to set the Contraption");
        tutorialCount = 5;
    }

    private void TutorialV5()
    {
        LeanTween.value(0, 1, 1f).setOnComplete(() => { if (pointer == null) return; pointer.gameObject.SetActive(true); pointer.position = turretSetup.position + new Vector3(220, 0, 0); });
        TextPopup.instance.GeneratePopup("Now with that out of the way, continue by clicking the Turret SETUP button");
        tutorialCount = 6;
    }

    private void TutorialV6()
    {
        pointer.gameObject.SetActive(false);
        TextPopup.instance.GeneratePopup("Click on any Flat surface EXCEPT the Road to setup the TURRET");
        tutorialCount = 7;
    }

    private void TutorialV7()
    {
        TextPopup.instance.GeneratePopup("Try clicking on the TURRET you just placed to UPGRADE it");
        tutorialCount = 8;
    }

    private void TutorialV8()
    {
        TextPopup.instance.GeneratePopup("Increase ATTACK SPEED, RANGE, DAMAGE or SELL UNIT, click and check description for more details");
        tutorialCount = 9;
    }

    private void TutorialV9()
    {
        TextPopup.instance.GeneratePopup("You can confirm the Upgrade by tapping TICK button but remember to check the COST always");
        tutorialCount = 10;
    }

    private void TutorialVEnd()
    {
        TextPopup.instance.GeneratePopup("Thats it, defeat the waves of Monsters, earn COINS & obtain CASH for SPECIAL UPGRADES later!");
        tutorialCount = 1;
        tutorialOn = false;
    }


    public void PlayTutorial(int value)
    {
        if (!tutorialOn)
            return;

        if (tutorialCount == 1 && value == 1)
            Tutorial1();
        else if (tutorialCount == 2 && value == 2)
            Tutorial2();
        else if (tutorialCount == 3 && value == 3)
            Tutorial3();
        else if (tutorialCount == 4 && value == 4)
            Tutorial4();
        else if (tutorialCount == 5 && value == 5)
            Tutorial5();
        else if (tutorialCount == 6 && value == 3)
            Tutorial6();
        else if (tutorialCount == 7 && value == 6)
            Tutorial7();
        else if (tutorialCount == 8 && value == 7)
            Tutorial8();
        else if (tutorialCount == 9)
            TutorialEnd();
    }

    private void Tutorial1()
    {
        tutorialPanel.gameObject.SetActive(true);
        pointer.gameObject.SetActive(true);
        pointer.localScale = new Vector3(-1, 1, 1);
        pointer.position = playButton.position - new Vector3(750, 0, 0);
        initialParent = playButton.parent;
        playButton.SetParent(tutorialPanel);
        TextPopup.instance.GeneratePopup("Try clicking the highlighted PLAY Button");
        tutorialCount = 2;
    }

    private void Tutorial2()
    {
        windowManager.SetSiblingIndex(13);
        playButton.SetParent(initialParent);
        pointer.localScale = new Vector3(1, 1, 1);
        pointer.position = equipmentButton.position + new Vector3(400, 0, 0);
        tutorialCount = 3;
    }

    private void Tutorial3()
    {
        pointer.localScale = new Vector3(-.5f, .5f, .5f);
        pointer.position = purchaseButton.position - new Vector3(250, 0, 0);
        TextPopup.instance.GeneratePopup("Looks like you will need to purhcase a turret first, lets head to the SHOP, you can click Purchase");
        tutorialCount = 4;
    }

    private void Tutorial4()
    {
        pointer.gameObject.SetActive(false);
        TextPopup.instance.GeneratePopup("This is the Shop, now try clicking the first turret banner and proceed to buy it");
        tutorialCount = 5;
    }

    private void Tutorial5()
    {
        TextPopup.instance.GeneratePopup("Lets head back to the EQUIPMENT tab and equip the newly purchased turret");
        tutorialCount = 6;
    }

    private void Tutorial6()
    {
        pointer.gameObject.SetActive(true);
        pointer.localScale = new Vector3(-.5f, .5f, .5f);
        pointer.position = addButton.position - new Vector3(200, 25, 0);
        TextPopup.instance.GeneratePopup("By clicking the (+) Button under the EQUIP, you can equip the currently selected turret");
        tutorialCount = 7;
    }

    private void Tutorial7()
    {
        pointer.gameObject.SetActive(false);
        TextPopup.instance.GeneratePopup("Head to the LOBBY so we can continue further");
        tutorialCount = 8;
    }

    private void Tutorial8()
    {
        windowManager.SetSiblingIndex(5);
        pointer.gameObject.SetActive(true);
        pointer.localScale = new Vector3(-1, 1, 1);
        pointer.position = playButton.position - new Vector3(750, 0, 0);
        playButton.SetParent(tutorialPanel);
        TextPopup.instance.GeneratePopup("Now hit PLAY Button again, we can finally start\n<size=75%> Tip: Click on this popup to close it");
        tutorialCount = 9;
    }

    private void TutorialEnd()
    {
        pointer.gameObject.SetActive(false);
        tutorialPanel.gameObject.SetActive(false);
        playButton.SetParent(initialParent);
        tutorialCount = 1;     
    }
}
