
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Simple_UI : MonoBehaviour
{
    public Robot_UI complex_ui_script;
    public void ResetScene(){
        SceneManager.LoadScene("Scenes/OutdoorsScene");
        complex_ui_script.refresh();
    }
}