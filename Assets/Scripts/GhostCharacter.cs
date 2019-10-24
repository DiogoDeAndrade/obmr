using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostCharacter : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        Character character = GetComponentInParent<Character>();
        anim = GetComponent<Animator>();
    }

    public void SetColor(Color c)
    {
        Color ghostColor = c;
        ghostColor.a = 0.5f;

        var sprites = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sprite in sprites)
        {
            sprite.color = ghostColor;
            sprite.sortingLayerID = SortingLayer.NameToID("CharacterGhost");
        }
    }

    public void RunShock()
    {
        anim.SetTrigger("Shock");
    }
}
