using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }


    [SerializeField] private TextMeshProUGUI scoreText;
    private int score = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }
    public int GetScore()
    {
        return score;
    }
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            // La forma de actualizar el texto es la misma
            scoreText.text = "Puntaje: " + score;
        }
    }
}