using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public GameParams       gameParams;
    public Gradient         colors;

    float           cameraLimitX;
    SpriteRenderer  sprite;
    bool            isDead = false;
    float           tColor;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        tColor = Random.Range(0.0f, 1.0f);
        sprite.color = colors.Evaluate(tColor);
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

    private void Update()
    {
        tColor += Time.deltaTime * 2.5f;
        if (tColor > 1.0f) tColor -= 1.0f;
        sprite.color = colors.Evaluate(tColor);

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
            character.DealDamage(gameParams.blockDamage);
            CameraCtrl.Shake(0.1f, 20.0f);
        }
    }

    void DestroyBlock()
    {
        Destroy(gameObject);
    }
}
