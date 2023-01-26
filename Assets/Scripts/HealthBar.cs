using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float setTimer = 5f;
    float timer = 5f;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            timer = setTimer;
            gameObject.SetActive(false);
        }
    }
}
