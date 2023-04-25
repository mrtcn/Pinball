using SgLib;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoSection : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI username;
    [SerializeField]
    private TextMeshProUGUI scoreAndTier;
    [SerializeField]
    private Image profileImage;
    // Start is called before the first frame update
    void Start()
    {
        ProfileService.Instance.OnUsernameUpdate += UsernameUpdate;
        AuthService.Instance.OnUserLoggedIn += UserLoggedIn;
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn += UserLoggedIn;
    }

    private void OnDestroy()
    {
        ProfileService.Instance.OnUsernameUpdate -= UsernameUpdate;
        AuthService.Instance.OnUserLoggedIn -= UserLoggedIn;
        GooglePlayGamesScript.Instance.OnGoogleUserLoggedIn -= UserLoggedIn;
    }

    private async void UsernameUpdate()
    {
        await UpdateAllFields();
    }

    private async void UserLoggedIn()
    {
        await UpdateAllFields();
    }

    private async Task UpdateAllFields()
    {
        var userInfo = await AuthService.Instance.GetUserAsync();
        username.text = userInfo.Username;
        scoreAndTier.text = $"{userInfo.Rank} {userInfo.HighScore}";
        StartCoroutine(Utilities.Instance.LoadImage(userInfo.ImagePath, profileImage));
        //StartCoroutine(DownloadImage("https://img.freepik.com/free-vector/businessman-character-avatar-isolated_24877-60111.jpg"));
    }
}
