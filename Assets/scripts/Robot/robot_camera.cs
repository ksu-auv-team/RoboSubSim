using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
public class robot_camera : MonoBehaviour
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
    private string frontCamSavePath;
    private string downCamSavePath;
    private enum renderStatesEnum : int{
        Off = 0,
        PreRender = 1,
        Rendering = 2,
        Rendered = 3
    };
    private renderStatesEnum renderState;
    private int currentFrames;
    void Start()
    {
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //CaptureAll();
            renderState = renderStatesEnum.PreRender;
            
        }
        //Debug.Log(renderState);
        //int cam_count = downCamera.allCameras.Length;
        //Debug.Log(cam_count);
        switch (renderState){
            case renderStatesEnum.Off:
                //MainCameraEnable();
                break;
            case renderStatesEnum.PreRender:
                if (currentFrames >= renderFramesCount){
                    currentFrames = 0;
                    renderState = renderStatesEnum.Rendering;
                }
                RenderCameraEnable();
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
                CaptureAll();
                RenderCameraDisable();
                currentFrames = 0;
                renderState = renderStatesEnum.Off;
                break;
        }
        
    }
    private void RenderCameraDisable(){
        frontCamera.enabled = false;
        downCamera.enabled = false;
        #if (WINDOWS)
        frontPerceptionCamera.enabled = false; // idk why ubuntu has fps issue with this, might be Vulkan stuff
        downPerceptionCamera.enabled = false;
        #endif
        //frontPerceptionCameraScript.enabled = false;
        //downPerceptionCameraScript.enabled = false;
        mainCamera.enabled = true;
        //mainCamera.depth = Camera.main.depth + 1;
        //Debug.Log("Depth" + Camera.main.depth + " " + mainCamera.depth);
    }
    private void RenderCameraEnable(){
        frontCamera.enabled = true;
        downCamera.enabled = true;
        //frontPerceptionCameraScript.enabled = true;
        //downPerceptionCameraScript.enabled = true;
        frontPerceptionCamera.enabled = true;
        downPerceptionCamera.enabled = true;
        
        mainCamera.enabled = true;
    }
    private void CaptureAll(){
        frontCounter = Capture(frontCamera, frontCamSavePath, frontCounter);
        downCounter = Capture(downCamera, downCamSavePath, downCounter);
        frontPerceptionCameraScript.RequestCapture();
        downPerceptionCameraScript.RequestCapture();
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
    private float rotation_gui = 180f;
    
    void OnGUI(){
        //GUIUtility.RotateAroundPivot(rotation_gui, new Vector2(mainCamera.pixelWidth/2, mainCamera.pixelHeight/2));
        //GUIUtility.ScaleAroundPivot(new Vector2(1, -1), new Vector2(mainCamera.pixelWidth/2, mainCamera.pixelHeight/2));
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
