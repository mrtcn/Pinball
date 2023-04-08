using GoogleMobileAds.Api;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    // These ad units are configured to always serve test ads.

#if DEBUG && UNITY_ANDROID
    string _adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif DEBUG && UNITY_IPHONE
    string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#elif UNITY_ANDROID
    string _adUnitId = "ca-app-pub-7911525237540335/4988807415";
#elif UNITY_IPHONE
    string _adUnitId = "ca-app-pub-7911525237540335/8471142457";
#else
    string _adUnitId = "unused";
#endif


    private InterstitialAd interstitialAd;

    /// <summary>
    /// Loads the interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder()
                .AddKeyword("unity-admob-pinball-crush")
                .Build();

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                interstitialAd = ad;
            });
    }

    /// <summary>
    /// Shows the interstitial ad.
    /// </summary>
    public void ShowAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }
    /// <summary>
    /// Destroy ad objects
    /// </summary>
    public void Destroy()
    {
        interstitialAd.Destroy();
    }
}
