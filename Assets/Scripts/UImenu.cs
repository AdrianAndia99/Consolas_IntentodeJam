using UnityEngine;
using UnityEngine.SceneManagement;
public class UImenu : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}