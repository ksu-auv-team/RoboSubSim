using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles all objects (with matching tagNames) in the scene so no calls to individual script should be needed
/// Therefore it grabs all objects in the scene and has the ability to copy/remove objects (that exists) in the scene
/// </summary>
public class TCPRobot{
    public GameObject tcpObject;
    public RobotCamera cameraScript;
    public RobotForce controlScript;
    public RobotIMU imuScript;
    public BuoyancyForces buoyScript;
    public void setNewRobot(GameObject robot){
        tcpObject = robot;
        cameraScript = robot.GetComponent<RobotCamera>();
        controlScript = robot.GetComponent<RobotForce>();
        imuScript = robot.GetComponent<RobotIMU>();
        buoyScript = robot.GetComponent<BuoyancyForces>();
    }
}

[RequireComponent (typeof(TCPServer))]
public class SceneManagement : MonoBehaviour
{
    public Robot_UI ui_script;
    const int ROBOT = 0;
    string[] tagNames = {"Robot", "Pool", "2023Objective", "Environment"};
    List<GameObject[]> gameObjects = new List<GameObject[]>();
    List<TCPRobot> allRobots = new List<TCPRobot>();
    int tagSelect;
    int objectSelect;
    public TCPServer tcpServer;
    public TCPRobot tcpRobot;
    public bool tcpObjectChanged;
    public int tcpTagSelect;
    public int tcpObjectSelect;
    public string[] sceneLists = new string[] { "Scenes/LobbyScene",
                                                "Scenes/OutdoorsScene",
                                                "Scenes/OutdoorsScene"};
    public bool sceneRefresh = false;
    public int sceneSelect = 0;
    public IEnumerator ResetSceneCoroutine(){
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneLists[sceneSelect]);
        while(!asyncLoad.isDone){
            yield return null;
        }

