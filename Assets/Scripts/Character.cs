using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Color          playerColor;
    public SpriteRenderer eyeLeft;
    public SpriteRenderer eyeRight;

    SpriteRenderer playerBaseSprite;

    // Start is called before the first frame update
    void Start()
    {
        playerBaseSprite = GetComponent<SpriteRenderer>();
        playerBaseSprite.color = playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        EyesLookAt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    void EyesLookAt(Vector3 position)
    {
        Vector2 delta;
        float   radius = 6;

        delta = (position - eyeLeft.transform.position).normalized;

        eyeLeft.transform.localPosition = radius * delta;

        delta = (position - eyeRight.transform.position).normalized;

        eyeRight.transform.localPosition = radius * delta;
    }
}
