using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextPopup : MonoBehaviour
{
    public static TextPopup instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject popupTextObject;
    RectTransform canvasObj;
    GameObject popup;


    public void GeneratePopup(string text)
    {
        if (popup != null)
            Destroy(popup);

        if (canvasObj == null)
            canvasObj = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>();
            
        popup = Instantiate(popupTextObject, canvasObj) as GameObject;
        popup.transform.GetComponentInChildren<TextMeshProUGUI>().text = text;
        popup.LeanScale(Vector3.one, .5f).setIgnoreTimeScale(true).setEaseInOutElastic().setOnComplete(() => { popup.LeanDelayedCall(6f, () => popup.GetComponent<CanvasGroup>().LeanAlpha(0, .3f).setIgnoreTimeScale(true).setOnComplete(() => Destroy(popup))).setIgnoreTimeScale(true).setOnUpdate(x => { x = 1; if (popup.name == "close") popup.GetComponent<CanvasGroup>().LeanAlpha(0, .3f).setIgnoreTimeScale(true).setOnComplete(() => Destroy(popup)); }); });
    }
}
