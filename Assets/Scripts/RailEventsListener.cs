using UnityEngine;
using System;
using TMPro;

public class RailEventsListener : MonoBehaviour
{
    [Header("UI Final")]
    [SerializeField] private GameObject finishPanel;          // Panel a activar al terminar
    [SerializeField] private TextMeshProUGUI currentScoreText; // Texto para puntaje actual
    [SerializeField] private TextMeshProUGUI highScoreText;    // Texto para récord

    [Header("Opcional")]
    [SerializeField] private bool pauseOnFinish = true;        // Pausar juego al mostrar panel
    [SerializeField] private string highScoreKey = "HighScore";
    void OnEnable()
    {
        PlayerControl.OnReachedWaypoint += HandleReachedWaypoint;
        PlayerControl.OnPathCompleted += HandlePathCompleted;
    }

    void OnDisable()
    {
        PlayerControl.OnReachedWaypoint -= HandleReachedWaypoint;
        PlayerControl.OnPathCompleted -= HandlePathCompleted;
    }

    private void HandleReachedWaypoint(PlayerControl pc, int index)
    {
        Debug.Log($"Se alcanzó el waypoint {index} por {pc.name}.");
    }

    private void HandlePathCompleted(PlayerControl pc)
    {
        // 1) Leer puntaje actual del ScoreManager
        int current = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetScore() : 0;

        // 2) Leer/actualizar récord con PlayerPrefs
        int best = PlayerPrefs.GetInt(highScoreKey, 0);
        if (current > best)
        {
            best = current;
            PlayerPrefs.SetInt(highScoreKey, best);
            PlayerPrefs.Save();
        }

        // 3) Activar panel y mostrar textos
        if (finishPanel != null) finishPanel.SetActive(true);
        if (currentScoreText != null) currentScoreText.text = $"Puntuación: {current}";
        if (highScoreText != null) highScoreText.text = $"Récord: {best}";

        // 4) Pausar juego (opcional)
        if (pauseOnFinish) Time.timeScale = 0f;
    }
    private int GetCurrentScore()
    {
        // ScoreManager actualmente no expone el score, puedes agregar un getter público
        return ScoreManager.Instance.GetScore();
    }
}