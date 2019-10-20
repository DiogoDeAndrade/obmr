using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMng : MonoBehaviour
{
    [Header("Title")]
    public GameObject[] titleElements;
    [Header("Prepare")]
    public TextMeshProUGUI countdownPrefab;
    public TextMeshProUGUI countdownGoPrefab;
    public RectTransform   countdownTarget;
    [Header("Players")]
    public Character            characterPrefab;
    public Transform[]          playerSpawnPoint;
    public Color[]              playerColors;
    public TextMeshProUGUI[]    playerScores;

    public static GameMng instance;

    public enum GameState { Title, Prepare, Playing };

    [HideInInspector]
    public GameState           gameState = GameState.Title;
    List<Character>            players;
    float                      timeToStart;
    BackgroundManager[]        backgroundLayers;
    float                      currentSpeed = 0.0f;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        backgroundLayers = FindObjectsOfType<BackgroundManager>();
        StartTitle();
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameState)
        {
            case GameState.Title:
                WaitPlayer();
                break;
            case GameState.Prepare:
                WaitPlayer();
                RunCountdown();
                break;
            case GameState.Playing:
                break;
            default:
                break;
        }

        if (gameState == GameState.Playing)
        {
            float desiredSpeed = GetAverageSpeed();

            currentSpeed = currentSpeed + (desiredSpeed - currentSpeed) * 0.1f * Time.deltaTime;

            SetGlobalSpeed(currentSpeed);
        }

        for (int i = 0; i < players.Count; i++)
        {
            playerScores[i].text = string.Format("{0:000000}", Mathf.FloorToInt(players[i].score));
        }
    }

    void RunCountdown()
    {
        int prevTime = Mathf.FloorToInt(timeToStart);
        timeToStart -= Time.deltaTime;
        int curTime = Mathf.FloorToInt(timeToStart);

        if ((prevTime != curTime) && (curTime >= 0) && (curTime <= 5))
        {
            if (curTime == 0)
            {
                var countdown = Instantiate(countdownGoPrefab, countdownTarget);
                countdown.text = "GO!";
                StartRunning();
            }
            else
            {
                var countdown = Instantiate(countdownPrefab, countdownTarget);
                countdown.text = "" + curTime;
            }
        }
    }

    void WaitPlayer()
    {
        if (players.Count < 4)
        {
            var button = OneButton.GetButtonPress();
            if (button != null)
            {
                JoinPlayer(button);
                if (gameState == GameState.Title) StartPrepare();
                timeToStart = 10;
            }
        }
    }

    void JoinPlayer(OneButton button)
    {
        Character newChar = Instantiate(characterPrefab, playerSpawnPoint[players.Count].position, Quaternion.identity);

        newChar.playerColor = playerColors[players.Count];
        newChar.button = button;
        newChar.playerId = players.Count;
        newChar.RunSpawnFX();

        playerScores[players.Count].gameObject.SetActive(true);
        playerScores[players.Count].color = newChar.playerColor;

        players.Add(newChar);

        CameraCtrl.Shake(0.15f, 20.0f);
    }

    void StartTitle()
    {
        if (players != null)
        {
            foreach (var p in players)
            {
                Destroy(p.gameObject);
            }
        }
        players = new List<Character>();
        gameState = GameState.Title;
        OneButton.ClearButtons();
        EnableMenu(true);
        foreach (var ps in playerScores)
        {
            ps.gameObject.SetActive(false);
        }
    }

    void StartPrepare()
    {
        gameState = GameState.Prepare;
        timeToStart = 10;
        EnableMenu(false);
        SetGlobalSpeed(0.0f);
    }

    void StartRunning()
    {
        gameState = GameState.Playing;
    }

    void EnableMenu(bool b)
    {
        // Enable title elements
        foreach (var te in titleElements)
        {
            te.SetActive(b);
        }
    }

    void SetGlobalSpeed(float speed)
    {
        currentSpeed = speed;
        foreach (var background in backgroundLayers)
        {
            background.scrollSpeed = speed;
        }
    }

    float GetAverageSpeed()
    {
        float avgSpeed = 0.0f;
        foreach (var player in players)
        {
            avgSpeed += player.speed;
        }
        avgSpeed /= players.Count;

        return avgSpeed;
    }
}
