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
    public Transform        eyesBase;
    public SpriteRenderer   eyeLeft;
    public SpriteRenderer   eyeRight;
    public ParticleSystem   burstParticleSystem;
    public ParticleSystem   jumpParticleSystem;
    public ParticleSystem   dashParticleSystem;
    public ParticleSystem   explodeParticleSystem;
    public Transform        groundCheck;
    public Transform        barAnchor;
    public TrailRenderer    dashRenderer;
    public GhostCharacter   ghostCharacter;
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
    bool            canDash = true;

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
        SetPSColor(explodeParticleSystem, playerColor);
        ghostCharacter.SetColor(playerColor);
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
            canDash = true;
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
        else if ((button.IsPressed()) && (button.GetTimeSincePress() > 0.1f) && (canDash))
        {
            if (!isGrounded)
            {
                if (currentVelocity.y > 0) currentVelocity.y = 0.0f;

                rigidBody.gravityScale = gameParams.gravityScaleChargeDash;

                dashCharge = Mathf.Min(dashCharge + Time.deltaTime * gameParams.dashChargeSpeed, gameParams.maxDashCharge);

                if (gameParams.overchargeExplode)
                {
                    if (dashCharge >= gameParams.maxDashCharge)
                    {
                        dashCharge = 0;
                        canDash = false;

                        speed = speed - gameParams.overchargePenaltySpeed;
                        health = health - gameParams.overchargeDamage;

                        RunOvercharge();
                    }
                }
            }
        }
        else if (dashCharge > 0)
        {
            if (!isDashing)
            {
                canDash = false;
                jumpCount = 0;
                isDashing = true;
                ChangeSpeed(gameParams.speedBoostDash * speed);
                currentVelocity.y = gameParams.jumpVelocity * 0.5f;

                RunDashFX();
            }           
            dashCharge -= gameParams.dashDischargeSpeed * Time.deltaTime;           
            rigidBody.gravityScale = gameParams.gravityScaleDash;           
        }
        else
        {
            isDashing = false;
            dashCharge = 0.0f;
            rigidBody.gravityScale = 1.0f;

            StopDashFX();
        }

        if (isDashing)
            EyesLookTo(new Vector2(1.0f, 0.0f));
        else
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

    void ChangeSpeed(float deltaSpeed)
    {
        speed = Mathf.Clamp(speed + deltaSpeed, 0, gameParams.maxSpeed);
    }

    #region Eyes
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
    #endregion

    #region FX
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

    public void RunDashFX()
    {
        dashRenderer.emitting = true;
        dashParticleSystem.Play();
        var emission = dashParticleSystem.emission;
        emission.enabled = true;
        CameraCtrl.Shake(0.15f, 20.0f);
        eyesBase.localScale = new Vector3(1.5f, 1.5f, 1.0f);
    }

    public void StopDashFX()
    {
        var emission = dashParticleSystem.emission;
        emission.enabled = false;
        dashRenderer.emitting = false;
        eyesBase.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public void RunOvercharge()
    {
        ghostCharacter.RunShock();
        explodeParticleSystem.Play();
        burstParticleSystem.Play();
        CameraCtrl.Shake(0.15f, 50.0f);
    }
    #endregion
}
