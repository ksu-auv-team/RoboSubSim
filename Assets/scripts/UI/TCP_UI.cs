using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;

public class TCP_UI : MonoBehaviour
{
    public TCPServer tcp_script;
    public TMPro.TMP_InputField IPaddr;
    public TMPro.TMP_InputField Port;
    public Toggle runServer;

    // Start is called before the first frame update
    void Start()
    {
        //print(IPaddr.text);
        //print(Port.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void configTCPScript(){
        print("UI TOGGLE");
        tcp_script.IPAddr = IPaddr.text;
        tcp_script.port = int.Parse(Port.text);
        tcp_script.runServer = runServer.isOn;
    }
}
