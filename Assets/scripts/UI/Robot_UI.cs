using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Robot_UI : MonoBehaviour
{
    public GameObject Robot;
    TCPServer tcp_script;
    public TMPro.TMP_InputField IPaddr;
    public TMPro.TMP_InputField Port;
    public Toggle runServer;
    void Start(){
        refresh();
    }
    public void refresh(){
        tcp_script = Robot.GetComponent<TCPServer>();
    }
    public void configTCPScript(){
        print("UI TOGGLE");
        tcp_script.IPAddr = IPaddr.text;
        tcp_script.port = int.Parse(Port.text);
        tcp_script.runServer = runServer.isOn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
