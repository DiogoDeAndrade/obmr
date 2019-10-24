using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    public enum Mode { None, Health, Dash };

    public Image        outlineRef;
    public Image        barRef;
    public Character    character;

    CanvasGroup     canvasGroup;
    float           alpha;
    float           targetAlpha;
    RectTransform   rectTransform;
    float           prevHealth;
    float           prevDash;
    float           fadeTime;
    float           targetValue;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup.alpha = alpha = targetAlpha = 0.0f;
        outlineRef.color = character.playerColor;
        prevHealth = character.health;
        prevDash = character.dashCharge;
        targetValue = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Color color = barRef.color;
        float currentValue = barRef.rectTransform.localScale.x;

        bool    dashChange = false;
        bool    healthChange = false;
        float   changeSpeed = 1.0f;

        if (prevDash != character.dashCharge) dashChange = true;
        if (prevHealth != character.health) healthChange = true;

        if (dashChange)
        {
            targetAlpha = 1.0f;
            color = character.gameParams.dashBarColor;
            targetValue = character.dashCharge / character.gameParams.maxDashCharge;
            prevDash = character.dashCharge;

            fadeTime = 0.25f;
        }
        else if (healthChange)
        {
            currentValue = prevHealth;
            targetAlpha = 1.0f;
            color = character.gameParams.healthBarColor;
            targetValue = character.health / character.gameParams.maxHealth;
            prevHealth = character.health;
            changeSpeed = 0.1f;

            fadeTime = 2.0f;
        }
        else
        {
            if (Mathf.Abs(currentValue - targetValue) < 1.0f)
            {
                if (fadeTime > 0)
                {
                    fadeTime -= Time.deltaTime;
                }
                else
                {
                    targetAlpha = 0.0f;
                }
            }
        }

        currentValue = currentValue + (targetValue - currentValue) * changeSpeed;

        alpha = Mathf.Clamp01(alpha + (targetAlpha - alpha) * 0.1f);
        canvasGroup.alpha = alpha;

        if (targetAlpha > 0.0f)
        {
            barRef.color = color;

            barRef.rectTransform.localScale = new Vector3(currentValue, 1.0f, 1.0f);
        }

        transform.position = character.barAnchor.position;
    }
}
