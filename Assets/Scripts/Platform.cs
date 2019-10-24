using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float    scrollScale = 1.1f;
    public Vector2  size = new Vector2(1024, 32);

    SpriteRenderer sprite;
    float          cameraLimitX;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        cameraLimitX = GameMng.instance.GetPlayfieldLimitX();
    }

    void Update()
    {
        float speed = GameMng.instance.GetCurrentSpeed() * scrollScale * Time.deltaTime;

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
        collider.size = new Vector2(width, collider.size.y);

        size = new Vector2(width, sprite.size.y);
    }
}
