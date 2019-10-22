using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "OBMR/GameParams")]
public class GameParams : ScriptableObject
{
    public float    acceleration = 500.0f;
    public float    breakVelocity = 200.0f;
    public float    maxSpeed = 2000.0f;
    public float    jumpVelocity = 1200.0f;
    public int      maxJumpCount = 2;
    public float    speedBoostJump = 0.1f;
}
