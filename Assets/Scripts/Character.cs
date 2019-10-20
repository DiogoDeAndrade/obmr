using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Player prefs")]
    public Color          playerColor;
    [Header("References")]
    public SpriteRenderer   eyeLeft;
    public SpriteRenderer   eyeRight;
    public ParticleSystem   burstParticleSystem;
    [Header("Runtime")]
    public OneButton      button;
    public float          speed = 0.0f;
    public int            playerId;
    public float          score = 0.0f;

    SpriteRenderer  playerBaseSprite;
    Vector2         currentEyeDir;

    // Start is called before the first frame update
    void Start()
    {
        playerBaseSprite = GetComponent<SpriteRenderer>();
        playerBaseSprite.color = playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        switch (GameMng.instance.gameState)
        {
            case GameMng.GameState.Title:
                {
                    Vector2 desiredDir = new Vector2(1, -1).normalized;

                    EyesLookTo(Vector2.Lerp(currentEyeDir, desiredDir, 0.1f));
                }
                break;
            case GameMng.GameState.Prepare:
                EyesLookRandom();
                break;
            case GameMng.GameState.Playing:
                break;
            default:
                break;
        }
    }

    public void EyesBeat()
    {
        EyesLookTo(new Vector2(1, 1).normalized);
    }

    void EyesLookRandom()
    {
        float x = Mathf.PerlinNoise(Time.time * 1.1f + playerId, 0.0f) * 2.0f - 1.0f;
        float y = Mathf.PerlinNoise(0.0f, Time.time * 0.9f + 0.77f * playerId) * 2.0f - 1.0f;

        var dir = new Vector2(x,y).normalized;

        EyesLookTo(dir);
    }

    void EyesLookAt(Vector3 position)
    {
        Vector2 delta;
        float   radius = 6;
         
        delta = (position - eyeLeft.transform.position).normalized;

        eyeLeft.transform.localPosition = radius * delta;

        delta = (position - eyeRight.transform.position).normalized;

        eyeRight.transform.localPosition = radius * delta;
    }

    void EyesLookTo(Vector2 direction)
    {
        float radius = 6;

        eyeLeft.transform.localPosition = radius * direction;
        eyeRight.transform.localPosition = radius * direction;
        currentEyeDir = direction;
    }

    public void RunSpawnFX()
    {
        var mainDefs = burstParticleSystem.main;
        var startColor = mainDefs.startColor;
        startColor.color = playerColor;
        mainDefs.startColor = startColor;

        burstParticleSystem.Play();
    }
}
