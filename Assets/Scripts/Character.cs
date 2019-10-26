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
    public ParticleSystem   chargeParticleSystem;
    public Transform        groundCheck;
    public Transform        barAnchor;
    public TrailRenderer    dashRenderer;
    public GhostCharacter   ghostCharacter;
    public Collider2D       hitCollider;
    [Header("Audio")]
    public AudioClip        jumpSound;
    public AudioClip        chargeSound;
    public AudioClip        overchargeSound;
    [Header("Runtime")]
    public OneButton    button;
    public float        health;
    public float        speed = 0.0f;
    public int          playerId;
    public float        score = 0.0f;
    public float        dashCharge = 0.0f;
    public UIBar        uiBar;
    public bool         isDead = false;

    SpriteRenderer  playerBaseSprite;
    Vector2         currentEyeDir;
    Rigidbody2D     rigidBody;
    int             jumpCount;
    bool            isDashing;
    bool            canDash = true;
    float           invulnerabilityTimer = 0.0f;
    Collider2D      mainCollider;
    Coroutine       dropDownCR;
    AudioSource     chargeSoundInstance;
    float           respawnTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        mainCollider = GetComponent<Collider2D>();
        playerBaseSprite = GetComponent<SpriteRenderer>();
        playerBaseSprite.color = playerColor;
        rigidBody = GetComponent<Rigidbody2D>();
        health = gameParams.maxHealth;
        dashRenderer.emitting = false;
        dashRenderer.startColor = dashRenderer.endColor = new Color(playerColor.r, playerColor.g, playerColor.b, 0.75f);

        SetPSColor(jumpParticleSystem, playerColor);
        SetPSColor(dashParticleSystem, playerColor);
        SetPSColor(explodeParticleSystem, playerColor);
        SetPSColor(chargeParticleSystem, playerColor);
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
                Prepare();
                break;
            case GameMng.GameState.Playing:
                if (!isDead)
                {
                    Play();
                }
                else
                {
                    if (respawnTimer > 0)
                    {
                        respawnTimer -= Time.deltaTime;
                        
                        if (respawnTimer < 0)
                        {
                            EnableColliders(true);
                            health = gameParams.maxHealth;
                            transform.position = GameMng.instance.playerSpawnPoint[0].position;
                            speed = GameMng.instance.GetCurrentSpeed();
                            isDead = false;
                            ShowChar();
                        }
                    }
                }
                break;
            case GameMng.GameState.End:
                {
                    rigidBody.velocity = new Vector2(1000.0f, rigidBody.velocity.y);
                }
                break;
            default:
                break;
        }
    }

    void Prepare()
    {
        Collider2D collider = Physics2D.OverlapCircle(groundCheck.transform.position, 5.0f, LayerMask.GetMask("Ground"));
        if (collider != null)
        {
            jumpCount = gameParams.maxJumpCount;
            dashCharge = 0.0f;
            canDash = true;
        }

        Vector2 currentVelocity = rigidBody.velocity;

        if (button.IsTapped())
        {
            if (jumpCount > 0)
            {
                SoundManager.PlaySound(SoundManager.SoundType.SoundFX, jumpSound, 0.25f, Random.Range(0.75f, 1.25f));
                jumpParticleSystem.Play();

                currentVelocity = new Vector2(0.0f, gameParams.jumpVelocity);
                ChangeSpeed(speed * gameParams.speedBoostJump);
                jumpCount--;
                rigidBody.gravityScale = 1.0f;
                dashCharge = 0.0f;
            }
        }

        rigidBody.velocity = currentVelocity;

        invulnerabilityTimer = 2.0f;
    }

    void Play()
    {
        if (invulnerabilityTimer > 0)
            invulnerabilityTimer -= Time.deltaTime;

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
            if (dropDownCR == null)
            {
                EnableChargeFX(false);
                jumpCount = gameParams.maxJumpCount;
                dashCharge = 0.0f;
                isGrounded = true;
                canDash = true;
            }
        }

        Vector2 currentVelocity = rigidBody.velocity;

        if ((button.IsTapped()) && (dashCharge <= 0.0f))
        {
            if (jumpCount > 0)
            {
                SoundManager.PlaySound(SoundManager.SoundType.SoundFX, jumpSound, 0.25f, Random.Range(0.75f, 1.25f));
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

                dashCharge = Mathf.Min(dashCharge + Time.deltaTime * gameParams.dashChargeSpeed, gameParams.overchargeLimit);

                EnableChargeFX(true);

                if (gameParams.overchargeExplode)
                {
                    if (dashCharge >= gameParams.overchargeLimit)
                    {
                        dashCharge = 0;
                        canDash = false;

                        speed = speed - gameParams.overchargePenaltySpeed;
                        DealDamage(gameParams.overchargeDamage);

                        RunOvercharge();

                        EnableChargeFX(false);
                    }
                }
            }
            else
            {
                EnableChargeFX(false);

                if (button.GetTimeSincePress() > 0.5f)
                {
                    if (transform.position.y > -280.0f)
                    {
                        if (dropDownCR == null)
                        {
                            // Drop down
                            dropDownCR = StartCoroutine(DropDownCR());
                            canDash = false;
                            currentVelocity.y = gameParams.jumpVelocity * 0.25f;
                        }
                    }
                    else if (gameParams.allowBreak)
                    {
                        speed = Mathf.Max(speed - Time.deltaTime * gameParams.breakAmmount, gameParams.breakMinimum);
                    }
                }
            }
        }
        else if (dashCharge > 0)
        {
            if (!isDashing)
            {
                EnableChargeFX(false);
                canDash = false;
                jumpCount = 0;
                isDashing = true;
                if (dashCharge > gameParams.maxDashCharge) dashCharge = gameParams.maxDashCharge;
                dashCharge += 25;
                ChangeSpeed(gameParams.speedBoostDash * speed);
                currentVelocity.y = gameParams.jumpVelocity * 0.5f;

                RunDashFX();
            }           
            dashCharge -= gameParams.dashDischargeSpeed * Time.deltaTime;           
            rigidBody.gravityScale = gameParams.gravityScaleDash;           
        }
        else
        {
            EnableChargeFX(false);

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

        if (!IsInvulnerable())
        {
            if (currentVelocity.y < 0)
            {
                collider = Physics2D.OverlapCircle(groundCheck.transform.position, 30.0f, LayerMask.GetMask("Character"));
                if (collider != null)
                {
                    if (HitAnotherPlayer(collider, gameParams.hitOnHeadScore, gameParams.hitOnHeadDamage, gameParams.hitOnHeadSpeedDown))
                    {
                        jumpParticleSystem.Play();
                        currentVelocity.y = gameParams.jumpVelocity;
                    }
                }
            }

            if (isDashing)
            {
                float boxWidth = 150.0f;
                var boxCenter = transform.position + new Vector3(boxWidth * 0.5f, 0.0f, 0.0f);

                var colliders = Physics2D.OverlapBoxAll(boxCenter, new Vector2(boxWidth, 128), 0, LayerMask.GetMask("Character"));
                bool hit = false;
                foreach (var col in colliders)
                {
                    hit |= HitAnotherPlayer(col, gameParams.hitOnDashScore, gameParams.hitOnDashDamage, gameParams.hitOnDashSpeedDown);
                }
                if (hit)
                {
                    burstParticleSystem.Play();
                }
            }
        }

        rigidBody.velocity = currentVelocity;
    }

    IEnumerator DropDownCR()
    {
        mainCollider.enabled = false;

        yield return new WaitForSeconds(0.5f);

        mainCollider.enabled = true;

        dropDownCR = null;
    }

    void ChangeSpeed(float deltaSpeed)
    {
        speed = Mathf.Clamp(speed + deltaSpeed, 0, gameParams.maxSpeed);
    }

    bool HitAnotherPlayer(Collider2D collider, float scoreInc, float damage, float speedPenalty)
    {
        Character otherCharacter = collider.GetComponent<Character>();
        if ((otherCharacter) && (otherCharacter != this))
        {
            if ((otherCharacter.isDead) ||
                (otherCharacter.IsInvulnerable()))
            {
                return false;
            }

            score += scoreInc;

            otherCharacter.DealDamage(damage);
            otherCharacter.RunOvercharge();
            otherCharacter.ChangeSpeed(-speedPenalty);

            return true;
        }

        return false;
    }

    public bool IsInvulnerable()
    {
        return invulnerabilityTimer > 0;
    }

    public void Heal(float gain)
    {
        health = Mathf.Clamp(health + gain, 0, gameParams.maxHealth);
    }

    public void DealDamage(float damage)
    {
        if (IsInvulnerable()) return;

        EnableChargeFX(false);

        health = Mathf.Clamp(health - damage, 0, gameParams.maxHealth);

        if (health <= 0)
        {
            isDead = true;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, gameParams.jumpVelocity * 1.0f);

            HideChar();
            EnableColliders(false);

            if (gameParams.allowRespawn)
            {
                respawnTimer = gameParams.respawnTimer;
            }
        }

        invulnerabilityTimer = 1.0f;
    }

    public void EnableColliders(bool b)
    {
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = b;
        }
    }

    public void EnableHit(bool b)
    {
        hitCollider.enabled = b;
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
        SoundManager.PlaySound(SoundManager.SoundType.SoundFX, overchargeSound, 1.0f, Random.Range(0.75f, 1.25f));
        CameraCtrl.Shake(0.15f, 50.0f);
    }

    void ShowChar()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            sprite.enabled = true;
        }

        sprites = ghostCharacter.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            sprite.enabled = false;
        }
    }

    void HideChar()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            sprite.enabled = false;
        }

        sprites = ghostCharacter.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            sprite.enabled = true;
        }

        ghostCharacter.RunSpin();
    }

    void EnableChargeFX(bool b)
    {
        var emission = chargeParticleSystem.emission;
        emission.enabled = b;

        if (b)
        {
            if (chargeSoundInstance == null)
            {
                chargeSoundInstance = SoundManager.PlaySound(SoundManager.SoundType.SoundFX, chargeSound, 0.7f, Random.Range(0.75f, 1.25f));
            }
        }
        else
        {
            if (chargeSoundInstance)
            {
                chargeSoundInstance.Stop();
                chargeSoundInstance = null;
            }
        }
    }
    #endregion
}
