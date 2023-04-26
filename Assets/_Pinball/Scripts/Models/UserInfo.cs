
using System;

namespace Assets._Pinball.Scripts.Models
{
    [Serializable]
    public class UserInfo
    {
        public UserInfo(string name, string username, string imagePath, AuthenticationType authenticationType, string externalProviderId)
        {
            Name = name;
            Username = username;
            ImagePath = imagePath;
            AuthenticationType = authenticationType;
            ExternalProviderId = externalProviderId;
        }

        public string Name;
        public string Username;
        public string ImagePath;
        public AuthenticationType AuthenticationType;
        public string ExternalProviderId;
    }
}
