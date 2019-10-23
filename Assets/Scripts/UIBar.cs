using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    public enum Mode { None, Health, Dash };

    public Mode         mode;
    public Image        outlineRef;
    public Image        barRef;
    public Character    character;

    CanvasGroup     canvasGroup;
    float           alpha;
    float           targetAlpha;
    RectTransform   rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup.alpha = alpha = targetAlpha = 0.0f;
        outlineRef.color = character.playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        Color color = Color.white;
        float value = 0.0f;

        switch (mode)
        {
            case Mode.None:
                targetAlpha = 0.0f;
                break;
            case Mode.Health:
                targetAlpha = 1.0f;
                color = character.gameParams.healthBarColor;
                value = character.health / character.gameParams.maxHealth;
                break;
            case Mode.Dash:
                targetAlpha = 1.0f;
                color = character.gameParams.dashBarColor;
                value = character.dashCharge / character.gameParams.maxDashCharge;
                break;
            default:
                break;
        }

        alpha = Mathf.Clamp01(alpha + (targetAlpha - alpha) * 0.1f);
        canvasGroup.alpha = alpha;

        if (targetAlpha > 0.0f)
        {
            barRef.color = color;

            barRef.rectTransform.localScale = new Vector3(value, 1.0f, 1.0f);
        }

        transform.position = character.barAnchor.position;
    }
}
