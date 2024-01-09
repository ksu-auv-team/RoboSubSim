
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Simple_UI : MonoBehaviour
{
    public Robot_UI complex_ui_script;
    public TMPro.TMP_Text FPS;
    SceneManagement sceneManagement;
    void Start(){
        sceneManagement = GameObject.FindGameObjectWithTag("SceneManagement").GetComponent<SceneManagement>();
    }
    public void ResetScene(){
        sceneManagement.sceneRefresh = true;
    }
    void Update(){
        FPS.text = "FPS: " + (1 / Time.smoothDeltaTime).ToString("0.0");
    }
}