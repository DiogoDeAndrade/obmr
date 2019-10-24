using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum Type { Life, Score };

    public Type             type;
    public float            scoreGain;
    public float            healthGain;
    public GameParams       gameParams;
    public AudioClip        sound;

    float           cameraLimitX;
    SpriteRenderer  sprite;
    bool            isDead = false;
    float           tColor;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        tColor = Random.Range(0.0f, 1.0f);
        cameraLimitX = GameMng.instance.GetPlayfieldLimitX();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float speed = GameMng.instance.GetCurrentSpeed() * GameMng.instance.layer0ScrollSpeed * Time.fixedDeltaTime;

        var currentPos = transform.position;
        currentPos.x -= speed;
        transform.position = currentPos;

        if (sprite.bounds.max.x < -cameraLimitX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        Character character = collision.GetComponent<Character>();
        if (character)
        {
            if (character.isDead) return;

            isDead = true;
            GetComponentInChildren<ParticleSystem>().Play();
            GetComponentInChildren<Animator>().SetTrigger("Explode");

            character.Heal(healthGain);
            character.score += scoreGain;

            SoundManager.PlaySound(SoundManager.SoundType.SoundFX, sound, 0.5f, Random.Range(0.8f, 1.2f));
        }
    }

    void DestroyBlock()
    {
        Destroy(gameObject);
    }
}
