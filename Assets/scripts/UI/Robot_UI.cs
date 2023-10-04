using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Robot_UI : MonoBehaviour
{
    public GameObject Robot;
    TCPServer tcp_script;
    RobotForce control_script;
    BuoyancyForces buoyancy_script;
    RobotCamera camera_script;
    RobotIMU imu_script;
    public TMPro.TMP_InputField IPaddr;
    public TMPro.TMP_InputField Port;
    public TMPro.TMP_InputField SendFreq;
    public TMPro.TMP_Text TCPMessage;
    public Toggle runServer;
    public TMPro.TMP_Dropdown controlModeDropdown;
    public TMPro.TMP_InputField Mass;
    public TMPro.TMP_InputField Volume;
    public TMPro.TMP_InputField ImageWidth;
    public TMPro.TMP_InputField ImageHeight;
    public TMPro.TMP_Dropdown cameraModeDropdown;
    void Start(){
        refresh();
    }
    public void refresh(){
        tcp_script = Robot.GetComponent<TCPServer>();
        control_script = Robot.GetComponent<RobotForce>();
        buoyancy_script = Robot.GetComponent<BuoyancyForces>();
        camera_script = Robot.GetComponent<RobotCamera>();
        imu_script = Robot.GetComponent<RobotIMU>();

        configTCPScript();
        //configRobotParams();
        configRobotParams();
        configCamera();
        changeTCPMessage();
    }
    public void changeTCPMessage(){
        TCPMessage.text = tcp_script.ui_message;
    }
    public void configTCPScript(){
        print("UI TOGGLE");
        tcp_script.IPAddr = IPaddr.text;
        tcp_script.port = int.Parse(Port.text);
        tcp_script.runServer = runServer.isOn;
        tcp_script.msPerTransmit = int.Parse(SendFreq.text);
    }
    public void configControlMode(){
        control_script.controlMethod = (RobotForce.controlMode)controlModeDropdown.value;
    }
    public void configRobotParams(){
        control_script.m_rigidBody.mass = float.Parse(Mass.text);
        buoyancy_script.volumeDisplaced = float.Parse(Volume.text);
    }
    public void captureImage(){
        camera_script.generateData = true;
        camera_script.renderState = RobotCamera.renderStatesEnum.PreRender;
    }
    public void configCamera(){
        //
        if (camera_script.imgHeight != int.Parse(ImageHeight.text) || camera_script.imgWidth != int.Parse(ImageWidth.text)){
            camera_script.imgHeight = int.Parse(ImageHeight.text);
            camera_script.imgWidth = int.Parse(ImageWidth.text);
            camera_script.resetCameraTexture();
        }
        camera_script.currentCommand = (RobotCamera.CamCommandsID)cameraModeDropdown.value;
    }
    // Update is called once per frame
    void Update()
    {
        
        changeTCPMessage();
    }
}
