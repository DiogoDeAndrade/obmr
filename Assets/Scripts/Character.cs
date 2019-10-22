using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Game Params")]
    public GameParams   gameParams;
    [Header("Player prefs")]
    public Color          playerColor;
    [Header("References")]
    public SpriteRenderer   eyeLeft;
    public SpriteRenderer   eyeRight;
    public ParticleSystem   burstParticleSystem;
    public ParticleSystem   jumpParticleSystem;
    public Transform        groundCheck;
    [Header("Runtime")]
    public OneButton      button;
    public float          speed = 0.0f;
    public int            playerId;
    public float          score = 0.0f;

    SpriteRenderer  playerBaseSprite;
    Vector2         currentEyeDir;
    Rigidbody2D     rigidBody;
    int             jumpCount;
    int             currentJumpCount;

    // Start is called before the first frame update
    void Start()
    {
        playerBaseSprite = GetComponent<SpriteRenderer>();
        playerBaseSprite.color = playerColor;
        rigidBody = GetComponent<Rigidbody2D>();

        SetPSColor(jumpParticleSystem, playerColor);
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
                Play();
                break;
            default:
                break;
        }
    }

    void Play()
    {
        if (speed < gameParams.maxSpeed)
        {
            speed = Mathf.Clamp(speed + gameParams.acceleration * Time.deltaTime, 0.0f, gameParams.maxSpeed);
        }
        else
        {
            speed = speed - gameParams.breakVelocity * Time.deltaTime;
        }

        bool       isGrounded = false;
        Collider2D collider = Physics2D.OverlapCircle(groundCheck.transform.position, 5.0f, LayerMask.GetMask("Ground"));
        if (collider != null)
        {
            jumpCount = gameParams.maxJumpCount;
            isGrounded = true;
        }

        Vector2 currentVelocity = rigidBody.velocity;

        if (button.IsTapped())
        {
            if (jumpCount > 0)
            {
                jumpParticleSystem.Play();

                currentVelocity = new Vector2(0.0f, gameParams.jumpVelocity);
                speed = speed + speed * gameParams.speedBoostJump;
                jumpCount--;
            }
        }

        EyesLookTo(new Vector2(speed * 0.25f, currentVelocity.y));

        currentVelocity.x = speed - GameMng.instance.GetCurrentSpeed();

        rigidBody.velocity = currentVelocity;
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
        direction = direction.normalized;

        float radius = 6;

        eyeLeft.transform.localPosition = radius * direction;
        eyeRight.transform.localPosition = radius * direction;
        currentEyeDir = direction;
    }

    public void SetPSColor(ParticleSystem ps, Color c)
    {
        var mainDefs = ps.main;
        var startColor = mainDefs.startColor;
        startColor.color = c;
        mainDefs.startColor = startColor;
    }

    public void RunSpawnFX()
    {
        SetPSColor(burstParticleSystem, playerColor);

        burstParticleSystem.Play();
    }
}
