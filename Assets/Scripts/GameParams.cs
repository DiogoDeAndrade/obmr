using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "OBMR/GameParams")]
public class GameParams : ScriptableObject
{
    public float maxSpeed = 2000.0f;
    [Header("Passive acceleration")]
    public float    acceleration = 500.0f;
    public float    breakVelocity = 200.0f;
    public float    stdAccelerationMaxSpeed = 2000.0f;
    [Header("Jump")]
    public float    jumpVelocity = 1200.0f;
    public int      maxJumpCount = 2;
    public float    speedBoostJump = 0.1f;
    [Header("Dash")]
    public Color    dashBarColor = Color.cyan;
    public float    maxDashCharge = 100.0f;
    public float    dashChargeSpeed = 100.0f;
    public float    dashDischargeSpeed = 200.0f;
    public float    speedBoostDash = 100.0f;
    public float    velocityBoostDash = 300.0f;
    public float    gravityScaleChargeDash = 0.1f;
    public float    gravityScaleDash = 0.0f;
    public bool     overchargeExplode = true;
    public float    overchargePenaltySpeed = 1000.0f;
    public float    overchargeDamage = 0.0f;
    [Header("Health")]
    public Color    healthBarColor = Color.green;
    public float    maxHealth = 100.0f;
    [Header("Platforms")]
    public float    platformStartTime = 15.0f;
    public Vector2  platformWidthRange = new Vector2(1280 * 10, 1280 * 30);
    public float[]  platformHeight;
    public float    platformProbability = 1.0f;
}
