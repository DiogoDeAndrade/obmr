using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    public Character        character;
    public Image            body;
    public Transform        eyesBase;
    public Image            eyeLeft;
    public Image            eyeRight;
    public TextMeshProUGUI  scoreText;
    public AudioClip        tickSound;

    public int              score;
    public float            scoreUpdateTime;
    public float            targetX;
    public Vector2          lookTarget = Vector2.zero;

    Image playerBaseSprite;
    Vector2 currentEyeDir;
        
    void Start()
    {
        Init();
    }

    public void Init()
    {
        score = 0;
        if (character)
        {
            body.color = character.playerColor;
            scoreText.color = character.playerColor;
            targetX = transform.position.x;
        }
    }

    void Update()
    {
        if (character)
        {
            if ((lookTarget.x == 0) && (lookTarget.y == 0))
            {
                EyesLookRandom();
            }
            else
            {
                EyesLookTo(lookTarget);
            }
        }

        scoreUpdateTime -= Time.deltaTime;

        if (scoreUpdateTime <= 0.0f)
        {
            int inc = Random.Range(10, 100);
            int newScore = Mathf.FloorToInt(Mathf.Clamp(score + inc, 0, character.score));

            if (newScore != score)
            {
                score = newScore;

                float t = score / character.score;

                if (tickSound) SoundManager.PlaySound(SoundManager.SoundType.SoundFX, tickSound, 0.15f, 0.9f + t * 0.2f);
            }

            scoreText.text = string.Format("{0:000000}", score);

            scoreUpdateTime = 0.1f;
        }

        var currentPos = transform.position;
        currentPos.x = currentPos.x + (targetX - currentPos.x) * 0.05f;
        transform.position = currentPos;
    }

    #region Eyes
    public void EyesBeat()
    {
        EyesLookTo(new Vector2(1, 1).normalized);
    }

    void EyesLookRandom()
    {
        float x = Mathf.PerlinNoise(Time.time * 1.1f + character.playerId, 0.0f) * 2.0f - 1.0f;
        float y = Mathf.PerlinNoise(0.0f, Time.time * 0.9f + 0.77f * character.playerId) * 2.0f - 1.0f;

        var dir = new Vector2(x, y).normalized;

        EyesLookTo(dir);
    }

    void EyesLookAt(Vector3 position)
    {
        Vector2 delta;
        float radius = 6;

        delta = (position - eyeLeft.transform.position).normalized;

        eyeLeft.transform.localPosition = radius * delta;

        delta = (position - eyeRight.transform.position).normalized;

        eyeRight.transform.localPosition = radius * delta;
    }

    void EyesLookTo(Vector2 direction)
    {
        direction = direction.normalized;

        float radius = 6;

        eyeLeft.transform.localPosition = radius * direction;
        eyeRight.transform.localPosition = radius * direction;
        currentEyeDir = direction;
    }
    #endregion
}
