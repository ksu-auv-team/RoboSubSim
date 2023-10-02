using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class TCPServer : MonoBehaviour
{
    public string IPAddr = "127.0.0.1";
    public int port = -1;
    //public int motorsPort = -1;
    //public int gyroPort = -1;
    //public int imageCommandsPort = -1;
    //public bool general = true;
    //bool running;
    public int msPerTransmit = 100;

    private RobotIMU imu_script;
    private RobotForce motor_script;
    private RobotCamera camera_script;

    private enum PortsID : int{
        general = 0,
        motors = 1,
        imu = 2,
        commands = 3,
        other = 4,
    }
    //private Thread threadMotors;
    //private Thread threadGyro;
    //private Thread threadImages;
    private Thread threadGeneral;
    //private PortsID portsID; 
    //[HideInInspector]
    public bool runServer = false;
    private bool serverStarted = false;
    void Start()
    {
        motor_script = GetComponent<RobotForce>();
        imu_script = GetComponent<RobotIMU>();
        camera_script = GetComponent<RobotCamera>();
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        threadGeneral = new Thread(() => GetData(port, PortsID.general));
        //threadMotors = new Thread(() => GetData(motorsPort, PortsID.motors));
        //threadGyro = new Thread(() => GetData(gyroPort, PortsID.imu));
        //threadImages = new Thread(() => GetData(imageCommandsPort, PortsID.commands));


    }
    void Update(){
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
            threadGeneral.Abort();
        //}else{
        //    threadMotors.Abort();
        //    threadGyro.Abort();
        //    threadImages.Abort();
        //}
    }
    void GetData(int port, PortsID id)
    {
        if (port < 0){
            return;
        }
        // Use TCP control because potentially valid port, configure scripts
        switch (id) {
            case PortsID.general:
            case PortsID.motors:
                // change the source of robot control from Unity to TCP
                motor_script.tcpControlled = true;
                break;
        }
        
        TcpListener server = new TcpListener(IPAddress.Parse(IPAddr), port);
        server.Start();
        print("Port:" + port + ", started on " + IPAddr + ". waiting connection...");
        while (!server.Pending()) {
            if (!runServer) {
                server.Stop();
                print("Port:" + port + ", closed on " + IPAddr);
                return;
            }
        }
        // Create a client to get the data stream
        TcpClient client = server.AcceptTcpClient();
        NetworkStream nwStream = client.GetStream();
        print("Port:"+port + ", stream connected");
        // Start listening
        bool running;
        while (runServer)
        {
            running = Connection(client, nwStream, id);
        }
        print("Port:" + port + ", closed on " + IPAddr);
        server.Stop();
        // Create the server
        
    }

    bool Connection(TcpClient client, NetworkStream nwStream, PortsID id)
    {
        switch (id) {
            // receivers
            case PortsID.general:
                ParseSendData(id, nwStream);
                goto case PortsID.motors;
            case PortsID.motors:
            case PortsID.commands:
                // Read data from the network stream
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                // Decode the bytes into a string
                string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //print(dataReceived);
                // Make sure we're not getting an empty string
                dataReceived.Trim();
                if (dataReceived != null && dataReceived != "")
                {
                    // Convert the received string of data to the format we are using
                    ParseReceivedData(dataReceived, id);
                    //nwStream.Write(buffer, 0, bytesRead);
                }
                break;
            // transmitters
            case PortsID.imu:
                Thread.Sleep(msPerTransmit);
                ParseSendData(id, nwStream);
                break;
            
        }
        return true;
    }
    void ParseSendData(PortsID id, NetworkStream nwStream){
        byte[] imu_buf = new byte[24];
        switch (id) {
            case PortsID.general:
                // attach 'I' to the end of imu buffer
                imu_buf = new byte[26];
                System.Buffer.BlockCopy(System.BitConverter.GetBytes('I'), 0, imu_buf, 24, 2);
                goto case PortsID.imu;
            case PortsID.imu:
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        imu_script.imu.quaternion.eulerAngles.z), 0, imu_buf, 0, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        -imu_script.imu.quaternion.eulerAngles.x), 0, imu_buf, 4, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        -imu_script.imu.quaternion.eulerAngles.y), 0, imu_buf, 8, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        -imu_script.imu.linearAccel.z), 0, imu_buf, 12, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        imu_script.imu.linearAccel.x), 0, imu_buf, 16, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        imu_script.imu.linearAccel.y), 0, imu_buf, 20, 4);
                nwStream.Write(imu_buf,0,imu_buf.Length);
                //Debug.Log(imu_script.imu.quaternion.eulerAngles);
                //Debug.Log(System.BitConverter.IsLittleEndian);
                break;
        }
    }
    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    void ParseReceivedData(string dataString, PortsID id)
    {
        Debug.Log(dataString);
        // Remove the parentheses
        if (dataString.StartsWith("(") && dataString.EndsWith(")"))
        {
            dataString = dataString.Substring(1, dataString.Length - 2);
        }

        // Split the elements into an array
        string[] stringArray = dataString.Split(',');
        switch (id) {
            case PortsID.general:
                var endVal = char.Parse(stringArray[stringArray.Length-1]);
                print(endVal);
                if (endVal == 'R') {    // raw (individual motors)
                    goto case PortsID.motors;
                }
                if (endVal == 'C') {    // command
                    goto case PortsID.commands;
                }
                if (endVal == 'L') {    // local

                }
                break;
            case PortsID.motors:
                motor_script.thrust_strengths[0] = float.Parse(stringArray[0]);
                motor_script.thrust_strengths[1] = float.Parse(stringArray[1]);
                motor_script.thrust_strengths[2] = float.Parse(stringArray[2]);
                motor_script.thrust_strengths[3] = float.Parse(stringArray[3]);
                motor_script.thrust_strengths[4] = float.Parse(stringArray[4]);
                motor_script.thrust_strengths[5] = float.Parse(stringArray[5]);
                motor_script.thrust_strengths[6] = float.Parse(stringArray[6]);
                motor_script.thrust_strengths[7] = float.Parse(stringArray[7]);
                break;
            case PortsID.commands:
                int command = int.Parse(stringArray[0]);
                camera_script.CommandTrigger(command);
                break;
            case PortsID.other:
                motor_script.other_control[0] = float.Parse(stringArray[0]);
                motor_script.other_control[1] = float.Parse(stringArray[1]);
                motor_script.other_control[2] = float.Parse(stringArray[2]);
                motor_script.other_control[3] = float.Parse(stringArray[3]);
                motor_script.other_control[4] = float.Parse(stringArray[4]);
                motor_script.other_control[5] = float.Parse(stringArray[5]);
                break;
        }
        return;
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
            threadGeneral = new Thread(() => GetData(port, PortsID.general));
        
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