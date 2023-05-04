using UnityEngine;

public class BallController : MonoBehaviour
{

    public static event System.Action<GameObject> BallLost = delegate { };
    public static event System.Action ExtraLifeCollected = delegate { };

    private SpriteRenderer spriteRenderer;
    private ScoreSO score;
    // Use this for initialization
    void Start()
    {
        gameObject.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();
        //transform.position += (Random.value >= 0.5f) ? (new Vector3(0.2f, 0)) : (new Vector3(-0.2f, 0));
        gameObject.SetActive(true);
        score = ScriptableObject.FindObjectOfType<ScoreSO>()??ScriptableObject.CreateInstance<ScoreSO>();
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
            score.AddScore(1);
            GameManager.Instance.CheckAndUpdateValue();

            PlayParticle(other, GameManager.Instance.hitGold);
            GameManager.Instance.CreateTarget();
        } else if(other.CompareTag("ExtraBall") && !GameManager.Instance.gameOver)
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
