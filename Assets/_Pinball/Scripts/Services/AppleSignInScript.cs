using System;
using System.Text;
using System.Threading.Tasks;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Models.Enums;
using Assets._Pinball.Scripts.Services;
using Firebase.Crashlytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AppleSignInScript : MonoBehaviour
{
    public static AppleSignInScript Instance;
    public bool IsAuthenticated { get; set; }
    public event Action<UserInfo> OnAppleUserLogIn = delegate { };
    public event Action OnAppleUserLogOut = delegate { };

    private IAppleAuthManager appleAuthManager;
    private readonly string AppleUserInfo = "AppleUserInfo";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
       // If the current platform is supported
       if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            this.appleAuthManager = new AppleAuthManager(deserializer);
            var userInfo = UserCacheService.Instance.GetUserInfo();
            if(userInfo != null)
                GetCredentialStates(userInfo.ExternalProviderId);

            this.appleAuthManager.SetCredentialsRevokedCallback(RevokeCredentials);
        }
    }

    private void RevokeCredentials(string result)
    {
        // Sign in with Apple Credentials were revoked.
        // Discard credentials/user id and go to login screen.
        IsAuthenticated = false;
        ExternalProviderStateHelper.SetLastLoginState(ExternalProviderLastLoginType.Ios_State, ExternalProviderState.LoggedOut);
    }

    void Update()
    {
        // Updates the AppleAuthManager instance to execute
        // pending callbacks inside Unity's execution loop
        if (this.appleAuthManager != null)
        {
            this.appleAuthManager.Update();
        }
    }

    public void SignIn()
    {
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        this.appleAuthManager.LoginWithAppleId(loginArgs, async credential => {
            // Obtained credential, cast it to IAppleIDCredential
            var appleIdCredential = credential as IAppleIDCredential;
            if (appleIdCredential != null)
            {
                // Apple User ID
                // You should save the user ID somewhere in the device
                var userId = appleIdCredential.User;

                //Email (Received ONLY in the first login)
                var email = appleIdCredential.Email;

                //Full name (Received ONLY in the first login)
                // Identity token
                var identityToken = Encoding.UTF8.GetString(
                            appleIdCredential.IdentityToken,
                            0,
                            appleIdCredential.IdentityToken.Length);

                // Authorization code
                var authorizationCode = Encoding.UTF8.GetString(
                            appleIdCredential.AuthorizationCode,
                            0,
                            appleIdCredential.AuthorizationCode.Length);

                await LinkWithAppleAsync(identityToken);
                // And now you have all the information to create/login a user in your system
                IsAuthenticated = true;
                ExternalProviderStateHelper.SetLastLoginState(ExternalProviderLastLoginType.Ios_State, ExternalProviderState.LoggedIn);
                OnAppleUserLogIn(GetUserInfo(appleIdCredential, userId));
            }
        }, error => {
            // Something went wrong
            var authorizationErrorCode = error.GetAuthorizationErrorCode();
            var errorMessage = $"Apple login error: {error.Code}: {error.LocalizedDescription}";
            Debug.LogError(errorMessage);
            Crashlytics.Log(errorMessage);
        });
    }

    public void QuickLogin()
    {
        var quickLoginArgs = new AppleAuthQuickLoginArgs();

        this.appleAuthManager.QuickLogin(quickLoginArgs, async credential => {
            // Received a valid credential!
            // Try casting to IAppleIDCredential or IPasswordCredential

            // Previous Apple sign in credential
            var appleIdCredential = credential as IAppleIDCredential;

            // Saved Keychain credential (read about Keychain Items)
            var passwordCredential = credential as IPasswordCredential;

            // Identity token
            var identityToken = Encoding.UTF8.GetString(
                        appleIdCredential.IdentityToken,
                        0,
                        appleIdCredential.IdentityToken.Length);
            await LinkWithAppleAsync(identityToken);

            IsAuthenticated = true;
            var userId = appleIdCredential.User;
            var email = appleIdCredential.Email;

            ExternalProviderStateHelper.SetLastLoginState(ExternalProviderLastLoginType.Ios_State, ExternalProviderState.LoggedIn);
            OnAppleUserLogIn(GetUserInfo(appleIdCredential, userId));
        }, error => {
            // Quick login failed. The user has never used Sign in With Apple on your app. Go to login screen
            var authorizationErrorCode = error.GetAuthorizationErrorCode();
            var errorMessage = $"Apple quick login error: {error.Code}: {error.LocalizedDescription}";
            Debug.LogError(errorMessage);
            Crashlytics.Log(errorMessage);
        });
    }

    async Task SignInWithAppleAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithAppleAsync(accessToken);
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

    public void GetCredentialStates(string userId)
    {
        this.appleAuthManager.GetCredentialState(userId, state => {
            switch (state)
            {
                case CredentialState.Authorized:
                    // User ID is still valid. Login the user.
                    IsAuthenticated = true;
                    break;

                case CredentialState.Revoked:
                    // User ID was revoked. Go to login screen.
                    IsAuthenticated = false;
                    break;

                case CredentialState.NotFound:
                    // User ID was not found. Go to login screen.
                    IsAuthenticated = false;
                    break;
            }
        }, error => {
            // Something went wrong
            var errorMessage = $"Apple get credentials error: {error.Code}: {error.LocalizedDescription}";
            Debug.LogError(errorMessage);
            Crashlytics.Log(errorMessage);
        });
    }

    public void Logout()
    {
        IsAuthenticated = false;
        ExternalProviderStateHelper.SetLastLoginState(ExternalProviderLastLoginType.Ios_State, ExternalProviderState.LoggedOut);
        OnAppleUserLogOut();
    }

    async Task LinkWithAppleAsync(string accessToken)
    {
        try
        {
            var linkOpt = new LinkOptions();
            linkOpt.ForceLink = true;
            await AuthenticationService.Instance.LinkWithAppleAsync(accessToken, linkOpt);
            Debug.LogError("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
            Crashlytics.Log($"This apple user is already linked with another account. Log in instead.");
            Crashlytics.LogException(ex);
            await SignInWithAppleAsync(accessToken);
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

    private UserInfo GetUserInfo(IAppleIDCredential appleIdCredential, string userId)
    {
        var fullName = appleIdCredential?.FullName;
        var name = string.Empty;
        if (fullName != null && !string.IsNullOrWhiteSpace(fullName?.GivenName))
        {
            var middleName = string.IsNullOrWhiteSpace(fullName.MiddleName) ? "" : $" {fullName.MiddleName}";
            var familyName = string.IsNullOrWhiteSpace(fullName.FamilyName) ? "" : $" {fullName.FamilyName}";
            name = $"{fullName.GivenName}{middleName}{familyName}";
        }
        var userInfo = UserCacheService.Instance.GetUserInfo();
        if(userInfo == null || string.IsNullOrWhiteSpace(userInfo.Username))
            return new UserInfo(name, fullName?.Nickname, "", AuthenticationType.AppleGameCenter, userId);
        else
            return new UserInfo(userInfo.Name, userInfo.Username, "", AuthenticationType.AppleGameCenter, userId);
    }
}
