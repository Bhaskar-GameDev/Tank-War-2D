using UnityEngine;
using UnityEngine.SceneManagement;

public class GameObjectActivator : MonoBehaviour
{
    public GameObject objectToDeactivate;

    void Start()
    {
        if (objectToDeactivate != null && SceneManager.GetActiveScene().name != "Game")
        {
            objectToDeactivate.SetActive(false);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game" && objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(true);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
