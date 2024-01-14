using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Robot_UI : MonoBehaviour
{
    //private GameObject Robot;
    SceneManagement sceneManagement;
    //TCPServer tcp_script;
    //RobotForce control_script;
    //BuoyancyForces buoyancy_script;
    //RobotCamera camera_script;
    //RobotIMU imu_script;
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
    
    //public void setNewRobot(){
    //    if (Robot == null){
    //        Robot = GameObject.FindGameObjectWithTag("Robot");
    //        if (Robot == null) {
    //            return;
    //        }
    //    }
    //    
    //    //tcp_script = Robot.GetComponent<TCPServer>();
    //    control_script = Robot.GetComponent<RobotForce>();
    //    buoyancy_script = Robot.GetComponent<BuoyancyForces>();
    //    camera_script = Robot.GetComponent<RobotCamera>();
    //    imu_script = Robot.GetComponent<RobotIMU>();
    //}
    void Start(){
        refresh();
    }
    public void refresh(){
        sceneManagement = GameObject.FindGameObjectWithTag("SceneManagement").GetComponent<SceneManagement>();
        //setNewRobot();
        //
        if (sceneManagement == null) {
            print("No Scene Management Found");
            return;
        }
        sceneManagement.registerAllSceneObjects();
        sceneManagement.displayAllRegisteredObjectsNames();
        configTCPScript();
        changeTCPMessage();
        if (sceneManagement.getRobotCount() > 0){
            configRobotParams();
            configControlMode();
            configCamera();
        }
    }
    public void changeTCPMessage(){
        TCPMessage.text = sceneManagement.tcpServer.ui_message;
    }
    public void configTCPScript(){
        //print("UI TOGGLE");
        sceneManagement.setupTCPServer(IPaddr.text,
                                        int.Parse(Port.text),
                                        runServer.isOn,
                                        int.Parse(SendFreq.text));
        //tcp_script.IPAddr = IPaddr.text;
        //tcp_script.port = int.Parse(Port.text);
        //tcp_script.runServer = runServer.isOn;
        //tcp_script.msPerTransmit = int.Parse(SendFreq.text);
    }
    public void configControlMode(){
        sceneManagement.configRobotControlMode(controlModeDropdown.value);
        //control_script.controlMethod = (RobotForce.controlMode)controlModeDropdown.value;
    }
    public void configRobotParams(){
        sceneManagement.configRobotParams(float.Parse(Mass.text), float.Parse(Volume.text));
    }
    public void captureImage(){
        sceneManagement.captureImage(cameraModeDropdown.value);
        //camera_script.generateData = true;
        //camera_script.configCommand(cameraModeDropdown.value);
        //camera_script.CommandTrigger(cameraModeDropdown.value);
        //camera_script.renderState = RobotCamera.renderStatesEnum.PreRender;
    }
    public void configCamera(){
        sceneManagement.configRobotCamera(int.Parse(ImageHeight.text), int.Parse(ImageWidth.text), cameraModeDropdown.value);
        //return;
        //if (camera_script.imgHeight != int.Parse(ImageHeight.text) || camera_script.imgWidth != int.Parse(ImageWidth.text)){
        //    camera_script.imgHeight = int.Parse(ImageHeight.text);
        //    camera_script.imgWidth = int.Parse(ImageWidth.text);
        //    
        //    // check current tcp server (kill and re-enable on new robot)
        //    bool hasServer = sceneManagement.tcpServer.runServer;
        //    if (hasServer) {runServer.isOn = false;}
        //    
        //    // create a new robot with proper perception camera resolutions
        //    var tempRobot = Instantiate(Robot, Robot.transform.position, Robot.transform.rotation);
        //    Destroy(Robot);
        //    Robot = tempRobot;
        //    setNewRobot();
        //    if (hasServer) {runServer.isOn = true;}
            // copy settings to new robot
             
        //}
        //camera_script.configCommand(cameraModeDropdown.value);
    }
    
    /// <summary>
    /// Debug Section
    /// </summary>
    public TMPro.TMP_Dropdown poolModeDropdown;
    public void configPool(){
        sceneManagement.sceneSelect = poolModeDropdown.value;
        sceneManagement.ResetScene();
    }
    public void randomizePoolWaterColor(){
        sceneManagement.poolColorRandom();
    }
    public Slider brightnessSlider;
    public Slider bluenessSlider;
    public Slider greennessSlider;
    public void setPoolWaterColor(){
        sceneManagement.setPoolWaterColor(  (int)(bluenessSlider.value * 255),
                                            (int)(greennessSlider.value * 255),
                                            brightnessSlider.value);
    }
    public Toggle physicsOnToggle;
    public void toggleWorldPhysics(){
        sceneManagement.togglePhysics(physicsOnToggle.isOn);
    }
    // Update is called once per frame
    void Update()
    {
        if (sceneManagement != null){
            changeTCPMessage();
        }
    }
}
