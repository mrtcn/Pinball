using TMPro;
using UnityEngine;

namespace Assets._Pinball.Scripts
{
    public class ScoreChange: MonoBehaviour
    {
        public Animator scoreAnimator;
        private ScoreSO score;
        private TextMeshProUGUI scoreText;
        private int scoreToIncrement;
        private readonly object syncObject = new object();

        private void Start()
        {
            score = ScriptableObject.FindObjectOfType<ScoreSO>() ?? ScriptableObject.CreateInstance<ScoreSO>();
            score.OnScoreUpdate += ScoreUpdated;
            scoreText = gameObject.GetComponent<TextMeshProUGUI>();
        }

        private void ScoreUpdated(int scoreToAdd)
        {
            scoreToIncrement = scoreToAdd;                
            scoreAnimator.SetTrigger("Change");
        }

        public void ScoreCounter()
        {
            lock(syncObject)
            {
                if (int.TryParse(scoreText.text, out int intScore))
                {
                    scoreText.text = (intScore + scoreToIncrement).ToString();
                }
            }
        }

        private void OnDestroy()
        {
            score.OnScoreUpdate -= ScoreUpdated;
        }
    }
}
