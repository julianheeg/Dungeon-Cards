using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script that serves as an entry point for the program. It handles some initializations and then changes to the main menu
/// </summary>
public class Preload : MonoBehaviour {
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        CardDatabase.Init();
        SceneManager.LoadScene("MainMenu");
    }
}