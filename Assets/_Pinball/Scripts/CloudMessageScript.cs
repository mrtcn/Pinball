using UnityEditor;
using UnityEngine;

public class CloudMessageScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try 
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        }
        catch (System.Exception ex)
        {
            Firebase.Crashlytics.Crashlytics.LogException(ex);
            throw;
        }
    }

    // Update is called once per frame
    void Update()
    {

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
