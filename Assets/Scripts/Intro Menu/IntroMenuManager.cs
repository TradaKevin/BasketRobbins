using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMenuManager : MonoBehaviour
{
    public GameObject OptionMenu;
    public void OnClickPlay()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OnClickOptions()
    {
        OptionMenu.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }



    public void OnClickBackOptions()
    {
        OptionMenu.SetActive(false);
    }

}
