using UnityEngine;
using System.Collections;
using SgLib;
using System.Collections.Generic;
using Assets._Pinball.Scripts.Services;
using Assets._Pinball.Scripts.Models;
using UnityEngine.SceneManagement;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public enum LastSignificantGameState 
{
    GameOver,
    TargetReceived,
    ExtraBallReceived,
    ExtraLifeReceived,
    BallLost,
    TargetMissed,
    ExtraBallMissed,
    ExtraLifeMissed,
}

public class GameManager : Singleton<GameManager>
{
    public static int GameCount
    { 
        get { return _gameCount; } 
        private set { _gameCount = value; } 
    }

    private static int _gameCount = 0;

    public static event System.Action<GameState, GameState> GameStateChanged = delegate {};
    public static event System.Action LifeLost = delegate {};
    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private FixedSizeConcurrentQueue<LastSignificantGameState> LastSignificantGameStates = new FixedSizeConcurrentQueue<LastSignificantGameState>(10);

    private GameState _gameState = GameState.Prepare;
    private Coroutine _tempSkillCoroutine;
    [Header("Gameplay References")]
    public GameObject ballPrefab;
    public GameObject ballPoint;
    public GameObject obstacleManager;
    public GameObject targetPointManager;
    public GameObject temporarySkillPointManager;
    public GameObject leftFlipper;
    public GameObject rightFlipper;
    //public GameObject targetPrefab;
    public GameObject extraBallPrefab;
    public GameObject extraLifePrefab;    
    public GameObject ushape;
    public GameObject background;
    public GameObject fence;
    [HideInInspector]
    public GameObject currentTargetPointWrapper;
    //[HideInInspector]
    //public GameObject currentTarget;
    [HideInInspector]
    public GameObject currentTemporarySkillPoint;
    [HideInInspector]
    public GameObject currentTemporarySkill;
    public ParticleSystem die;
    public ParticleSystem hitGold;
    public ParticleSystem hitTemporarySkill;
    [HideInInspector]
    public bool gameOver;

    [Header("Gameplay Config")]
    public Color[] backgroundColor;
    public int scoreToIncreaseDifficulty;
    public float targetAliveTime = 20;
    public float targetAliveTimeDecreaseValue = 1;
    public float temporarySkillAliveTime = 10;
    public float temporarySkillAliveTimeDecreaseValue = 1;
    public int minTargetAliveTime = 3;
    public int scoreToAddTemporarySkill = 1;

    private List<GameObject> listBall = new List<GameObject>();
    private SpriteRenderer ushapeSpriteRenderer;
    private SpriteRenderer backgroundSpriteRenderer;
    private SpriteRenderer fenceSpriteRenderer;
    private ScoreSO score;
    private HealthSO healthSO;
    private int obstacleCounter = 0;
    private int playedAmount;

