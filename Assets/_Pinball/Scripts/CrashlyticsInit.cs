using UnityEngine;
// Import Firebase and Crashlytics
using Firebase;
using Firebase.Crashlytics;

public class CrashlyticsInit : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        // Initialize Firebase


        UnityEngine.Debug.Log("Firebase starting");
        Firebase.Crashlytics.Crashlytics.Log("Firebase starting");

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                try
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    // Crashlytics will use the DefaultInstance, as well;
                    // this ensures that Crashlytics is initialized.

                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    // When this property is set to true, Crashlytics will report all
                    // uncaught exceptions as fatal events. This is the recommended behavior.
                    Crashlytics.ReportUncaughtExceptionsAsFatal = true;

                    // Set a flag here for indicating that your project is ready to use Firebase.

                    Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError(System.String.Format("Firebase failed initializing: {0}", ex.Message));
                    Firebase.Crashlytics.Crashlytics.LogException(ex);
                    throw;
                }

            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
}
