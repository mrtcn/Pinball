using TMPro;
using UnityEngine;

namespace Assets._Pinball.Scripts
{
    public class ScoreAdded: MonoBehaviour
    {
        public Animator scoreToAddAnimator;
        private ScoreSO score;
        private TextMeshProUGUI scoreToAddText;
        private readonly object syncObject = new object();

        private void Start()
        {
            score = ScriptableObject.FindObjectOfType<ScoreSO>() ?? ScriptableObject.CreateInstance<ScoreSO>();
            score.OnScoreUpdate += ScoreUpdated;
            scoreToAddText = gameObject.GetComponent<TextMeshProUGUI>();
        }

        private void ScoreUpdated(int scoreToAdd)
        {
            lock (syncObject)
            {
                scoreToAddText.text = $"+{scoreToAdd}";
                scoreToAddAnimator.SetTrigger("ScoreAdded");
            }
        }

        public void OnScoreToAddDone()
        {            
            scoreToAddText.text = string.Empty;
        }
        private void OnDestroy()
        {
            score.OnScoreUpdate -= ScoreUpdated;
        }
    }
}
