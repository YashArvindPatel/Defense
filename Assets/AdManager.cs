using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdManager : MonoBehaviour, IUnityAdsListener
{
    private string gameId = "4157489";
    private string placement = "rewardedVideo";
    private bool testMode = false;

    public CashAndExp cashAndExp;

    void Start()
    {
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
    } 

    public void ShowRewardedVideo()
    {
        if (Advertisement.IsReady(placement))
        {
            TextPopup.instance.GeneratePopup("Showing Rewarded Video, watch fully to earn Cash!");

            Advertisement.Show(placement);
        }
        else
        {
            TextPopup.instance.GeneratePopup("Loading Rewarded Video, please try again!");
        }

        SFX.instance.PlaySingleSoundClip((int)SoundIndexes.TWEEN_OPEN_TURRET);
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            cashAndExp.ChangeInCashAmount(1500);

            TextPopup.instance.GeneratePopup("1500 Cash collected! Watch ads for more!");
        }
        else if (showResult == ShowResult.Skipped)
        {
            TextPopup.instance.GeneratePopup("Watch ad completely to collect Reward!");
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
    }

    public void OnUnityAdsDidError(string message)
    {
    }

    public void OnUnityAdsDidStart(string placementId)
    {
    }
}
