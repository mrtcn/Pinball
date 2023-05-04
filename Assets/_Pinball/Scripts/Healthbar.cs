using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    private HealthSO healthSO;
    [SerializeField] private Image currentHealthBar;
    [SerializeField] private Image totalHealthBar;
    // Start is called before the first frame update
    void Start()
    {
        healthSO = ScriptableObject.FindObjectOfType<HealthSO>() ?? ScriptableObject.CreateInstance<HealthSO>();
        totalHealthBar.fillAmount = healthSO.currentHealth / 10;

        GameManager.LifeLost += ReduceLife;
        BallController.ExtraLifeCollected += CollectExtraLife;
    }

    private void OnDestroy()
    {
        GameManager.LifeLost -= ReduceLife;
        BallController.ExtraLifeCollected -= CollectExtraLife;
    }

    private void CollectExtraLife()
    {
        currentHealthBar.fillAmount = healthSO.currentHealth / 10;
    }

    private void ReduceLife()
    {
        currentHealthBar.fillAmount = healthSO.currentHealth / 10;
    }
}
