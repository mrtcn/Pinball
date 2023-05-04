using System;
using UnityEngine;

public class HealthSO : ScriptableObject
{
    public event Action NoLifeLeft = delegate { };

    private float startingHealth = 3;

    public float currentHealth { get; private set; }

    public void Init()
    {
        SubscribeToEvents();
        currentHealth = startingHealth;
    }

    private void SubscribeToEvents()
    {
        UnsubscribeEvents();
        GameManager.LifeLost += ReduceLife;
        BallController.ExtraLifeCollected += CollectExtraLife;
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        GameManager.LifeLost -= ReduceLife;
        BallController.ExtraLifeCollected -= CollectExtraLife;
    }

    private void ReduceLife()
    {
        DropBall(1);
    }

    private void CollectExtraLife()
    {
        GainBall(1);
    }

    public void DropBall(float dropAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth - dropAmount, 0, startingHealth);

        if (currentHealth < 1)
        {
            //GAME OVER
            NoLifeLeft();
        }
    }

    public void GainBall(float dropAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + dropAmount, 0, startingHealth);
    }
}
