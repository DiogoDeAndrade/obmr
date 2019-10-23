using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Game Params")]
    public GameParams gameParams;
    [Header("Player prefs")]
    public Color playerColor;
    [Header("References")]
    public SpriteRenderer   eyeLeft;
    public SpriteRenderer   eyeRight;
    public ParticleSystem   burstParticleSystem;
    public ParticleSystem   jumpParticleSystem;
    public ParticleSystem   dashParticleSystem;
    public Transform        groundCheck;
    public Transform        barAnchor;
    public TrailRenderer    dashRenderer;
    [Header("Runtime")]
    public OneButton    button;
    public float        health;
    public float        speed = 0.0f;
    public int          playerId;
    public float        score = 0.0f;
    public float        dashCharge = 0.0f;
    public UIBar        uiBar;

    SpriteRenderer  playerBaseSprite;
    Vector2         currentEyeDir;
    Rigidbody2D     rigidBody;
    int             jumpCount;
    bool            isDashing;

    // Start is called before the first frame update
    void Start()
    {
        playerBaseSprite = GetComponent<SpriteRenderer>();
        playerBaseSprite.color = playerColor;
        rigidBody = GetComponent<Rigidbody2D>();
        health = gameParams.maxHealth;
        dashRenderer.emitting = false;
        dashRenderer.startColor = dashRenderer.endColor = new Color(playerColor.r, playerColor.g, playerColor.b, 0.75f);

        SetPSColor(jumpParticleSystem, playerColor);
        SetPSColor(dashParticleSystem, playerColor);
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
        if (speed < gameParams.stdAccelerationMaxSpeed)
        {
            ChangeSpeed(gameParams.acceleration * Time.deltaTime);
        }
        else if (dashCharge <= 0.0f)
        {
            ChangeSpeed(-gameParams.breakVelocity * Time.deltaTime);
        }

        bool isGrounded = false;
        Collider2D collider = Physics2D.OverlapCircle(groundCheck.transform.position, 5.0f, LayerMask.GetMask("Ground"));
        if (collider != null)
        {
            jumpCount = gameParams.maxJumpCount;
            dashCharge = 0.0f;
            isGrounded = true;
        }

        Vector2 currentVelocity = rigidBody.velocity;

        if (button.IsTapped())
        {
            if (jumpCount > 0)
            {
                jumpParticleSystem.Play();

                currentVelocity = new Vector2(0.0f, gameParams.jumpVelocity);
                ChangeSpeed(speed * gameParams.speedBoostJump);
                jumpCount--;
                rigidBody.gravityScale = 1.0f;
                dashCharge = 0.0f;
            }
        }
        else if ((button.IsPressed()) && (button.GetTimeSincePress() > 0.1f))
        {
            if (!isGrounded)
            {
                if (currentVelocity.y > 0) currentVelocity.y = 0.0f;

                rigidBody.gravityScale = gameParams.gravityScaleChargeDash;

                dashCharge = Mathf.Min(dashCharge + Time.deltaTime * gameParams.dashChargeSpeed, gameParams.maxDashCharge);

                uiBar.mode = UIBar.Mode.Dash;
            }
        }
        else if (dashCharge > 0)
        {
            if (!isDashing)
            {
                jumpCount = 0;
                isDashing = true;
                ChangeSpeed(gameParams.speedBoostDash * speed);
                currentVelocity.y = gameParams.jumpVelocity * 0.5f;
                dashRenderer.emitting = true;
                dashParticleSystem.Play();
                var emission = dashParticleSystem.emission;
                emission.enabled = true;
                CameraCtrl.Shake(0.15f, 20.0f);
            }           
            dashCharge -= gameParams.dashDischargeSpeed * Time.deltaTime;           
            rigidBody.gravityScale = gameParams.gravityScaleDash;           
        }
        else
        {
            var emission = dashParticleSystem.emission;
            emission.enabled = false;
            dashRenderer.emitting = false;
            isDashing = false;
            dashCharge = 0.0f;
            if (uiBar.mode == UIBar.Mode.Dash) uiBar.mode = UIBar.Mode.None;
            rigidBody.gravityScale = 1.0f;
            
        }

        EyesLookTo(new Vector2(speed * 0.25f, currentVelocity.y));

        currentVelocity.x = speed - GameMng.instance.GetCurrentSpeed();
        if (isDashing)
        {
            currentVelocity.x = gameParams.velocityBoostDash;
            if (currentVelocity.x < 0.0f)
                dashRenderer.emitting = false;
        }

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

    void ChangeSpeed(float deltaSpeed)
    {
        speed = Mathf.Min(speed + deltaSpeed, gameParams.maxSpeed);
    }
}
