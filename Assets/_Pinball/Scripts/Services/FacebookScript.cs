using Assets._Pinball.Scripts.Models;
using Facebook.Unity;
using Firebase.Crashlytics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class FacebookScript : MonoBehaviour
{
    public static FacebookScript Instance;
    public event Action<UserInfo> OnFacebookUserLogIn = delegate { };
    public event Action OnFacebookUserLogOut = delegate { };
    public string Token;
    public string Error;

    // Awake function from Unity's MonoBehavior
    void Awake()
    {
        Instance = this;
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    public void Login()
    {            
        // Define the permissions
        var perms = new List<string>() { "public_profile", "email" };        
        FB.LogInWithReadPermissions(perms, async result =>
        {
            if (FB.IsLoggedIn)
            {
                Token = AccessToken.CurrentAccessToken.TokenString;
                Debug.LogError($"Facebook Login token: {Token}");

                await LinkWithFacebookAsync(Token);

                FB.API("/me?fields=id,name,picture", HttpMethod.GET, LoginCallback);

            }
            else
            {
                Error = "User cancelled login";
                Debug.Log("[Facebook Login] User cancelled login");
            }
        });
    }

    void LoginCallback(IGraphResult result)
    {
        if (result.Error == null)
        {
            var username = AuthenticationService.Instance.PlayerName;
            var userId = result.ResultDictionary["id"].ToString();
            var name = result.ResultDictionary["name"].ToString();
            var imageResult = result.ResultDictionary["picture"];
            var imageData = imageResult as Dictionary<string, object>;
            var image = "";
            if (imageData != null)
            {
                if (imageData.TryGetValue("data", out Dictionary<string, object> value))
                {
                    image = value.GetValueOrDefault("url")?.ToString();
                }

            }

            OnFacebookUserLogIn(new UserInfo(name, username, image, AuthenticationType.Facebook, userId));
        }
    }

    async Task SignInWithFacebookAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithFacebookAsync(accessToken);
            Debug.LogError("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Crashlytics.LogException(ex);
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Crashlytics.LogException(ex);
            Debug.LogException(ex);
        }
    }
    async Task LinkWithFacebookAsync(string accessToken)
    {
        try
        {
            var linkOpt = new LinkOptions();
            linkOpt.ForceLink = true;
            await AuthenticationService.Instance.LinkWithFacebookAsync(accessToken, linkOpt);
            Debug.LogError("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
            await SignInWithFacebookAsync(accessToken);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Crashlytics.LogException(ex);
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Crashlytics.LogException(ex);
            Debug.LogException(ex);
        }
    }

    public bool IsAuthenticated()
    {
        return FB.IsLoggedIn;
    }

    public void Logout()
    {
        FB.LogOut();
        OnFacebookUserLogOut();
    }
}
