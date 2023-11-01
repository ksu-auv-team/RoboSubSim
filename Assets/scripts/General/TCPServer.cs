using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public struct CommandPacket{
    public byte[] body;
    public int body_counter;
    public bool EscapeNextByte;
    public int bodyLen;
    private const byte ESCAPE = 255;
    private const byte START = 253;
    private const byte END = 254;
    public SceneManagement sceneManagement;
    public enum states : int {
        Waiting = 0,
        Reading = 1,
    }
    public states commandState;
    public CommandPacket(int length, SceneManagement sceneM){
        commandState = states.Waiting;
        bodyLen = length;
        body = new byte[bodyLen];
        body_counter = 0;
        EscapeNextByte = false;
        sceneManagement = sceneM;
    }
    public bool processByte(byte data){
        switch(commandState){
            case states.Reading:
                if (EscapeNextByte) {
                    body[body_counter] = data;
                    body_counter += 1;
                    EscapeNextByte = false;
                    break;
                }
                if (data == END) {return true;}
                else if (data == ESCAPE) {EscapeNextByte = true; break;}
                if (body_counter >= bodyLen){
                    return true;
                }
                body[body_counter] = data;
                body_counter += 1;
                EscapeNextByte = false;
                break;
            case states.Waiting:
                if (data == START){
                    commandState = states.Reading;
                }
                break;
            //case states.WaitProcess:
            //    processCommand();
            //    commandState = states.Processed;
            //    break;
        }
        return false;
    }
    public void processCommand(){
        
    }
    public void reset(){
        System.Array.Clear(body, 0, bodyLen);
        body_counter = 0;
        EscapeNextByte = false;
        commandState = states.Waiting;
    }
    public string ToString(){
        return System.Text.Encoding.Default.GetString(body);
    }
    float[] ParseFloats(byte[] byteValues, int numFloats, int startIndex){
        float[] floatValues = new float[numFloats];
        int count = 0;
        while (count < numFloats){
            floatValues[count] = System.BitConverter.ToSingle(byteValues, startIndex);
            startIndex += 4;
            count += 1;
        }
        return floatValues;
    }
}
public class TCPServer : MonoBehaviour
{
    List<string> possibleCommands = new List<string>{
            // mimic control board commands
            "RAW", "LOCAL", "BNO055P", "MS5837P", "BNO055R", "MS5837R",
            // Other commands
            "CAPTURE", "RESETS", "CAMCFG", "ROBSEL"
        };
    SceneManagement sceneManagement;
    public string IPAddr = "127.0.0.1";
    public int port = -1;
    //public int motorsPort = -1;
    //public int gyroPort = -1;
    //public int imageCommandsPort = -1;
    //public bool general = true;
    //bool running;
    public int msPerTransmit = 100;
    private float currentTime = 0;
    //private RobotIMU imu_script;
    //private RobotForce motor_script;
    //private RobotCamera camera_script;
    public string ui_message = "Closed";
    //private Thread threadMotors;
    //private Thread threadGyro;
    //private Thread threadImages;
    private Thread threadGeneral;
    //private PortsID portsID; 
    //[HideInInspector]
    public bool runServer = false;
    private bool serverStarted = false;
    private byte[] buffer;
    List<CommandPacket> commandsPool = new List<CommandPacket>(64);
    CommandPacket currentPacket;
    void Start()
    {
        sceneManagement = GetComponent<SceneManagement>();
        currentPacket = new CommandPacket(256, sceneManagement);
        buffer = new byte[10];
        //motor_script = GetComponent<RobotForce>();
        //imu_script = GetComponent<RobotIMU>();
        //camera_script = GetComponent<RobotCamera>();
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        threadGeneral = new Thread(() => GetData(port));
        //threadMotors = new Thread(() => GetData(motorsPort, PortsID.motors));
        //threadGyro = new Thread(() => GetData(gyroPort, PortsID.imu));
        //threadImages = new Thread(() => GetData(imageCommandsPort, PortsID.commands));


    }
    void Update(){
        currentTime += (int)(Time.deltaTime * 1000);
        if (currentTime > 1000000) {currentTime = 0;}
        if (!serverStarted && runServer) {
            startThread();
            serverStarted = true;
        }
        if (serverStarted && !runServer) {
            stopThread();
            serverStarted = false;
        }
        //print(runServer);
    }
    void OnDestroy(){
        //if (general){
            runServer = false;
            //threadGeneral.Abort();
        //}else{
        //    threadMotors.Abort();
        //    threadGyro.Abort();
        //    threadImages.Abort();
        //}
    }
    void OnApplicationQuit(){
        runServer = false;
    }
    void GetData(int port)
    {
        if (port < 0){
            return;
        }
        
        TcpListener server = new TcpListener(IPAddress.Parse(IPAddr), port);
        server.Start();
        ui_message = "Waiting... :"+IPAddr+":"+port;
        Debug.Log("Port:" + port + ", started on " + IPAddr + ". waiting connection...");
        while (!server.Pending()) {
            if (!runServer) {
                server.Stop();
                ui_message = "Closed:"+IPAddr+":"+port;
                Debug.Log("Port:" + port + ", closed on " + IPAddr);
                return;
            }
        }
        // Create a client to get the data stream
        TcpClient client = server.AcceptTcpClient();

        // TODO: FIND BETTER MESSAGE PROCESSING METHODS SO THIS NUMBER CAN BE AS LOW AS POSSIBLE
        client.ReceiveBufferSize = 8;  

        NetworkStream nwStream = client.GetStream();
        ui_message = "Connected:"+IPAddr+":"+port;
        Debug.Log("Port:"+port + ", stream connected");
        // Start listening
        bool running;
        while (runServer)
        {
            running = Connection(client, nwStream);
        }
        Debug.Log("Port:" + port + ", closed on " + IPAddr);
        server.Stop();
        ui_message = "Closed:"+IPAddr+":"+port;
        // Create the server
        
    }

