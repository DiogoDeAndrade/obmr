using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMng : MonoBehaviour
{
    public Camera       baseCamera;
    public GameParams   gameParams;
    [Header("Title")]
    public GameObject[] titleElements;
    [Header("Prepare")]
    public TextMeshProUGUI countdownPrefab;
    public TextMeshProUGUI countdownGoPrefab;
    public RectTransform   countdownTarget;
    public AudioClip       countdownSound;
    public AudioClip       countdownDone;
    public AudioClip       playerJoinSound;
    [Header("Players")]
    public Character            characterPrefab;
    public Transform[]          playerSpawnPoint;
    public Color[]              playerColors;
    public TextMeshProUGUI[]    playerScores;
    public UIBar[]              uiBars;
    [Header("Platforms")]
    public Platform             platformPrefab;

    public static GameMng instance;

    public enum GameState { Title, Prepare, Playing };

    [HideInInspector]
    public GameState           gameState = GameState.Title;
    List<Character>            players;
    float                      timeToStart;
    BackgroundManager[]        backgroundLayers;
    float                      currentSpeed = 0.0f;
    float                      minSpeed = 0.0f;
    float                      raceTime = 0.0f;
    float                      platformSpawnTimer = 0.0f;
    Platform[]                 platforms;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        backgroundLayers = FindObjectsOfType<BackgroundManager>();
        platforms = new Platform[gameParams.platformHeight.Length];
        StartTitle();
    }

    public int SortByXOp(Character c1, Character c2)
    {
        float x1 = c1.transform.position.x;
        float x2 = c2.transform.position.x;

        if (System.Math.Abs(x1 - x2) < 0.00001f) return 0;
        else if (x1 < x2) return 1;

        return -1;
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
                {
                    ComputeMinSpeed();

                    float desiredSpeed = GetMinSpeed() * 1.1f;

                    currentSpeed += (desiredSpeed - currentSpeed) * 0.2f;

                    raceTime += Time.deltaTime;

                    SetGlobalSpeed(currentSpeed);

                    UpdateScore();

                    UpdatePlatforms();
                }
                break;
            default:
                break;
        }

        

        for (int i = 0; i < players.Count; i++)
        {
            playerScores[i].text = string.Format("{0:000000}", Mathf.FloorToInt(players[i].score));
        }

        OneButton.UpdateButtons();
    }

    void UpdateScore()
    {
        List<Character> positions = new List<Character>(players);

        positions.Sort(SortByXOp);

        float multiplier = 10.0f;

        foreach (var c in positions)
        {
            c.score += multiplier * Time.deltaTime;
            multiplier *= 0.75f;
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
                SoundManager.PlaySound(SoundManager.SoundType.SoundFX, countdownDone, 1, 1);

                var countdown = Instantiate(countdownGoPrefab, countdownTarget);
                countdown.text = "GO!";
                StartRunning();
            }
            else
            {
                SoundManager.PlaySound(SoundManager.SoundType.SoundFX, countdownSound, 0.75f, 0.7f - curTime * 0.05f);

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
                timeToStart = 7;
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

        uiBars[players.Count].gameObject.SetActive(true);
        uiBars[players.Count].character = newChar;

        newChar.uiBar = uiBars[players.Count];

        players.Add(newChar);

        CameraCtrl.Shake(0.15f, 20.0f);

        SoundManager.PlaySound(SoundManager.SoundType.SoundFX, playerJoinSound, 1, 1 + 0.05f * players.Count);
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
        foreach (var bar in uiBars)
        {
            bar.gameObject.SetActive(false);
        }
        SetGlobalSpeed(4000.0f);
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
        raceTime = 0.0f;
        foreach (var player in players)
        {
            player.speed = 800.0f;
        }
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

    void ComputeMinSpeed()
    {
        minSpeed = players[0].speed;

        foreach (var player in players)
        {
            minSpeed = Mathf.Min(minSpeed, player.speed);
        }
    }

    public float GetMinSpeed()
    {
        return minSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetPlayfieldLimitX()
    {
        return baseCamera.aspect* baseCamera.orthographicSize;
    }

    void UpdatePlatforms()
    {
        if (raceTime < gameParams.platformStartTime) return;

        platformSpawnTimer -= Time.deltaTime;

        if (platformSpawnTimer > 0) return;

        platformSpawnTimer = 1.0f;

        int r = Random.Range(0, platforms.Length);

        if (platforms[r] != null) return;

        float p = Random.Range(0.0f, 1.0f);

        if (p > gameParams.platformProbability) return;

        // Spawn platform

        Vector3 pos = new Vector3(GetPlayfieldLimitX(), gameParams.platformHeight[r], 0.0f);

        platforms[r] = Instantiate(platformPrefab, pos, Quaternion.identity);
        platforms[r].SetWidth(Random.Range(gameParams.platformWidthRange.x, gameParams.platformWidthRange.y));
    }
}
