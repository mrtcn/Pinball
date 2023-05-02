using System.Text.RegularExpressions;
using System;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Firebase.Crashlytics;

public class UserService : MonoBehaviour
{
    public Action OnUsernameUpdate = delegate { };

    public static UserService Instance;
    private void Awake()
    {
        Instance = this;
    }


    public async Task<bool> UpdateUsername(string username)
    {
        //if (!Regex.Match(username, @"^[a-zA-Z0-9]{3,49}").Success)
        //{
        //    return false;
        //}
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            Debug.LogException(ex);
            Crashlytics.LogException(ex);
        }
        UserCacheService.Instance.UpdateUsernameIfExists(username);
        OnUsernameUpdate();
        return true;
    }
}