    bool Connection(TcpClient client, NetworkStream nwStream)
    {
        if (currentTime > msPerTransmit){
            //Debug.Log(currentTime);
            //ParseSendData(nwStream);
            currentTime = 0;
        }
        // Read data from the network stream
        if (nwStream.DataAvailable){
            int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
            //print(bytesRead);
            ParseReceivedData(bytesRead);
        }
        else{
            if (commandsPool.Count > 0){
                commandsPool[commandsPool.Count-1].processCommand();
                commandsPool.RemoveAt(commandsPool.Count-1);
            }
            if (commandsPool.Count > 256) {
                print("TCP Command Pool Overflow");
                commandsPool.Clear();
            }
            print("Pooled Commands: " + commandsPool.Count);
        }
        
            //Debug.Log("No msg received");
        return true;
    }
    void ParseSendData(NetworkStream nwStream){
        byte[] imu_buf = new byte[26];
        System.Buffer.BlockCopy(System.BitConverter.GetBytes('I'), 0, imu_buf, 24, 2);
        IMU imu = sceneManagement.getRobotIMU();
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                imu.quaternion.eulerAngles.z), 0, imu_buf, 0, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                360-imu.quaternion.eulerAngles.x), 0, imu_buf, 4, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                360-imu.quaternion.eulerAngles.y), 0, imu_buf, 8, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                -imu.linearAccel.z), 0, imu_buf, 12, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                imu.linearAccel.x), 0, imu_buf, 16, 4);
        System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                imu.linearAccel.y), 0, imu_buf, 20, 4);
        nwStream.Write(imu_buf,0,imu_buf.Length);
                //Debug.Log(imu.quaternion.eulerAngles);
                //Debug.Log(System.BitConverter.IsLittleEndian);
    }
    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    void ParseReceivedData(int bytesRead)
    {
        int i = 0;
        while (i < bytesRead){
            if (currentPacket.processByte(buffer[i])){
                print(currentPacket.ToString());
                commandsPool.Add(currentPacket);
                currentPacket.reset();
            }
            //print("received");
            i += 1;
        }
    }

    void startThread(){
        //if (general) {
        //    if (!threadGeneral.IsAlive) {
                threadGeneral.Start();
        //    }
        //} else {
        //    threadMotors.Start();
        //    threadGyro.Start();
        //    threadImages.Start();
        //}
    }
    void stopThread(){
        //if (general) {
            //threadGeneral.Abort();
            runServer = false;
            threadGeneral = new Thread(() => GetData(port));
        
        //} else {
        //    threadMotors = new Thread(() => GetData(motorsPort, PortsID.motors));
        //    threadGyro = new Thread(() => GetData(gyroPort, PortsID.imu));
        //    threadImages = new Thread(() => GetData(imageCommandsPort, PortsID.commands));
        //}
    }
    
    // Position is the data being received in this example
    //Vector3 position = Vector3.zero;
    //void Update()
    //{
    //    // Set this object's position in the scene according to the position received
    //    //transform.position = position;
    //}
}