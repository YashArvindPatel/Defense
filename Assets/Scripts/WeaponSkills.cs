using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class WeaponSkills : MonoBehaviour
{
    public RadialSlider s1, r1, d1;
    public RadialSlider s2, r2, d2;
    public RadialSlider s3, r3, d3;
    public RectTransform listRT;

    // Update is called once per frame
    void Update()
    {
        float yPos = listRT.anchoredPosition.y;

        if (yPos <= 150f)
        {
            if (s1.isActiveAndEnabled)
            {
                s1.SliderValue = Mathf.Clamp(s1.SliderValue + 2f, 0f, 80f);
                r1.SliderValue = Mathf.Clamp(r1.SliderValue + 2f, 0f, 72f);
                d1.SliderValue = Mathf.Clamp(d1.SliderValue + 2f, 0f, 62.5f);
            }
            
            if (s2.isActiveAndEnabled)
            {
                s2.SliderValue = Mathf.Clamp(s2.SliderValue - 2f, 0f, 100f);
                r2.SliderValue = Mathf.Clamp(r2.SliderValue - 2f, 0f, 40f);
                d2.SliderValue = Mathf.Clamp(d2.SliderValue - 2f, 0f, 37.5f);
            }         

            if (s3.isActiveAndEnabled)
            {
                s3.SliderValue = 0f;
                r3.SliderValue = 0f;
                d3.SliderValue = 0f;
            }               
        }
        else if (yPos > 150f && yPos <= 450f)
        {
            if (s1.isActiveAndEnabled)
            {
                s1.SliderValue = Mathf.Clamp(s1.SliderValue - 2f, 0f, 80f);
                r1.SliderValue = Mathf.Clamp(r1.SliderValue - 2f, 0f, 72f);
                d1.SliderValue = Mathf.Clamp(d1.SliderValue - 2f, 0f, 62.5f);
            }
           
            if (s2.isActiveAndEnabled)
            {
                s2.SliderValue = Mathf.Clamp(s2.SliderValue + 2f, 0f, 100f);
                r2.SliderValue = Mathf.Clamp(r2.SliderValue + 2f, 0f, 40f);
                d2.SliderValue = Mathf.Clamp(d2.SliderValue + 2f, 0f, 37.5f);
            }

            if (s3.isActiveAndEnabled)
            {
                s3.SliderValue = Mathf.Clamp(s3.SliderValue - 2f, 0f, 40f);
                r3.SliderValue = Mathf.Clamp(r3.SliderValue - 2f, 0f, 100f);
                d3.SliderValue = Mathf.Clamp(d3.SliderValue - 2f, 0f, 100f);
            }          
        }
        else if (yPos > 450f)
        {
            if (s1.isActiveAndEnabled)
            {
                s1.SliderValue = 0;
                r1.SliderValue = 0;
                d1.SliderValue = 0;
            }
            
            if (s2.isActiveAndEnabled)
            {
                s2.SliderValue = Mathf.Clamp(s2.SliderValue - 2f, 0f, 100f);
                r2.SliderValue = Mathf.Clamp(r2.SliderValue - 2f, 0f, 40f);
                d2.SliderValue = Mathf.Clamp(d2.SliderValue - 2f, 0f, 37.5f);
            }
            
            if (s3.isActiveAndEnabled)
            {
                s3.SliderValue = Mathf.Clamp(s3.SliderValue + 2f, 0f, 40f);
                r3.SliderValue = Mathf.Clamp(r3.SliderValue + 2f, 0f, 100f);
                d3.SliderValue = Mathf.Clamp(d3.SliderValue + 2f, 0f, 100f);
            }
        }

        if (s1.isActiveAndEnabled)
        {
            s1.UpdateUI();
            r1.UpdateUI();
            d1.UpdateUI();
        }
        
        if (s2.isActiveAndEnabled)
        {
            s2.UpdateUI();
            r2.UpdateUI();
            d2.UpdateUI();
        }

        if (s3.isActiveAndEnabled)
        {
            s3.UpdateUI();
            r3.UpdateUI();
            d3.UpdateUI();
        }
    }
}