    private void LoseLife()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
        DestroyTarget();
        PlayTemporarySkillParticle();
        StartGame();
        CreateBall();
    }

    private void OutOfLife()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
        DestroyTarget();
        PlayTemporarySkillParticle();
        gameOver = true;
        GameOver();
        SceneManager.LoadScene("Main");
    }

    private void LoseBall(GameObject ball)
    {
        //remove the ball from the list
        listBall.Remove(ball);

        //No ball left -> game over
        if (listBall.Count == 0)
        {
            LoseLife();
            LifeLost();
            LastSignificantGameStates.Enqueue(LastSignificantGameState.BallLost);
        }
    }

    public void PlayTemporarySkillParticle()
    {
        if (currentTemporarySkill != null && currentTemporarySkillPoint != null)
        {
            currentTemporarySkillPoint.SetActive(false);

            ParticleSystem temporarySkillParticle = Instantiate(hitTemporarySkill, currentTemporarySkill.transform.position, Quaternion.identity) as ParticleSystem;
            var temporarySkillMain = temporarySkillParticle.main;
            temporarySkillMain.startColor = currentTemporarySkill.gameObject.GetComponent<SpriteRenderer>().color;
            temporarySkillParticle.Play();
            Destroy(temporarySkillParticle.gameObject, 1f);
            Destroy(currentTemporarySkill.gameObject);
        }
    }

    /// <summary>
    /// Disable extra ball skill and its process
    /// </summary>
    public void DisableTemporarySkill()
    {
        //Stop all processing, disable current extra ball
        if (_tempSkillCoroutine != null)
            StopCoroutine(_tempSkillCoroutine);

        if (currentTemporarySkill != null)
        {
            currentTemporarySkill.SetActive(false);
            //Destroy(currentTemporarySkill);
        }
            

        if (currentTemporarySkillPoint != null)
        {
            currentTemporarySkillPoint.SetActive(false);
            //Destroy(currentTemporarySkillPoint);
        }
            
    }

    private void DestroyTarget()
    {
        StopCoroutine("Processing");
        currentTargetPointWrapper.SetActive(false);
        ParticleSystem particle = Instantiate(hitGold, currentTargetPointWrapper.transform.position, Quaternion.identity) as ParticleSystem;
        var main = particle.main;
        //main.startColor = currentTarget.gameObject.GetComponent<SpriteRenderer>().color;
        particle.Play();
        Destroy(particle.gameObject, 1f);
        Destroy(currentTargetPointWrapper);
    }

    // Use this for initialization
    void Start()
    {
        scoreToIncreaseDifficulty = AppInfo.TargetScore * 20;
        healthSO = ScriptableObject.FindObjectOfType<HealthSO>() ?? ScriptableObject.CreateInstance<HealthSO>();
        healthSO.Init();
        healthSO.NoLifeLeft += OutOfLife;
        BallController.BallLost += LoseBall;

        GameState = GameState.Prepare;
        
        score = ScriptableObject.FindObjectOfType<ScoreSO>() ?? ScriptableObject.CreateInstance<ScoreSO>();
        score.Reset();

        currentTargetPointWrapper = null;
        currentTemporarySkillPoint = null;
        ushapeSpriteRenderer = ushape.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
        fenceSpriteRenderer = fence.GetComponent<SpriteRenderer>();

        //Change color of backgorund, ushape, fence, flippers
        Color color = backgroundColor[Random.Range(0, backgroundColor.Length)];
        ushapeSpriteRenderer.color = color;
        backgroundSpriteRenderer.color = color; 
        fenceSpriteRenderer.color = color;
        StartGame();
        CreateBall();
    }

    /// <summary>
    /// Fire game event, create gold
    /// </summary>
    public void StartGame()
    {
        playedAmount = Utilities.PlayedGameAmount();
        gameOver = false;
        GameState = GameState.Playing;

        //Enable goldPoint, create gold at that position and start processing
        var targetPointWrapperTransform = FindTargetPointWrapper();
        InstantiateTargetPoint(targetPointWrapperTransform);
        StartCoroutine("Processing");
    }

    private Transform FindTargetPointWrapper()
    {
        return targetPointManager.transform.GetChild(Random.Range(0, targetPointManager.transform.childCount));
    }

    private void OnDestroy()
    {
        healthSO.NoLifeLeft -= OutOfLife;
        BallController.BallLost -= LoseBall;
    }

    void GameOver()
    {
        GameState = GameState.GameOver;
        LastSignificantGameStates.Enqueue(LastSignificantGameState.GameOver);
    }

    /// <summary>
    /// Create a ball
    /// </summary>
    public void CreateBall()
    {
        GameObject ball = Instantiate(ballPrefab, ballPoint.transform.position, Quaternion.identity) as GameObject;
        listBall.Add(ball);
    }

    /// <summary>
    /// Create gold 
    /// </summary>
    public void CreateTarget()
    {
        if (!gameOver)
        {
            //Stop all processing, disable current gold

            Debug.LogError($"Coroutine stopping");
            StopCoroutine("Processing");
            CleanupTarget();

            //Random new goldPoint and create new gold, then start processing
            Transform targetPointWrapperTransform = FindTargetPointWrapper();
            if (targetPointManager.transform.childCount <= 1) return;
            while (currentTargetPointWrapper == targetPointWrapperTransform.gameObject)
            {

                Debug.LogError($"While");
                targetPointWrapperTransform = FindTargetPointWrapper();
            }

            InstantiateTargetPoint(targetPointWrapperTransform);

            Debug.LogError($"Coroutine started");
            StartCoroutine("Processing");
            Debug.LogError($"Coroutine finished");

            LastSignificantGameStates.Enqueue(LastSignificantGameState.TargetReceived);
            Debug.LogError($"LastSignificantGameStates");
        }
    }

    private void InstantiateTargetPoint(Transform targetPointWrapperTransform)
    {
        currentTargetPointWrapper = Instantiate(targetPointWrapperTransform.gameObject, targetPointWrapperTransform.transform.position, Quaternion.identity, targetPointManager.transform) as GameObject;
        currentTargetPointWrapper.SetActive(true);
    }

    /// <summary>
    /// Create extra ball
    /// </summary>
    public void CreateTemporarySkill(GameObject temporarySkillPrefab)
    {
        if (!gameOver)
        {
            if (currentTemporarySkillPoint == null)
                currentTemporarySkillPoint = GetTemporarySkillPoint();

            //Random new extraBallPoing and create new ExtraBall, then start processing
            GameObject temporarySkillPoint = GetTemporarySkillPoint();
            while (currentTemporarySkillPoint == temporarySkillPoint)
            {
                temporarySkillPoint = GetTemporarySkillPoint();
            }
            temporarySkillPoint.SetActive(true);
            currentTemporarySkillPoint = temporarySkillPoint;
            currentTemporarySkill = InstantiateTemporarySkill(temporarySkillPrefab);
            _tempSkillCoroutine = StartCoroutine(TemporarySkillProcessing());

            if (temporarySkillPrefab.tag.Contains("ExtraLife"))
                LastSignificantGameStates.Enqueue(LastSignificantGameState.ExtraLifeReceived);
            else if (temporarySkillPrefab.tag.Contains("ExtraBall"))
                LastSignificantGameStates.Enqueue(LastSignificantGameState.ExtraBallReceived);
        }
    }

    private GameObject InstantiateTemporarySkill(GameObject temporarySkillPrefab)
    {
        Vector2 temporarySkillPos = Camera.main.ScreenToWorldPoint(currentTemporarySkillPoint.transform.position);
        return Instantiate(temporarySkillPrefab, temporarySkillPos, Quaternion.identity) as GameObject;
    }

    private GameObject GetTemporarySkillPoint()
    {
        return temporarySkillPointManager.transform.GetChild(Random.Range(0, temporarySkillPointManager.transform.childCount)).gameObject;
    }

    /// <summary>
    /// Change background element color, enable obstacles, update processing time
    /// </summary>
    public void CheckAndUpdateValue()
    {
        if (score.Score % scoreToIncreaseDifficulty == 0)
        {
            //Change background element color
            Color color = backgroundColor[Random.Range(0, backgroundColor.Length)];
            ushapeSpriteRenderer.color = color;
            backgroundSpriteRenderer.color = color;
            fenceSpriteRenderer.color = color;

            //Enable obstacles
            if (obstacleCounter < obstacleManager.transform.childCount)
            {
                obstacleManager.transform.GetChild(obstacleCounter).gameObject.SetActive(true);
                obstacleCounter++;
            }

            //Update processing time
            if (targetAliveTime > minTargetAliveTime)
            {
                targetAliveTime -= targetAliveTimeDecreaseValue;
            }
            else
            {
                targetAliveTime = minTargetAliveTime;
            }
        }

        if (score.Score % scoreToAddTemporarySkill == 0)
        {
            DisableTemporarySkill();
            if(Random.Range(1, 4) < 3)
                CreateTemporarySkill(extraBallPrefab);
            else
                CreateTemporarySkill(extraLifePrefab);
        }
    }

    IEnumerator Processing()
    {
        //Image img = currentTargetPointWrapper.transform.GetChild(0).GetComponent<Image>();
        //img.fillAmount = 0;
        float t = 0;
        Debug.LogError("Processing before while");
        while (t < targetAliveTime)
        {
            t += Time.deltaTime;
            //float fraction = t / targetAliveTime;
            //float newF = Mathf.Lerp(0, 1, fraction);
            //img.fillAmount = newF;
            yield return null;
        }
        Debug.LogError("Processing after while");

        if (!gameOver)
        {
            Debug.LogError("Before cleanup target");
            CleanupTarget();
            //Cleanup removed balls
            listBall.RemoveAll(x => x == null);
            var count = listBall.Count;

            Debug.LogError("Before ball count for");
            for (int i = 0; i < count; i++)
            {
                var ball = listBall[listBall.Count - 1];
                ball.GetComponent<BallController>().Exploring();
                LoseBall(ball);
            }

            Debug.LogError("Processing before LastSignificantGameStates");
            LastSignificantGameStates.Enqueue(LastSignificantGameState.TargetMissed);
            Debug.LogError("Processing after LastSignificantGameStates");
        }
    }

    private void CleanupTarget()
    {
        currentTargetPointWrapper.SetActive(false);
        Destroy(currentTargetPointWrapper);
    }

    IEnumerator TemporarySkillProcessing()
    {
        //Image img = currentTemporarySkillPoint.GetComponent<Image>();
        //img.fillAmount = 0;
        float t = 0;
        while (t < temporarySkillAliveTime)
        {
            t += Time.deltaTime;
            float fraction = t / temporarySkillAliveTime;
            float newF = Mathf.Lerp(0, 1, fraction);
            //if(img != null)
            //    img.fillAmount = newF;
            yield return null;
        }

        if (!gameOver)
        {
            var skillName = currentTemporarySkill != null ? currentTemporarySkill.name : "";
            PlayTemporarySkillParticle();
            if (skillName.Contains("ExtraBall"))
                LastSignificantGameStates.Enqueue(LastSignificantGameState.ExtraBallMissed);
            else if(skillName.Contains("ExtraLife"))
                LastSignificantGameStates.Enqueue(LastSignificantGameState.ExtraLifeMissed);
        }
    }
    void OnApplicationQuit()
    {
        var highScore = score.GetHighScore();
        FirebaseAnalyticsManager.SendApplicationQuitInfoEvent(new ApplicationQuitInfo(BackgroundType.Quit,  highScore, playedAmount, LastSignificantGameStates));

    }

    private void OnApplicationPause(bool pause)
    {
        var highScore = score.GetHighScore();
        FirebaseAnalyticsManager.SendApplicationQuitInfoEvent(new ApplicationQuitInfo(BackgroundType.Pause, highScore, playedAmount, LastSignificantGameStates));
    }
}
