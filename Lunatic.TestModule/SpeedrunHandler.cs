using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lunatic.TestModule;

public class SpeedrunHandler : MonoBehaviour
{
    public static string SceneName = "MainMenu";


    public void Start()
    {
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void Update()
    {
        if (UIExtensions.speedrunToggleOn && Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.LeftControl))
        {
            SceneManager.LoadScene(SceneNameKeys.MAIN_MENU_SCENE);
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneName = scene.name;
    }
}