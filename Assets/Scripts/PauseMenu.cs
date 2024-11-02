using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPaused = false;

    private void Awake()
    {
        // Ensure this object persists between scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Activate the pause menu
            pauseMenu.SetActive(true);
            // Pause the game
            Time.timeScale = 0f;
        }
        else
        {
            // Deactivate the pause menu
            pauseMenu.SetActive(false);
            // Resume the game
            Time.timeScale = 1f;
        }
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    public void QuitGame()
    {
        // Handle quitting the game
        Application.Quit();
        // If running in the editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
