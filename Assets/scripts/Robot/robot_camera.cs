using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class robot_camera : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera mainCamera;
    public Camera frontCamera;
    public Camera downCamera;
    private int frontCounter;
    private int downCounter;
    public bool generateData = true;
    public int imgWidth = 640;
    public int imgHeight = 480;
    public string frontCamFolderName = "FrontCam";
    public string downCamFolderName = "DownCam";
    private string frontCamSavePath;
    private string downCamSavePath;
    void Start()
    {
        //mainCamera = Camera.main;
        //mainCamera.targetDisplay = 1;
        if (generateData){
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaptureAll();
        }
    }
    private void CaptureAll(){
        frontCounter = Capture(frontCamera, frontCamSavePath, frontCounter);
        downCounter = Capture(downCamera, downCamSavePath, downCounter);
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
        var button_rect = new Rect(mainCamera.pixelWidth/20, mainCamera.pixelHeight/20, mainCamera.pixelWidth/4, 40);
        if (GUI.Button(button_rect, "Capture")){
            CaptureAll();
        }
        GUI.Label(new Rect(mainCamera.pixelWidth/20, mainCamera.pixelHeight-50, mainCamera.pixelWidth/2, 40), "Save to\n" + Application.persistentDataPath);
        var front_cam_display = new Rect(2*mainCamera.pixelWidth/4, 3*mainCamera.pixelHeight/4, mainCamera.pixelWidth/4, mainCamera.pixelHeight/4);
        GUI.DrawTexture(front_cam_display, frontCamera.targetTexture, ScaleMode.ScaleToFit);
        var down_cam_display = new Rect(3*mainCamera.pixelWidth/4, 3*mainCamera.pixelHeight/4, mainCamera.pixelWidth/4, mainCamera.pixelHeight/4);
        GUI.DrawTexture(down_cam_display, downCamera.targetTexture,  ScaleMode.ScaleToFit);
    }
}
