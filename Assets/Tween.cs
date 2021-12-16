
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Tween : MonoBehaviour
{
    public static float defaultDuration = 0.2f;
    public static float defaultAmount = 1.05f;

    public static void Bounce(Transform t, float amount)
    {
        Bounce( t , amount , defaultDuration );
    }

    public static void Bounce ( Transform t)
    {
        Bounce(t, defaultAmount);
    }

    public static void Bounce(Transform t, float amount, float duration)
    {
        //float prevScale = t.localScale.x;
        float prevScale = 1f;

        t.DOScale(amount, duration).SetEase(Ease.OutBounce);
        t.DOScale(prevScale, duration).SetEase(Ease.Linear).SetDelay(duration);
    }
}
