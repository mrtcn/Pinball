using System.Text.RegularExpressions;
using System;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;

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
        if (!Regex.Match(username, @"^[a-zA-Z0-9]{3,49}").Success)
        {
            return false;
        }

        await AuthenticationService.Instance.UpdatePlayerNameAsync(username);

        OnUsernameUpdate();
        return true;
    }
}
