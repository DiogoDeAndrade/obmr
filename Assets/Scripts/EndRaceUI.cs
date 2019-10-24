using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndRaceUI : MonoBehaviour
{
    public List<CharacterUI>    characters;
    public Transform[]          placements;
    public GameObject           placementPrefab;
    public RectTransform        placementContainer;
    public AudioClip            badgeHitSound;

    bool                awardedBadges = false;
    List<GameObject>    tempObjects;

    void Start()
    {
        tempObjects = new List<GameObject>();
    }

    void Update()
    {
        if (awardedBadges)
        {
            if (OneButton.GetButtonPress() != null)
            {
                OneButton.ClearButtons();

                GameMng.instance.globalFader.FadeTo(1.0f, 0.5f, () =>
                {
                    Fader fader = GetComponentInParent<Fader>();
                    fader.FadeTo(0.0f, 0.5f, () =>
                    {
                        GameMng.instance.StartTitle();

                        foreach (var obj in tempObjects)
                        {
                            Destroy(obj);
                        }

                        tempObjects.Clear();

                        for (int i = 0; i < characters.Count; i++)
                        {
                            characters[i].gameObject.SetActive(false);
                        }
                    });
                });
            }

            return;
        }

        characters.Sort(SortByScore);

        bool allDone = true;

        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].targetX = placements[i].position.x;
            if (characters[i].character)
            {
                if (characters[i].score != Mathf.FloorToInt(characters[i].character.score))
                {
                    allDone = false;
                }
                else if (Mathf.Abs(characters[i].transform.position.x - placements[i].position.x) > 2.0f)
                {
                    allDone = false;
                }
            }
        }

        if (allDone)
        {
            awardedBadges = true;
            StartCoroutine(PlacementCR());
            OneButton.ClearButtons();
        }
    }

    IEnumerator PlacementCR()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (!characters[i].gameObject.activeSelf) break;

            var p = Instantiate(placementPrefab, placements[i].position, Quaternion.identity);
            p.transform.SetParent(placementContainer, true);
            p.transform.localScale = Vector3.one;
            p.GetComponentInChildren<TextMeshProUGUI>().text = "" + (i + 1);

            if (badgeHitSound)
            {
                SoundManager.PlaySound(SoundManager.SoundType.SoundFX, badgeHitSound, 1.0f, 1.0f - 0.05f * i);
            }

            tempObjects.Add(p);

            switch (i)
            {
                case 0:
                    characters[i].lookTarget = new Vector2(0.707f, 0.707f);
                    break;
                case 1:
                case 2:
                    characters[i].lookTarget = new Vector2(1, -0.2f);
                    break;
                case 3:
                    characters[i].lookTarget = new Vector2(0.2f, -0.9f);
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    int SortByScore(CharacterUI c1, CharacterUI c2)
    {
        if (c1.score == c2.score) return 0;
        else if (c1.score > c2.score) return -1;

        return 1;
    }

    public void Setup(List<Character> players)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < characters.Count; i++)
        {
            if (i >= players.Count)
            {
                characters[i].gameObject.SetActive(false);
                characters[i].character = null;
            }
            else
            {
                characters[i].gameObject.SetActive(true);
                characters[i].character = players[i];
                characters[i].lookTarget = Vector2.zero;

            }

            characters[i].Init();
        }
        awardedBadges = false;
    }
}
