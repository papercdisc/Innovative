using UnityEngine;

public class MoveToScene : MonoBehaviour
{
    public void MoveToDesignatedScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}