        ui_script.refresh();
    }
    public void ResetScene(){
        StartCoroutine(ResetSceneCoroutine());
        sceneRefresh = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        setupTCPServer();
    }
    // Update is called once per frame
    void Update()
    {
        if (tcpObjectChanged){
            GameObject gameObject = selectObject(tagNames[tcpTagSelect], tcpObjectSelect);
            if (tcpTagSelect == ROBOT){
                tcpRobot.setNewRobot(gameObject);
            }
            tcpObjectChanged = false;
        }
        if (sceneRefresh) {
            ResetScene();
        }
    }
    /// <summary>
    /// TCP Server stuff
    /// </summary>
    void loadTCPServer(){
        tcpServer = GetComponent<TCPServer>();
    }
    public void setupTCPServer( string IPAddr = "127.0.0.1", 
                                int port = 1234, 
                                bool runServer = false, 
                                float msPerTransmit = 11.0f){
        loadTCPServer();
        tcpServer.IPAddr = IPAddr;
        //tcpServer.simCB_IPAddr = IPAddr;
        tcpServer.port = port;
        tcpServer.runServer = runServer;
        tcpServer.msPerTransmit = msPerTransmit;

        ui_script.IPaddr.text = IPAddr;
        ui_script.Port.text = port.ToString();
        ui_script.runServer.isOn = runServer;
        ui_script.SendFreq.text = msPerTransmit.ToString();
    }
    public void setupSimCBConnect(bool simCB_Connect){
        tcpServer.simCB_Connect = simCB_Connect;
        ui_script.runSimCB.isOn = simCB_Connect;
    }
    /// <summary>
    /// Robot Dynamics Configurations
    /// </summary>
    public void configRobotControlMode(int mode, int robotID = 0, bool tcp = false){
        configRobotControlMode((RobotForce.controlMode) mode, robotID, tcp);
    }
    public void configRobotControlMode(RobotForce.controlMode mode, int robotID = 0, bool tcp = false){
        GameObject robot = selectObject(ROBOT, robotID);
        RobotForce script = allRobots[robotID].controlScript;
        script.controlMethod = mode;

        ui_script.controlModeDropdown.value = (int)mode;
    }
    public void configRobotParams(float mass, float volume, int robotID = 0, bool tcp = false){
            //GameObject robot = selectObject(ROBOT, robotID);
        allRobots[robotID].controlScript.m_rigidBody.mass = mass;
        allRobots[robotID].buoyScript.volumeDisplaced = volume;
    
        ui_script.Mass.text = mass.ToString();
        ui_script.Volume.text = volume.ToString();
    }
    public void configRobotCamera(int height = -1, int width = -1, int mode = -1, int robotID = 0){
        GameObject robot = selectObject(ROBOT, robotID);
        RobotCamera script = allRobots[robotID].cameraScript;
        if (height > 0 && width > 0){
            if (script.imgHeight != height || script.imgWidth != width){
                script.imgHeight = height;
                script.imgWidth = width;

                // check current tcp server (kill and re-enable on new robot)
                //bool hasServer = tcpServer.runServer;
                //if (hasServer) {setupTCPServer(tcpServer.IPAddr, tcpServer.port, false, tcpServer.msPerTransmit);}

                // create a new robot with proper perception camera resolutions
                GameObject newrobot = copyNewObject(robot);
                Destroy(robot);
                replaceObjectInArray(newrobot, ROBOT, robotID);
                script = newrobot.GetComponent<RobotCamera>();
                //if (hasServer) {setupTCPServer(tcpServer.IPAddr, tcpServer.port, true, tcpServer.msPerTransmit);}
                // copy settings to new robot
            }
        }
        if (mode > 0){
            script.configCommand(mode);
        }
        ui_script.ImageHeight.text = script.imgHeight.ToString();
        ui_script.ImageWidth.text = script.imgWidth.ToString();
        ui_script.cameraModeDropdown.value = (int)script.currentCommand;
    }
    public int getCameraMode(int robotID = 0){
        GameObject robot = selectObject(ROBOT, robotID);
        RobotCamera script = allRobots[robotID].cameraScript;
        return (int)script.currentCommand;
    }
    /// <summary>
    /// Robot Actions
    /// </summary>
    public void captureImage(int mode, int robotID = 0){
        //GameObject robot = selectObject(ROBOT, robotID);
        RobotCamera script = allRobots[robotID].cameraScript;
        script.generateData = true;
        //camera_script.configCommand(cameraModeDropdown.value);
        script.CommandTrigger(mode);
        //camera_script.renderState = RobotCamera.renderStatesEnum.PreRender;
    }
    public void triggerCapture(int mode, int robotID = 0){
        //GameObject robot = selectObject(ROBOT, robotID);
        RobotCamera script = allRobots[robotID].cameraScript;
        script.CommandTrigger(mode);
    }
    public void setMotorPower(  float m1, float m2, float m3, float m4,
                                float m5, float m6, float m7, float m8,
                                int robotID = 0){
        //GameObject robot = selectObject(ROBOT, robotID);
        RobotForce script = allRobots[robotID].controlScript;
        script.thrust_strengths[0] = m1;
        script.thrust_strengths[1] = m2;
        script.thrust_strengths[2] = m3;
        script.thrust_strengths[3] = m4;
        script.thrust_strengths[4] = m5;
        script.thrust_strengths[5] = m6;
        script.thrust_strengths[6] = m7;
        script.thrust_strengths[7] = m8;
        //print(allRobots.Count);
        //print(script.controlMethod);
    }
    public void setOtherControlPower(   float m1, float m2, float m3, float m4,
                                        float m5, float m6, int robotID = 0){
        //GameObject robot = selectObject(ROBOT, robotID);
        RobotForce script = allRobots[robotID].controlScript;
        script.other_control[0] = m1;
        script.other_control[1] = m2;
        script.other_control[2] = m3;
        script.other_control[3] = m4;
        script.other_control[4] = m5;
        script.other_control[5] = m6;
    }
    public IMU getRobotIMU(int robotID = 0){
        //GameObject robot = selectObject(ROBOT, robotID);
        RobotIMU script = allRobots[robotID].imuScript;
        return script.imu;
    }
    
    /// <summary>
    /// Load and Debug Scene
    /// </summary>
    public void registerAllSceneObjects(){
        //int tagNum = 0;
        gameObjects.Clear();
        foreach (string tag in tagNames){
            gameObjects.Add(GameObject.FindGameObjectsWithTag(tag));
            if (System.String.Equals(tag, tagNames[ROBOT])) {
                allRobots.Clear();
                foreach (GameObject rob in GameObject.FindGameObjectsWithTag(tag)){
                    TCPRobot cur = new TCPRobot();
                    cur.setNewRobot(rob);
                    allRobots.Add(cur);
                }

            }
        }
    }
    public void displayAllRegisteredObjectsNames(){
        int tagNum = 0;
        foreach (GameObject[] objects in gameObjects){
            foreach(GameObject obj in objects){
                print(tagNames[tagNum] + ": " + obj);
            }
            tagNum+=1;
        }
    }
    public int getRobotCount(){
        return allRobots.Count;
    }
    /// <summary>
    /// Scene Object Utility
    /// </summary>
    GameObject copyNewObject(GameObject existingObject){
        GameObject newObject = Instantiate( existingObject, 
                                            existingObject.transform.position, 
                                            existingObject.transform.rotation);
        return newObject;
    }
    GameObject selectObject(int tagID, int objectID){
        
        if (tagID < gameObjects.Count && objectID < gameObjects[tagID].Length){
            tagSelect = tagID;
            objectSelect = objectID;
            return gameObjects[tagID][objectID];
        }
        return null;
    }
    GameObject selectObject(string tag, int objectID){
        int tagID = System.Array.IndexOf(tagNames, tag);
        if (tagID < 0) {return null;}
        return selectObject(tagID, objectID);
    }
    void replaceObjectInArray(GameObject newObject, int tagID, int objectID){
        gameObjects[tagID][objectID] = newObject;
    }
    void replaceObjectInArray(GameObject newObject, string tag, int objectID){
        int tagID = System.Array.IndexOf(tagNames, tag);
        replaceObjectInArray(newObject, tagID, objectID);
    }
    public void poolColorRandom(){
        var waterBodies = GameObject.FindGameObjectsWithTag("WaterColor");
        foreach(GameObject waterBody in waterBodies){
            waterBody.GetComponent<WaterRandomization>().RandomizeWater();
        }
    }
    public void setPoolWaterColor(int blue, int green, float brightness){
        var waterBodies = GameObject.FindGameObjectsWithTag("WaterColor");
        foreach(GameObject waterBody in waterBodies){
            waterBody.GetComponent<WaterRandomization>().SetWaterColor(blue, green, brightness);
        }
    }
    
    public void togglePhysics(bool On){
        Rigidbody[] allRigidBodies = GameObject.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach(Rigidbody rigidbody in allRigidBodies){
            rigidbody.isKinematic = On;
        }
    }
    public void setDisplayCamera(bool ShowGUI){
        allRobots[0].cameraScript.ShowGUI = ShowGUI;
    }
}
