using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Robot_UI : MonoBehaviour
{
    public GameObject Robot;
    TCPServer tcp_script;
    RobotForce control_script;
    public TMPro.TMP_InputField IPaddr;
    public TMPro.TMP_InputField Port;
    public Toggle runServer;
    public TMPro.TMP_Dropdown controlModeDropdown;
    void Start(){
        refresh();
    }
    public void refresh(){
        tcp_script = Robot.GetComponent<TCPServer>();
        control_script = Robot.GetComponent<RobotForce>();
    }
    public void configTCPScript(){
        print("UI TOGGLE");
        tcp_script.IPAddr = IPaddr.text;
        tcp_script.port = int.Parse(Port.text);
        tcp_script.runServer = runServer.isOn;
    }
    public void configControlMode(){
        //switch (controlModeDropdown.value){
        //    case (int)RobotForce.controlMode.Raw:
        //        control_script.controlMethod = RobotForce.controlMode.Raw;
        //        break;
        //    case (int)RobotForce.controlMode.Local:
        //        control_script.controlMethod = RobotForce.controlMode.Local;
        //        break;
        //    case (int)RobotForce.controlMode.Global:
        //        control_script.controlMethod = RobotForce.controlMode.Global;
        //        break;
        //    case (int)RobotForce.controlMode.bodyVelocityFixedHeading:
        //        control_script.controlMethod = RobotForce.controlMode.bodyVelocityFixedHeading;
        //        break;
        //}
        control_script.controlMethod = (RobotForce.controlMode)controlModeDropdown.value;
    }
    // Update is called once per frame
    void Update()
    {
        
        
    }
}
