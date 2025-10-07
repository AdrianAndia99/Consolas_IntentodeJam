using UnityEngine;
using UnityEngine.SceneManagement;
public class UImenu : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    [SerializeField] private string menuSceneName = "MenuPrincipal";
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
                                        Application.Quit();
#endif
    }
    public void BackToMenu()
    {
        // Si pausaste el juego al finalizar, reanuda antes de ir al menú
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        LoadScene(menuSceneName); // Usa el método que ya tienes:contentReference[oaicite:6]{index=6}
    }
}