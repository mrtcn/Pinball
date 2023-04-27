using Assets._Pinball.Scripts.Models;
using Assets._Pinball.Scripts.Models.Enums;
using UnityEngine;

namespace Assets._Pinball.Scripts.Services
{
    public static class ExternalProviderStateHelper
    {
        public static ExternalProviderState LastLoginState(ExternalProviderLastLoginType type)
        {
            var lastLoginState = PlayerPrefs.GetInt(type.ToString(), 0);
            return (ExternalProviderState)lastLoginState;
        }

        public static void SetLastLoginState(ExternalProviderLastLoginType type, ExternalProviderState externalProviderState)
        {
            PlayerPrefs.SetInt(type.ToString(), (int)externalProviderState);
        }
    }
}
