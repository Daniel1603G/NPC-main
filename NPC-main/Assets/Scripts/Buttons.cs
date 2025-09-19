using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string level1SceneName = "Nivel1";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();
    }

    public void GoToLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(level1SceneName);

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void QuitGame()
    {
        Application.Quit(); 
    }
}
