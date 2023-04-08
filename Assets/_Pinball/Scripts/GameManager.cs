using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SgLib;
using System.Collections.Generic;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public class GameManager : MonoBehaviour
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

    private GameState _gameState = GameState.Prepare;
    private Coroutine _tempSkillCoroutine;
    [Header("Gameplay References")]
    public UIManager uIManager;
    public GameObject ballPrefab;
    public GameObject ballPoint;
    public GameObject obstacleManager;
    public GameObject targetPointManager;
    public GameObject temporarySkillPointManager;
    public GameObject leftFlipper;
    public GameObject rightFlipper;
    public GameObject targetPrefab;
    public GameObject extraBallPrefab;
    public GameObject extraLifePrefab;    
    public GameObject ushape;
    public GameObject background;
    public GameObject fence;
    [HideInInspector]
    public GameObject currentTargetPoint;
    [HideInInspector]
    public GameObject currentTarget;
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
    public float torqueForce;
    public int scoreToIncreaseDifficulty = 10;
    public float targetAliveTime = 20;
    public float targetAliveTimeDecreaseValue = 2;
    public float temporarySkillAliveTime = 10;
    public float temporarySkillAliveTimeDecreaseValue = 1;
    public int minTargetAliveTime = 3;
    public int scoreToAddTemporarySkill = 1;

    private List<GameObject> listBall = new List<GameObject>();
    private Rigidbody2D leftFlipperRigid;
    private Rigidbody2D rightFlipperRigid;
    private SpriteRenderer ushapeSpriteRenderer;
    private SpriteRenderer backgroundSpriteRenderer;
    private SpriteRenderer fenceSpriteRenderer;
    private SpriteRenderer leftFlipperSpriteRenderer;
    private SpriteRenderer rightFlipperSpriteRenderer;
    private int obstacleCounter = 0;
    private bool stopProcessing;

    private void OnEnable()
    {
        Health.NoLifeLeft += OutOfLife;
        Health.LifeLost += LoseLife;
        BallController.BallLost += LoseBall;
    }

    private void OnDisable()
    {
        Health.NoLifeLeft -= OutOfLife;
        Health.LifeLost -= LoseLife;
        BallController.BallLost -= LoseBall;
    }

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
    }

    private void LoseBall(GameObject ball)
    {        
        //remove the ball from the list
        listBall.Remove(ball);

        //No ball left -> game over
        if (listBall.Count == 0)
        {
            LifeLost();
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
            currentTemporarySkill.SetActive(false);

        if (currentTemporarySkillPoint != null)
            currentTemporarySkillPoint.SetActive(false);
    }

    private void DestroyTarget()
    {
        currentTargetPoint.SetActive(false);
        ParticleSystem particle = Instantiate(hitGold, currentTarget.transform.position, Quaternion.identity) as ParticleSystem;
        var main = particle.main;
        main.startColor = currentTarget.gameObject.GetComponent<SpriteRenderer>().color;
        particle.Play();
        Destroy(particle.gameObject, 1f);
        Destroy(currentTarget.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        GameState = GameState.Prepare;

        ScoreManager.Instance.Reset();
        currentTargetPoint = null;
        currentTemporarySkillPoint = null;
        leftFlipperRigid = leftFlipper.GetComponent<Rigidbody2D>();
        rightFlipperRigid = rightFlipper.GetComponent<Rigidbody2D>();
        ushapeSpriteRenderer = ushape.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
        fenceSpriteRenderer = fence.GetComponent<SpriteRenderer>();
        leftFlipperSpriteRenderer = leftFlipper.GetComponent<SpriteRenderer>();
        rightFlipperSpriteRenderer = rightFlipper.GetComponent<SpriteRenderer>();

        //Change color of backgorund, ushape, fence, flippers
        Color color = backgroundColor[Random.Range(0, backgroundColor.Length)];
        ushapeSpriteRenderer.color = color;
        backgroundSpriteRenderer.color = color; 
        fenceSpriteRenderer.color = color;
        leftFlipperSpriteRenderer.color = color;
        rightFlipperSpriteRenderer.color = color;

        if (!UIManager.firstLoad)
        {
            StartGame();
            CreateBall();
        }
    }
	
    // Update is called once per frame
    void Update()
    {
        if (!gameOver && !UIManager.firstLoad)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.flipping);
                Vector3 mouseInput = Input.mousePosition;
                //Flipping right
                if (mouseInput.x >= Screen.width / 2f)
                {
                    AddTorque(rightFlipperRigid, -torqueForce);
                }

                //Flipping left
                if (mouseInput.x < Screen.width / 2f)
                {
                    AddTorque(leftFlipperRigid, torqueForce);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 mouseHolding = Input.mousePosition;

                //Holding right
                if (mouseHolding.x >= Screen.width / 2f)
                {
                    AddTorque(rightFlipperRigid, -torqueForce);
                }

                //Holdding left
                if (mouseHolding.x < Screen.width / 2f)
                {
                    AddTorque(leftFlipperRigid, torqueForce);
                }
            }
        }
    }

    /// <summary>
    /// Fire game event, create gold
    /// </summary>
    public void StartGame()
    {
        GameState = GameState.Playing;

        //Enable goldPoint, create gold at that position and start processing
        GameObject targetPoint = targetPointManager.transform.GetChild(Random.Range(0, targetPointManager.transform.childCount)).gameObject;
        targetPoint.SetActive(true);
        currentTargetPoint = targetPoint;
        Vector2 pos = Camera.main.ScreenToWorldPoint(currentTargetPoint.transform.position);
        currentTarget = Instantiate(targetPrefab, pos, Quaternion.identity) as GameObject;
        StartCoroutine("Processing");

        ////Enable extraBall, create extraball at that position and start processing
        //GameObject extraBallPoint = temporarySkillPointManager.transform.GetChild(Random.Range(0, temporarySkillPointManager.transform.childCount)).gameObject;
        //extraBallPoint.SetActive(true);
        //currentTemporarySkillPoint = extraBallPoint;
        //Vector2 extraBallPos = Camera.main.ScreenToWorldPoint(currentTemporarySkillPoint.transform.position);
        //currentTemporarySkill = Instantiate(extraBallPrefab, extraBallPos, Quaternion.identity) as GameObject;


        //StartCoroutine("TemporarySkillProcessing"); 
    }

    void GameOver()
    {
        GameState = GameState.GameOver;
    }

    void AddTorque(Rigidbody2D rigid, float force)
    {
        rigid.AddTorque(force);
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
            StopCoroutine("Processing");
            currentTargetPoint.SetActive(false);

            //Random new goldPoint and create new gold, then start processing
            GameObject goldPoint = targetPointManager.transform.GetChild(Random.Range(0, targetPointManager.transform.childCount)).gameObject;
            while (currentTargetPoint == goldPoint)
            {
                goldPoint = targetPointManager.transform.GetChild(Random.Range(0, targetPointManager.transform.childCount)).gameObject;
            }
            goldPoint.SetActive(true);
            currentTargetPoint = goldPoint;
            Vector2 goldPos = Camera.main.ScreenToWorldPoint(currentTargetPoint.transform.position);
            currentTarget = Instantiate(targetPrefab, goldPos, Quaternion.identity) as GameObject;
            StartCoroutine("Processing");
        }
    }

    /// <summary>
    /// Create extra ball
    /// </summary>
    public void CreateTemporarySkill(GameObject temporarySkillPrefab)
    {
        if (!gameOver)
        {
            if (currentTemporarySkillPoint == null)
                currentTemporarySkillPoint = temporarySkillPointManager.transform.GetChild(Random.Range(0, temporarySkillPointManager.transform.childCount)).gameObject;

            //Random new extraBallPoing and create new ExtraBall, then start processing
            GameObject temporarySkillPoint = temporarySkillPointManager.transform.GetChild(Random.Range(0, temporarySkillPointManager.transform.childCount)).gameObject;
            while (currentTemporarySkillPoint == temporarySkillPoint)
            {
                temporarySkillPoint = temporarySkillPointManager.transform.GetChild(Random.Range(0, temporarySkillPointManager.transform.childCount)).gameObject;
            }
            temporarySkillPoint.SetActive(true);
            currentTemporarySkillPoint = temporarySkillPoint;
            Vector2 temporarySkillPos = Camera.main.ScreenToWorldPoint(currentTemporarySkillPoint.transform.position);
            currentTemporarySkill = Instantiate(temporarySkillPrefab, temporarySkillPos, Quaternion.identity) as GameObject;
            _tempSkillCoroutine = StartCoroutine(TemporarySkillProcessing());
        }
    }

    /// <summary>
    /// Change background element color, enable obstacles, update processing time
    /// </summary>
    public void CheckAndUpdateValue()
    {
        if (ScoreManager.Instance.Score % scoreToIncreaseDifficulty == 0)
        {
            //Change background element color
            Color color = backgroundColor[Random.Range(0, backgroundColor.Length)];
            ushapeSpriteRenderer.color = color;
            backgroundSpriteRenderer.color = color;
            fenceSpriteRenderer.color = color;
            leftFlipperSpriteRenderer.color = color;
            rightFlipperSpriteRenderer.color = color;

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

        if (ScoreManager.Instance.Score % scoreToAddTemporarySkill == 0)
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
        Image img = currentTargetPoint.GetComponent<Image>();
        img.fillAmount = 0;
        float t = 0;
        while (t < targetAliveTime)
        {
            t += Time.deltaTime;
            float fraction = t / targetAliveTime;
            float newF = Mathf.Lerp(0, 1, fraction);
            img.fillAmount = newF;
            yield return null;
        }

        if (!gameOver)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
            gameOver = true;
            for (int i = 0; i < listBall.Count; i++)
            {
                listBall[i].GetComponent<BallController>().Exploring();
            }

            DestroyTarget();
            PlayTemporarySkillParticle();
            StartGame();
            CreateBall();
        }
    }

    IEnumerator TemporarySkillProcessing()
    {
        Image img = currentTemporarySkillPoint.GetComponent<Image>();
        img.fillAmount = 0;
        float t = 0;
        while (t < temporarySkillAliveTime)
        {
            t += Time.deltaTime;
            float fraction = t / temporarySkillAliveTime;
            float newF = Mathf.Lerp(0, 1, fraction);
            img.fillAmount = newF;
            yield return null;
        }

        if (!gameOver)
        {
            PlayTemporarySkillParticle();
        }
    }
}
