using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    float           speed = 0.5f;
    float           targetAlpha;
    Color           currentColor;
    Image           image;
    CanvasGroup     canvasGroup;
    System.Action   callback;

    void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (image) currentColor = image.color;
        else if (canvasGroup) currentColor = new Color(1, 1, 1, canvasGroup.alpha);

        targetAlpha = currentColor.a;
    }

    void Update()
    {
        if (targetAlpha < currentColor.a)
        {
            if (image) image.enabled = true;

            currentColor.a = currentColor.a - speed * Time.deltaTime;
            if (targetAlpha > currentColor.a)
            {
                currentColor.a = targetAlpha;
                if (callback != null)
                {
                    callback();
                    callback = null;
                }
            }
            if (image) image.color = currentColor;
            else if (canvasGroup) canvasGroup.alpha = currentColor.a;
        }
        else if (targetAlpha > currentColor.a)
        {
            if (image) image.enabled = true;

            currentColor.a = currentColor.a + speed * Time.deltaTime;
            if (targetAlpha < currentColor.a)
            {
                currentColor.a = targetAlpha;
                if (callback != null)
                {
                    callback();
                    callback = null;
                }
            }
            if (image) image.color = currentColor;
            else if (canvasGroup) canvasGroup.alpha = currentColor.a;
        }
        else
        {
            if ((targetAlpha == 0.0f) && (image)) image.enabled = false;
        }
    }

    public void FadeTo(float alpha, float time, System.Action callbackOnEnd = null)
    {
        if (callback != null)
        {
            callback();
            callback = null;
        }
        targetAlpha = alpha;
        speed = Mathf.Abs(targetAlpha - currentColor.a) / time;
        if (speed == 0.0f)
        {
            callbackOnEnd();
        }
        else
        {
            callback = callbackOnEnd;
        }
    }
}
