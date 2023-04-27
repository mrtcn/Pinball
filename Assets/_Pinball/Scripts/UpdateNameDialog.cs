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
    private TextMeshProUGUI usernameTextField;
    [SerializeField]
    private GameObject usernameEditDialog;
    [SerializeField]
    private TMP_InputField usernameInputField;
    [SerializeField]
    private TextMeshProUGUI inputFieldError;
    [SerializeField]
    private GameObject cancelButton;
    [SerializeField]
    private GameObject updateButton;
    [SerializeField]
    private TextMeshProUGUI updateButtonText;

    private void Awake()
    {
        Instance = this;
    }

    private void FillFields()
    {
        var userInfo = UserCacheService.Instance.GetUserInfo();
        userInfo = userInfo ?? new UserInfo(null, AuthenticationService.Instance.PlayerName, null, AuthenticationType.Anonymous, null);
        string normalizedUsername = NormalizeUsername(userInfo);
        usernameInputField.text = normalizedUsername;
        inputFieldError.text = "";
    }

    private static string NormalizeUsername(UserInfo userInfo)
    {
        if(userInfo == null || string.IsNullOrWhiteSpace(userInfo.Username))
            return string.Empty;
        var usernameArray = userInfo.Username.Split("#");
        var normalizedUsername = usernameArray[0];
        return normalizedUsername;
    }

    public void OpenDialog(bool hideCancelButton = false)
    {
        FillFields();
        if (hideCancelButton)
        {
            usernameTextField.text = "Enter Username";
            cancelButton.SetActive(false);
            updateButtonText.text = "OK";
            SetUpdateButtonPosition(0);
        }
        else
        {
            usernameTextField.text = "Update Username";
            updateButtonText.text = "UPDATE";
            cancelButton.SetActive(true);
            SetUpdateButtonPosition(100);
        }
        usernameEditDialog.SetActive(true);


    }

    private void SetUpdateButtonPosition(int posx)
    {
        updateButton.transform.GetLocalPositionAndRotation(out Vector3 updateButtonPosition, out Quaternion updateButtonRotation);
        updateButtonPosition.x = posx;
        updateButton.transform.SetLocalPositionAndRotation(updateButtonPosition, updateButtonRotation);
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
