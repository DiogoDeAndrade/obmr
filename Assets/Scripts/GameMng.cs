using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMng : MonoBehaviour
{
    public Camera       baseCamera;
    public Fader        globalFader;
    public GameParams   gameParams;
    public float        layer0ScrollSpeed = 1.05f;
    public GameObject   walls;
    [Header("Title")]
    public GameObject[] titleElements;
    [Header("Prepare")]
    public TextMeshProUGUI countdownPrefab;
    public TextMeshProUGUI countdownGoPrefab;
    public RectTransform   countdownTarget;
    public AudioClip       countdownSound;
    public AudioClip       countdownDone;
    public AudioClip       playerJoinSound;
    [Header("UI")]
    public TextMeshProUGUI      time;
    public Fader                fader;
    public EndRaceUI            endRaceUI;
    [Header("Players")]
    public Character            characterPrefab;
    public Transform[]          playerSpawnPoint;
    public Color[]              playerColors;
    public TextMeshProUGUI[]    playerScores;
    public UIBar[]              uiBars;
    [Header("Platforms")]
    public Platform             platformPrefab;
    [Header("Blocks")]
    public Block                blockPrefab;
    [Header("Mines")]
    public Mine                 minePrefab;
    [Header("Powerups")]
    public Powerup[]            powerupPrefabs;

    public static GameMng instance;

    public enum GameState { Title, Prepare, Playing, End };

    [HideInInspector]
    public GameState           gameState = GameState.Title;
    List<Character>            players;
    float                      timeToStart;
    BackgroundManager[]        backgroundLayers;
    float                      currentSpeed = 0.0f;
    float                      minSpeed = 0.0f;
    float                      raceTime = 0.0f;
    float                      platformSpawnTimer = 0.0f;
    float                      blockSpawnTimer = 0.0f;
    float                      blockProbability;
    Platform[]                 platforms;
    float                      mineSpawnTimer = 0.0f;
    float                      mineProbability = 0.0f;
    float                      powerupSpawnTime = 0.0f;
    bool                       canStart = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        backgroundLayers = FindObjectsOfType<BackgroundManager>();
        platforms = new Platform[gameParams.platformHeight.Length];
        blockProbability = gameParams.blockProbability;
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

    private void FixedUpdate()
    {
        switch (gameState)
        {
            case GameState.Playing:
                {
                    ComputeMinSpeed();

                    float desiredSpeed = GetMinSpeed() * 1.1f;

                    currentSpeed += (desiredSpeed - currentSpeed) * 0.2f;

                    SetGlobalSpeed(currentSpeed);
                }
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameState)
        {
            case GameState.Title:
                if (canStart) WaitPlayer();
                break;
            case GameState.Prepare:
                WaitPlayer();
                RunCountdown();
                break;
            case GameState.Playing:
                {
                    bool allDead = true;
                    foreach (var player in players)
                    {
                        if (!player.isDead) allDead = false;
                    }

                    if (allDead)
                    {
                        EndRace();
                    }
                    else
                    {
                        raceTime += Time.deltaTime;
                        if (raceTime >= gameParams.raceTime)
                        {
                            EndRace();
                        }
                        else
                        {
                            UpdateScore();

                            UpdatePlatforms();
                            UpdateBlocks();
                            UpdateMines();
                            UpdatePowerups();
                        }
                    }
                }
                break;
            default:
                break;
        }

        for (int i = 0; i < players.Count; i++)
        {
            playerScores[i].text = string.Format("{0:000000}", Mathf.FloorToInt(players[i].score));
        }

        time.text = string.Format("{0:0}", Mathf.Clamp(Mathf.FloorToInt(gameParams.raceTime - raceTime), 0, gameParams.raceTime));

        OneButton.UpdateButtons();

        if ((Input.GetKey(KeyCode.Escape)) && (Input.GetKey(KeyCode.LeftShift)))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void UpdateScore()
    {
        List<Character> positions = new List<Character>(players);

        positions.Sort(SortByXOp);

        float multiplier = 10.0f;

        foreach (var c in positions)
        {
            if (c.isDead) continue;

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

    public void StartTitle()
    {
        endRaceUI.gameObject.SetActive(false);

        canStart = false;

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

        globalFader.FadeTo(0.0f, 0.5f,
        () =>
        {
            canStart = true;
        });
    }

    void StartPrepare()
    {
        gameState = GameState.Prepare;
        timeToStart = 10;
        EnableMenu(false);
        SetGlobalSpeed(0.0f);
        raceTime = 0;
        EnableWalls(true);
    }

    void StartRunning()
    {
        gameState = GameState.Playing;
        raceTime = 0.0f;
        time.enabled = true;
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

        time.enabled = false;
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
        minSpeed = float.MaxValue;

        foreach (var player in players)
        {
            if (player.isDead) continue;

            minSpeed = Mathf.Min(minSpeed, player.speed);
        }

        if (minSpeed == float.MaxValue) minSpeed = 0.0f;
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

    void UpdateBlocks()
    {
        if (raceTime < gameParams.blockStartTime) return;

        blockSpawnTimer -= Time.deltaTime;
        blockProbability += gameParams.blockProbabilityOverTime * Time.deltaTime;

        if (blockSpawnTimer > 0) return;

        blockSpawnTimer = gameParams.blockInterval;

        int platformCount = 1;
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i]) platformCount++;
        }

        float actualProbability = platformCount * blockProbability;

        float p = Random.Range(0.0f, 1.0f);

        if (p > actualProbability) return;

        int r = Random.Range(0, platforms.Length);

        float x = GetPlayfieldLimitX();

        if (platforms[r] != null)
        {
            if (platforms[r].transform.position.x + platforms[r].size.x < x)
            {
                r = -1;
            }
        }
        else
        {
            r = -1;
        }

        float y = (r == -1) ? (-320) : (gameParams.platformHeight[r]);

        // Spawn block
        Vector3 pos = new Vector3(x, y + 32.0f, 0.0f);

        Instantiate(blockPrefab, pos, Quaternion.identity);
    }

    void UpdateMines()
    {
        if (raceTime < gameParams.mineStartTime) return;

        mineSpawnTimer -= Time.deltaTime;
        mineProbability += gameParams.mineProbabilityOverTime * Time.deltaTime;

        if (mineSpawnTimer > 0) return;

        float p = Random.Range(0.0f, 1.0f);

        mineSpawnTimer = gameParams.mineInterval;

        if (p > mineProbability) return;

        float x = GetPlayfieldLimitX() * 0.5f;
        x = Random.Range(0, x);

        Vector3 pos = new Vector3(x, baseCamera.transform.position.y + baseCamera.orthographicSize + 50.0f, 0.0f);

        Instantiate(minePrefab, pos, Quaternion.identity);
    }

    void UpdatePowerups()
    {
        if (raceTime < gameParams.powerupStartTime) return;

        powerupSpawnTime -= Time.deltaTime;

        if (powerupSpawnTime > 0) return;

        powerupSpawnTime = gameParams.powerupRate;

        float p = Random.Range(0.0f, 1.0f);

        if (p > gameParams.powerupProbability) return;

        int r = Random.Range(0, platforms.Length);

        float y = 0.0f;
        float width = 0.0f;
        float maxWidth = 2048;

        if (platforms[r] != null)
        {
            y = gameParams.platformHeight[r];
            width = (platforms[r].transform.position.x + platforms[r].size.x) - GetPlayfieldLimitX();

            if (width < 0) return;
            else
            {
                width = Mathf.Min(width, maxWidth);
            }
        }
        else
        {
            y = -320;
            width = maxWidth * 0.5f;
        }

        float x = 0.0f;

        var prefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];

        while (x < width)
        {
            Vector3 pos = new Vector3(x + GetPlayfieldLimitX(), y + 64.0f, 0.0f);

            Instantiate(prefab, pos, Quaternion.identity);

            x += 128.0f;
        }
    }

    void EndRace()
    {
        gameState = GameState.End;
        foreach (var player in players)
        {
            player.EnableHit(false);
        }
        EnableWalls(false);
        fader.FadeTo(0.75f, 1.0f,
        () =>
        {
            endRaceUI.Setup(players);
        });
    }

    void EnableWalls(bool b)
    {
        var wallColliders = walls.GetComponents<Collider2D>();
        foreach (var wall in wallColliders)
        {
            wall.enabled = b;
        }
    }
}
