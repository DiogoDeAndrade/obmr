using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public GameParams       gameParams;
    public Gradient         colors;

    float           cameraLimitX;
    SpriteRenderer  sprite;
    bool            isDead = false;
    float           tColor;

    void Start()
    {
        var rb = GetComponent<Rigidbody2D>();

        rb.velocity = new Vector2(-1000.0f, 0.0f);
        sprite = GetComponent<SpriteRenderer>();
        tColor = Random.Range(0.0f, 1.0f);
        sprite.color = colors.Evaluate(tColor);
        cameraLimitX = GameMng.instance.GetPlayfieldLimitX();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (sprite.bounds.min.y < -cameraLimitX)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        tColor += Time.deltaTime * 2.5f;
        if (tColor > 1.0f) tColor -= 1.0f;
        sprite.color = colors.Evaluate(tColor);

        transform.Rotate(0.0f, 0.0f, Time.deltaTime * 360.0f);
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
            character.DealDamage(gameParams.mineDamage);
            CameraCtrl.Shake(0.15f, 40.0f);
        }
    }

    void DestroyBlock()
    {
        Destroy(gameObject);
    }
}
