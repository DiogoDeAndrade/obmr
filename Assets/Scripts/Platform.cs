using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public Vector2  size = new Vector2(1024, 32);

    SpriteRenderer sprite;
    float          cameraLimitX;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        cameraLimitX = GameMng.instance.GetPlayfieldLimitX();
    }

    void FixedUpdate()
    {
        float speed = GameMng.instance.GetCurrentSpeed() * GameMng.instance.layer0ScrollSpeed * Time.fixedDeltaTime;

        var currentPos = transform.position;
        currentPos.x -= speed;
        transform.position = currentPos;

        if (sprite.bounds.max.x < -cameraLimitX)
        {
            Destroy(gameObject);
        }
    }

    public void SetWidth(float width)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.size = new Vector2(width, sprite.size.y);

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.offset = new Vector2(width * 0.5f, 0.0f);
        collider.size = new Vector2(width, collider.size.y);

        size = new Vector2(width, sprite.size.y);
    }
}
