using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Health healthScript;
    [SerializeField] private Image currentHealthBar;
    [SerializeField] private Image totalHealthBar;
    // Start is called before the first frame update
    void Start()
    {
        totalHealthBar.fillAmount = healthScript.currentHealth / 10;
    }

    private void OnEnable()
    {
        GameManager.LifeLost += ReduceLife;
        BallController.ExtraLifeCollected += CollectExtraLife;
    }

    private void OnDisable()
    {
        GameManager.LifeLost -= ReduceLife;
        BallController.ExtraLifeCollected -= CollectExtraLife;
    }

    private void CollectExtraLife()
    {
        currentHealthBar.fillAmount = healthScript.currentHealth / 10;
    }

    private void ReduceLife()
    {
        currentHealthBar.fillAmount = healthScript.currentHealth / 10;
    }
}
