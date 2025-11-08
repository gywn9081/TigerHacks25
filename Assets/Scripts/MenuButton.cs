using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuButton : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif
    [SerializeField, HideInInspector] private string sceneToLoad;

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogWarning($"[MenuButton] No scene name assigned on {gameObject.name}");
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
            sceneToLoad = sceneAsset.name;
    }
#endif
}