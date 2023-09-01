using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUiHandler : MonoBehaviour
{

    public MainManager mainManager;
    // Start is called before the first frame update
    void Start()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>().Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void start()
    {
        if (mainManager.GetPlayerName())
        {
            SceneManager.LoadScene(1);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
     EditorApplication.ExitPlaymode();
#else
     Application.Quit();
#endif

    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
