using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void GoToMainScene()
    {
        SceneManager.LoadScene("main");
    }
    public void GoToMenuScene()
    {
        SceneManager.LoadScene("menu");
    }

    public static void GoToVictoryScene1() {
        SceneManager.LoadScene("victory1");
    }

    public static void GoToVictoryScene2() {
        SceneManager.LoadScene("victory2");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
