using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using Assets._Pinball.Scripts.Services;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SgLib
{
    /// <summary>
    /// This class is not a singleton, but there's an static member Instance which is
    /// assigned to any alive instance of this class so that other classes can use it to access 
    /// public member methods. This way we can use it as a MonoBehaviour object to assign
    /// for button event in the inspector, while still can call its methods from the script
    /// just as an actual singleton.
    /// Usage: place a Utility object attached with this script in any scene needs to use these utilities.
    /// </summary>
    public class Utilities : Singleton<Utilities>
    {
        private const string PLAYED = "PLAYED";

        public static IEnumerator CRWaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }

        public void PlayButtonSound()
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.clickButton);
        }

        // Opens a specific scene
        public void GoToScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void ToggleMute()
        {
            SoundManager.Instance.ToggleMute();
        }

        public void RateApp()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    Application.OpenURL(AppInfo.Instance.APPSTORE_LINK);
                    break;
        			
                case RuntimePlatform.Android:
                    Application.OpenURL(AppInfo.Instance.PLAYSTORE_LINK);
                    break;
            }
        }

        public void ShowMoreGames()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    Application.OpenURL(AppInfo.Instance.APPSTORE_HOMEPAGE);
                    break;
    			
                case RuntimePlatform.Android:
                    Application.OpenURL(AppInfo.Instance.PLAYSTORE_HOMEPAGE);
                    break;
            }
        }

        public void OpenFacebookPage()
        {
            Application.OpenURL(AppInfo.Instance.FACEBOOK_LINK);
        }

        public void OpenTwitterPage()
        {
            Application.OpenURL(AppInfo.Instance.TWITTER_LINK);
        }

        public void ContactUs()
        {
            string email = AppInfo.Instance.SUPPORT_EMAIL;
            string subject = EscapeURL(AppInfo.Instance.APP_NAME + " [" + Application.version + "] Support");
            string body = EscapeURL("");
            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        public string EscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }

        public int[] GenerateShuffleIndices(int length)
        {
            int[] array = new int[length];

            // Populate array
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }

            // Shuffle
            for (int j = 0; j < array.Length; j++)
            {
                int tmp = array[j];
                int randomPos = UnityEngine.Random.Range(j, array.Length);
                array[j] = array[randomPos];
                array[randomPos] = tmp;
            }

            return array;
        }

        /// <summary>
        /// Stores a DateTime as string to PlayerPrefs.
        /// </summary>
        /// <param name="time">Time.</param>
        /// <param name="ppkey">Ppkey.</param>
        public static void StoreTime(string ppkey, DateTime time)
        {
            PlayerPrefs.SetString(ppkey, time.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the stored string in the PlayerPrefs and converts it to a DateTime.
        /// If no time stored previously, defaultTime is returned.
        /// </summary>
        /// <returns>The time.</returns>
        /// <param name="ppkey">Ppkey.</param>
        public static DateTime GetTime(string ppkey, DateTime defaultTime)
        {
            string storedTime = PlayerPrefs.GetString(ppkey, string.Empty);

            if (!string.IsNullOrEmpty(storedTime))
                return DateTime.FromBinary(Convert.ToInt64(storedTime));
            else
                return defaultTime;
        }

        /// <summary>
        /// Increment the overall played games amount by 1
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public int UpdatePlayedGame(int amount)
        {
            var played = PlayerPrefs.GetInt(PLAYED, 0);
            played++;
            PlayerPrefs.SetInt(PLAYED, played);

            FirebaseAnalyticsManager.SendPlayedAmountEvent(played);
            FirebaseAnalyticsManager.SetProperty("played", played.ToString());

            return played;
        }

        /// <summary>
        /// Returns the overall played games amount
        /// </summary>
        /// <returns></returns>
        public static int PlayedGameAmount()
        {
            return PlayerPrefs.GetInt(PLAYED, 0);
        }

        /// <summary>
        /// Load image url into image component
        /// </summary>
        /// <param name="MediaUrl">Image url to load</param>
        /// <param name="image">Image component to display the image</param>
        /// <returns></returns>
        public IEnumerator LoadImage(string MediaUrl, Image image)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);
            else
                image.canvasRenderer.SetTexture(((DownloadHandlerTexture)request.downloadHandler).texture);
        }

        /// <summary>
        /// Load image texture into image component
        /// </summary>
        /// <param name="MediaUrl">Image url to load</param>
        /// <param name="image">Image component to display the image</param>
        /// <returns></returns>
        public void LoadImage(Texture2D placeholderImage, Image image)
        {
            image.canvasRenderer.SetTexture(placeholderImage);
        }
    }
}