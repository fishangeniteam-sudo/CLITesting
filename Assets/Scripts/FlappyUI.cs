using UnityEngine;
using UnityEngine.UI;

public class FlappyUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject ReadyPanel;
    public GameObject PlayingPanel;
    public GameObject GameOverPanel;

    [Header("Text Displays")]
    public Text ScoreText;
    public Text GameOverScoreText;
    public Text HighScoreText;

    [Header("Restart Button")]
    public Button RestartButton;

    private void Awake()
    {
        if (RestartButton != null)
        {
            RestartButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartGame();
                }
            });
        }
    }

    public void ShowReadyScreen()
    {
        if (ReadyPanel != null) ReadyPanel.SetActive(true);
        if (PlayingPanel != null) PlayingPanel.SetActive(false);
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
    }

    public void ShowPlayingScreen()
    {
        if (ReadyPanel != null) ReadyPanel.SetActive(false);
        if (PlayingPanel != null) PlayingPanel.SetActive(true);
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
    }

    public void ShowGameOverScreen(int currentScore, int highScore)
    {
        if (ReadyPanel != null) ReadyPanel.SetActive(false);
        if (PlayingPanel != null) PlayingPanel.SetActive(false);
        if (GameOverPanel != null) GameOverPanel.SetActive(true);

        if (GameOverScoreText != null)
        {
            GameOverScoreText.text = "SCORE: " + currentScore;
        }

        if (HighScoreText != null)
        {
            HighScoreText.text = "BEST: " + highScore;
        }
    }

    public void UpdateScore(int score)
    {
        if (ScoreText != null)
        {
            ScoreText.text = score.ToString();
        }
    }
}
