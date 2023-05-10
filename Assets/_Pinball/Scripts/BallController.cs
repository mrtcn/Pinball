using UnityEngine;

public class BallController : MonoBehaviour
{

    public static event System.Action<GameObject> BallLost = delegate { };
    public static event System.Action ExtraLifeCollected = delegate { };
    public float rotateSpeed = 0;
    public float upMaxSpeed = 30;
    public float downMaxSpeed = 5;
    private SpriteRenderer spriteRenderer;
    private ScoreSO score;
    [SerializeField]
    private Rigidbody2D ballRigidBody;
    private Vector3 originalPosition = new Vector3(0,0,0);
    // Use this for initialization
    void Start()
    {
        gameObject.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.SetActive(true);
        score = ScriptableObject.FindObjectOfType<ScoreSO>()??ScriptableObject.CreateInstance<ScoreSO>();
    }
    private void Update()
    {   Vector2 v = ballRigidBody.velocity;
        float speed = Vector3.Magnitude(v);
        Vector3 moveDirection = ballRigidBody.transform.position - originalPosition;

        if (moveDirection.y >= 0)
            ballRigidBody.velocity = Vector3.ClampMagnitude(ballRigidBody.velocity, upMaxSpeed);
        else
            ballRigidBody.velocity = Vector3.ClampMagnitude(ballRigidBody.velocity, downMaxSpeed);

        originalPosition = ballRigidBody.transform.position;
        if (moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            if (moveDirection.y >= 0)
            {
                angle = Mathf.Clamp(angle, -20, 20);
                if(moveDirection.x >= 0)
                    ballRigidBody.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
                else
                    ballRigidBody.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }                
            else if (speed > 5)
            {

                angle = Mathf.Clamp(angle, -205, -165);
                ballRigidBody.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }                
            else
                ballRigidBody.transform.rotation = Quaternion.identity;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Dead") && !GameManager.Instance.gameOver)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.eploring);
            
            BallLost(gameObject);

            Exploring();            
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {        
        if (other.CompareTag("Gold") && !GameManager.Instance.gameOver)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.hitGold);
            Debug.LogError($"Gold hit sound");
            score.AddScore(AppInfo.TargetScore);
            Debug.LogError($"Score added");
            GameManager.Instance.CheckAndUpdateValue();

            Debug.LogError($"CheckAndUpdateValue invoked");
            PlayParticle(other, GameManager.Instance.hitGold);
            Debug.LogError($"Particle played");
            GameManager.Instance.CreateTarget();
            Debug.LogError($"Create target invoked");
        }
        else if (other.CompareTag("ExtraBall") && !GameManager.Instance.gameOver)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded);
            PlayParticle(other, GameManager.Instance.hitTemporarySkill);
            GameManager.Instance.PlayTemporarySkillParticle();
            GameManager.Instance.CreateBall();
        }
        else if (other.CompareTag("ExtraLife") && !GameManager.Instance.gameOver)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded);
            PlayParticle(other, GameManager.Instance.hitGold);
            GameManager.Instance.PlayTemporarySkillParticle();
            ExtraLifeCollected();
        }
    }

    private void PlayParticle(Collider2D other, ParticleSystem particleSystem)
    {
        ParticleSystem particle = Instantiate(particleSystem, other.transform.position, Quaternion.identity) as ParticleSystem;
        var main = particle.main;
        main.startColor = other.gameObject.GetComponent<SpriteRenderer>().color;
        particle.Play();
        Destroy(particle.gameObject, 1f);
        Destroy(other.gameObject);
    }

    /// <summary>
    /// Handle when player die
    /// </summary>
    public void Exploring()
    {
        ParticleSystem particle = Instantiate(GameManager.Instance.die, transform.position, Quaternion.identity) as ParticleSystem;
        var main = particle.main;
        main.startColor = spriteRenderer.color;
        particle.Play();
        Destroy(particle.gameObject, 1f);
        Destroy(gameObject);
    }

}
