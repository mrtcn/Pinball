using Assets._Pinball.Scripts.Models;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using Unity.Services.Authentication;
using UnityEngine;

public class GooglePlayGamesScript : MonoBehaviour
{
    public static GooglePlayGamesScript Instance;
    public string Error;
    public Action OnUserLoggedIn = delegate { };

    private void Awake()
    {
        Instance = this;
    }

    public void Activate()
    {
        //Initialize PlayGamesPlatform
        PlayGamesPlatform.Activate();
    }

    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    PlayerPrefs.SetString(PlayerPrefType.GooglePlayGamesAuthorizationCode.ToString(), code);
                    AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code);

                    OnUserLoggedIn();
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                });
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
            }
        });
    }
}
