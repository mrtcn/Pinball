using Assets._Pinball.Scripts.Models;
using System;
using UnityEngine;

public class UserCacheService : MonoBehaviour
{
    public static UserCacheService Instance;
    public event Action<UserInfo> OnUserInfoUpdate = delegate { };

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        FacebookScript.Instance.OnFacebookUserLogIn += FacebookUserLoggedIn;
        FacebookScript.Instance.OnFacebookUserLogOut += FacebookUserLoggedOut;
    }

    private void FacebookUserLoggedOut()
    {
        var userInfo = GetUserInfo();
        userInfo.ExternalProviderId = null;
        userInfo.Name = null;
        userInfo.ImagePath = null;
        userInfo.AuthenticationType = AuthenticationType.Anonymous;
        SaveUserInfo(userInfo);
        OnUserInfoUpdate(userInfo);
    }

    private void OnDestroy()
    {
        FacebookScript.Instance.OnFacebookUserLogIn -= FacebookUserLoggedIn;
        FacebookScript.Instance.OnFacebookUserLogOut -= FacebookUserLoggedOut;
    }

    private void FacebookUserLoggedIn(UserInfo userInfo)
    {
        SaveUserInfo(userInfo);
        OnUserInfoUpdate(userInfo);
    }

    public void SaveUserInfo(UserInfo userInfo)
    {
        var json = JsonUtility.ToJson(userInfo);
        PlayerPrefs.SetString("UserInfo", json);
    }

    public UserInfo GetUserInfo()
    {
        var userInfoJson = PlayerPrefs.GetString("UserInfo");
        return JsonUtility.FromJson<UserInfo>(userInfoJson);
    }
}
