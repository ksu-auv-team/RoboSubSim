using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
public class RobotCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera mainCamera;
    public Camera frontCamera;
    public Camera downCamera;
    public GameObject frontPerceptionCameraObject;
    public GameObject downPerceptionCameraObject;
    private PerceptionCamera frontPerceptionCameraScript;
    private PerceptionCamera downPerceptionCameraScript;
    private Camera frontPerceptionCamera;
    private Camera downPerceptionCamera;
    private int frontCounter;
    private int downCounter;
    public int renderFramesCount = 1;
    public bool generateData = true;
    public int imgWidth = 640;
    public int imgHeight = 480;
    public string frontCamFolderName = "FrontCam";
    public string downCamFolderName = "DownCam";
    public bool ShowGUI = false;
    private string frontCamSavePath;
    private string downCamSavePath;
    private enum renderStatesEnum : int{
        Off = 0,
        PreRender = 1,
        Rendering = 2,
        Rendered = 3
    };
    private enum CamCommandsID : int{
        front_no_percept = 0,   // front RGB
        down_no_percept = 1,    // down RGB
        both_no_percept = 2,    // RGB for both
        both_percept_only = 3,  // Segmentation for both
        all = 4                 // all four
    }
    private renderStatesEnum renderState;
    private CamCommandsID currentCommand = CamCommandsID.all;
    private int currentFrames;
    void Start()
    {
        if (SystemInfo.graphicsDeviceID == 0) {
            print("Running in Server Mode, Cannot Render Images");
            return;
        }
        //mainCamera = Camera.main;
        //mainCamera.targetTexture = new RenderTexture(imgWidth, imgHeight, 24);
        //mainCamera.targetDisplay = 1;
        if (generateData){
            //
            if (frontCamera.targetTexture == null){
                frontCamera.targetTexture = new RenderTexture(imgWidth, imgHeight, 24);
            }
            if (downCamera.targetTexture == null){
                downCamera.targetTexture = new RenderTexture(imgWidth, imgHeight, 24);
            }
            frontCamSavePath = Application.persistentDataPath+"/"+frontCamFolderName;
            downCamSavePath = Application.persistentDataPath+"/"+downCamFolderName;
            System.IO.Directory.CreateDirectory(frontCamSavePath);
            System.IO.Directory.CreateDirectory(downCamSavePath);

            frontPerceptionCameraScript = frontPerceptionCameraObject.GetComponent<PerceptionCamera>();
            frontPerceptionCamera = frontPerceptionCameraObject.GetComponent<Camera>();
            frontPerceptionCamera.targetTexture = new RenderTexture(imgWidth, imgHeight, 24);

            downPerceptionCameraScript = downPerceptionCameraObject.GetComponent<PerceptionCamera>();
            downPerceptionCamera = downPerceptionCameraObject.GetComponent<Camera>();
            downPerceptionCamera.targetTexture = new RenderTexture(imgWidth, imgHeight, 24);
        }
        //MainCameraEnable();
        renderState = renderStatesEnum.Off;
    }

    // Update is called once per frame
    void Update()
    {
        if (SystemInfo.graphicsDeviceID == 0) {
            
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            renderState = renderStatesEnum.PreRender;            
        }
        switch (renderState){
            case renderStatesEnum.Off:
                //MainCameraEnable();
                break;
            case renderStatesEnum.PreRender:
                if (currentFrames >= renderFramesCount){
                    currentFrames = 0;
                    renderState = renderStatesEnum.Rendering;
                }
                CommandEnable();
                currentFrames += 1;
                break;
            case renderStatesEnum.Rendering:
                if (currentFrames >= renderFramesCount) {
                    renderState = renderStatesEnum.Rendered;
                    currentFrames = 0;
                }
                currentFrames += 1;
                break;
            case renderStatesEnum.Rendered:
                CommandCapture();
                CommandDisable();
                currentFrames = 0;
                renderState = renderStatesEnum.Off;
                break;
        }
        
    }
    public void CommandTrigger(int command){
        currentCommand = (CamCommandsID)command;
        renderState = renderStatesEnum.PreRender;
    }
    private void CommandDisable(){
        frontCamera.enabled = false;
        downCamera.enabled = false;
        #if WINDOWS
        frontPerceptionCamera.enabled = false; // idk why ubuntu has fps issue with this, might be Vulkan stuff
        downPerceptionCamera.enabled = false;
        #endif
        mainCamera.enabled = true;
    }
    private void CommandEnable(){
        mainCamera.enabled = true;
        switch (currentCommand){
            case CamCommandsID.front_no_percept:
                frontCamera.enabled = true;
                break;
            case CamCommandsID.down_no_percept:
                downCamera.enabled = true;
                break;
            case CamCommandsID.both_no_percept:
                frontCamera.enabled = true;
                downCamera.enabled = true;
                break;
            case CamCommandsID.both_percept_only:
                frontPerceptionCamera.enabled = true;
                downPerceptionCamera.enabled = true;
                break;
            case CamCommandsID.all:
                frontCamera.enabled = true;
                downCamera.enabled = true;
                frontPerceptionCamera.enabled = true;
                downPerceptionCamera.enabled = true;
                break;
        }
    }
    private void CommandCapture(){
        switch (currentCommand) {
            case CamCommandsID.front_no_percept:
                frontCounter = Capture(frontCamera, frontCamSavePath, frontCounter);
                break;
            case CamCommandsID.down_no_percept:
                downCounter = Capture(downCamera, downCamSavePath, downCounter);
                break;
            case CamCommandsID.both_no_percept:
                frontCounter = Capture(frontCamera, frontCamSavePath, frontCounter);
                downCounter = Capture(downCamera, downCamSavePath, downCounter);
                break;
            
            case CamCommandsID.all:
                frontCounter = Capture(frontCamera, frontCamSavePath, frontCounter);
                downCounter = Capture(downCamera, downCamSavePath, downCounter);
                goto case CamCommandsID.both_percept_only;
                //frontPerceptionCameraScript.RequestCapture();
                //downPerceptionCameraScript.RequestCapture();
                //break;
            case CamCommandsID.both_percept_only:
                #if (UNITY_STANDALONE_LINUX)
                    print("There is some sort of bug with perception package in Linux Compiled, use editor in linux");
                    break;
                #endif
                frontPerceptionCameraScript.RequestCapture();
                downPerceptionCameraScript.RequestCapture();
                break;
        }
    }
    
    private int Capture(Camera cam, string save_path, int id){
        RenderTexture activeRT = RenderTexture.active;
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        cam.Render();
        RenderTexture.active = cam.targetTexture;
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0,0);
        image.Apply();
        RenderTexture.active = activeRT;
        byte[] data = image.EncodeToPNG();
        Destroy(image);
        Debug.Log(save_path + "/" + id + ".png");
        File.WriteAllBytes(save_path + "/" + id + ".png", data);
        return id + 1;
    }
    private void Down_Capture(){
        RenderTexture activeRT = RenderTexture.active;
        Texture2D image = new Texture2D(downCamera.targetTexture.width, downCamera.targetTexture.height);
        downCamera.Render();
        RenderTexture.active = downCamera.targetTexture;
        image.ReadPixels(new Rect(0, 0, downCamera.targetTexture.width, downCamera.targetTexture.height), 0,0);
        image.Apply();
        RenderTexture.active = activeRT;
        byte[] data = image.EncodeToPNG();
        Destroy(image);
        string save_path = Application.persistentDataPath + "/DownCam/down_"+downCounter+".png";
        Debug.Log(save_path);
        if (generateData){
            File.WriteAllBytes(save_path, data);
            downCounter += 1;
        }
    }
    private void Front_Capture(){
        RenderTexture activeRT = RenderTexture.active;
        Texture2D image = new Texture2D(frontCamera.targetTexture.width, frontCamera.targetTexture.height);
        frontCamera.Render();
        RenderTexture.active = frontCamera.targetTexture;
        image.ReadPixels(new Rect(0, 0, frontCamera.targetTexture.width, frontCamera.targetTexture.height), 0,0);
        image.Apply();
        RenderTexture.active = activeRT;
        byte[] data = image.EncodeToPNG();
        Destroy(image);
        string save_path = Application.persistentDataPath + "/FrontCam/front_"+frontCounter+".png";
        Debug.Log(save_path);
        if (generateData){
            File.WriteAllBytes(save_path, data);
            frontCounter += 1;
        }
    }

    //private void Capture(){
    //    Front_Capture();
    //    Down_Capture();
    //}    
    void OnGUI(){
        if (ShowGUI) {
            var button_rect = new Rect(mainCamera.pixelWidth/20, mainCamera.pixelHeight/20, mainCamera.pixelWidth/4, 40);
            if (GUI.Button(button_rect, "Space to capture, fps:" + (int)(1.0f / Time.smoothDeltaTime))){
                renderState = renderStatesEnum.PreRender;
            }
            //Debug.Log(GUI.transform);
            //Debug.Log(Camera.main.name);
            GUI.Label(new Rect(mainCamera.pixelWidth/20, mainCamera.pixelHeight-50, mainCamera.pixelWidth/2, 40), "Save to\n" + Application.persistentDataPath);
            //var main_cam_display = new Rect(0, 0, mainCamera.pixelWidth, mainCamera.pixelHeight);
            //GUI.DrawTexture(main_cam_display, mainCamera.targetTexture, ScaleMode.ScaleToFit);
            var front_cam_display = new Rect(2*mainCamera.pixelWidth/4, 3*mainCamera.pixelHeight/4, mainCamera.pixelWidth/4, mainCamera.pixelHeight/4);
            GUI.DrawTexture(front_cam_display, frontCamera.targetTexture, ScaleMode.ScaleToFit);
            var down_cam_display = new Rect(3*mainCamera.pixelWidth/4, 3*mainCamera.pixelHeight/4, mainCamera.pixelWidth/4, mainCamera.pixelHeight/4);
            GUI.DrawTexture(down_cam_display, downCamera.targetTexture,  ScaleMode.ScaleToFit);
        }
    }
    
}
