using Assets._Pinball.Scripts.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }
    // Start is called before the first frame update
    async void Start()
    {
        var initOpt = new InitializationOptions();
#if UNITY_EDITOR
        initOpt.SetEnvironmentName("test");
#else
        initOpt.SetEnvironmentName("production");
#endif
        await UnityServices.InitializeAsync(initOpt);

        SetupEvents();

        if (AuthenticationService.Instance.IsAuthorized && !AuthenticationService.Instance.IsExpired && AuthenticationService.Instance.IsSignedIn)
            return;
        await SignInAnonymouslyAsync();
        if (AuthenticationService.Instance.IsAuthorized && !AuthenticationService.Instance.IsExpired && AuthenticationService.Instance.IsSignedIn)
        {
            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
        }
    }
    void Awake()
    {
        Instance = this;
    }

    public bool IsAuthenticated
    {
        get { return AuthenticationService.Instance.PlayerInfo.Identities.Any(); }
        private set { }
    }

    async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    // Setup authentication event handlers if desired
    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    public async Task<User> GetUserAsync()
    {
        if (!IsAuthenticated)
            return new User(AuthenticationService.Instance.PlayerId, "Anonymous");
        
        return new User(AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.Profile);
    }
}
