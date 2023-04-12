using GoogleMobileAds.Api;
using SgLib;
using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public static event Action NoLifeLeft = delegate { };
    public static event Action LifeLost = delegate { };

    [SerializeField]
    private float startingHealth;
    [SerializeField]
    private AdManager adManager;
    private readonly int playedToShowAd = 2;

    int updatesBeforeException;

    public float currentHealth { get; private set; }

    private void Start()
    {
        updatesBeforeException = 0;

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            adManager.LoadInterstitialAd();
        });
    }

    private void Update()
    {
        // Call the exception-throwing method here so that it's run
        // every frame update
        throwExceptionEvery60Updates();
    }
    // A method that tests your Crashlytics implementation by throwing an
    // exception every 60 frame updates. You should see reports in the
    // Firebase console a few minutes after running your app with this method.
    void throwExceptionEvery60Updates()
    {
        if (updatesBeforeException > 0)
        {
            updatesBeforeException--;
        }
        else
        {
            // Set the counter to 60 updates
            updatesBeforeException = 60;

            // Throw an exception to test your Crashlytics implementation
	    try
	    {
            	throw new System.Exception("test exception please ignore");
	    }
	    catch(Exception ex)
	    {
	    }

        }
    }

    private void OnDestroy()
    {
        adManager.Destroy();
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

    private void ReduceLife()
    {
        DropBall(1);
    }

    private void CollectExtraLife()
    {
        GainBall(1);
    }

    private void Awake()
    {
        currentHealth = startingHealth;
    }

    public void DropBall(float dropAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth - dropAmount, 0, startingHealth);

        if(currentHealth < 1) 
        {
            NoLifeLeft();
            var played = ScoreManager.Instance.UpdatePlayedGame(1);
            if(played % playedToShowAd == 0)
                adManager.ShowAd();
        }
        else
        {
            LifeLost();
        }
    }

    public void GainBall(float dropAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + dropAmount, 0, startingHealth);
    }
}
