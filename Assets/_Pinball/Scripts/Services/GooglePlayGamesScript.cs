using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class GooglePlayGamesScript : MonoBehaviour
{
    public static GooglePlayGamesScript Instance;
    public Action OnGoogleUserLoggedIn = delegate { };
    public string Error;
    private void Start()
    {
        PlayGamesPlatform.Activate();
    }
    private void Awake()
    {
        Instance = this;
    }

    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
                {
                    try
                    {
                        var name = await AuthenticationService.Instance.GetPlayerNameAsync();
                        // Shows how to get a playerID
                        Debug.Log($"DebugLogs PlayerName1: {name}");
                        Debug.Log($"DebugLogs Social.localUser.userName: {Social.localUser.userName}");
                        var googleName = PlayGamesPlatform.Instance.GetUserDisplayName();
                        Debug.Log($"DebugLogs PlayGamesPlatform.Instance.GetUserDisplayName(): {googleName}");
                        if (!string.IsNullOrWhiteSpace(googleName))
                            await AuthenticationService.Instance.UpdatePlayerNameAsync(googleName);
                        name = await AuthenticationService.Instance.GetPlayerNameAsync();
                        Debug.Log($"DebugLogs PlayerName2: {name}");
                        await Link(code);
                        OnGoogleUserLoggedIn();
                        // This token serves as an example to be used for SignInWithGooglePlayGames
                    }
                    catch (AuthenticationException ex)
                    {
                        if (ex.ErrorCode != 10003) throw;

                        Debug.Log($"AuthenticationException: ErrorCode: {ex.ErrorCode} - Message: {ex.Message}");
                    }
                });
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
            }
        });
    }

    public async Task Link(string code)
    {
        try
        {
            var linkOpt = new LinkOptions();
            linkOpt.ForceLink = true;
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code, linkOpt);
        }
        catch (AuthenticationException ex)
        {
            Debug.Log($"Link AuthenticationException: ErrorCode: {ex.ErrorCode} - Message: {ex.Message}");
        }
    }

    public bool IsAuthenticated()
    {
        return PlayGamesPlatform.Instance.IsAuthenticated();
    }
}
