using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogHandler : MonoBehaviour
{
    private void Start()
    {
        Vector3 initialScale = transform.localScale;
        float initialY = transform.localPosition.y;

        transform.LeanScale(initialScale + new Vector3(1, 0, 1), 10f).setLoopPingPong();
        transform.LeanMoveLocalY(initialY + 1, 10f).setDelay(10f).setLoopPingPong();
    }
}
