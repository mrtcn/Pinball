using Assets._Pinball.Scripts.Models;
using Firebase.Crashlytics;
using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class UpdateNameDialog : MonoBehaviour
{
    public static UpdateNameDialog Instance;

    [SerializeField]
    private GameObject usernameEditDialog;
    [SerializeField]
    private TMP_InputField usernameInputField;
    [SerializeField]
    private TextMeshProUGUI inputFieldError;
    [SerializeField]
    private GameObject cancelButton;

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        var userInfo = UserCacheService.Instance.GetUserInfo();
        userInfo = userInfo ?? new UserInfo(null, AuthenticationService.Instance.PlayerName, null, AuthenticationType.Anonymous, null);
        usernameInputField.text = userInfo.Username;
        inputFieldError.text = "";
    }

    public void OpenDialog(bool hideCancelButton = false)
    {
        if(hideCancelButton)
        {
            cancelButton.SetActive(false);
        }
        else
        {
            cancelButton.SetActive(true);
        }
        usernameEditDialog.SetActive(true);


    }

    public async void UpdateEditDialog()
    {
        try
        {
            var success = await UserService.Instance.UpdateUsername(usernameInputField.text);
            if(!success) 
            {
                inputFieldError.text = "Please enter a valid username";
                return;
            }
        }
        catch (Exception ex)
        {
            Crashlytics.LogException(ex);
            inputFieldError.text = "An error occured during updating the username.";
        }

        inputFieldError.text = "";
        usernameEditDialog.SetActive(false);
    }

    public void CloseEditDialog()
    {
        inputFieldError.text = "";
        usernameEditDialog.SetActive(false);

    }
}
