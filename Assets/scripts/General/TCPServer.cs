using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class TCPServer : MonoBehaviour
{
    public string IPAddr = "127.0.0.1";
    public int motorsPort = -1;
    public int gyroPort = -1;
    public int imageCommandsPort = -1;
    //bool running;
    public int msPerTransmit = 100;

    private robot_imu imu_script;
    private RobotForce motor_script;
    private robot_camera camera_script;

    private enum PortsID : int{
        motors = 0,
        gyro = 1,
        commands = 2
    }
    private Thread threadMotors;
    private Thread threadGyro;
    private Thread threadImages;
    private PortsID portsID; 
    void Start()
    {
        motor_script = GetComponent<RobotForce>();
        imu_script = GetComponent<robot_imu>();
        camera_script = GetComponent<robot_camera>();

        // Receive on a separate thread so Unity doesn't freeze waiting for data
        //ThreadStart tsMotors = new ThreadStart(GetData);
        threadMotors = new Thread(() => GetData(motorsPort, PortsID.motors));
        threadMotors.Start();
        //ThreadStart tsGyro = new ThreadStart(GetData);
        threadGyro = new Thread(() => GetData(gyroPort, PortsID.gyro));
        threadGyro.Start();
        //ThreadStart tsImages = new ThreadStart(GetData);
        threadImages = new Thread(() => GetData(imageCommandsPort, PortsID.commands));
        threadImages.Start();
    }
    void OnDestroy(){
        threadMotors.Abort();
        threadGyro.Abort();
        threadImages.Abort();
    }
    void GetData(int port, PortsID id)
    {
        if (port < 0){
            return;
        }
        
        // Create the server
        TcpListener server = new TcpListener(IPAddress.Parse(IPAddr), port);
        server.Start();
        print("Port:" + port + ", started on " + IPAddr + ". waiting connection...");
        // Create a client to get the data stream
        TcpClient client = server.AcceptTcpClient();
        NetworkStream nwStream = client.GetStream();
        print("Port:"+port + ", stream connected");
        // Start listening
        bool running = true;
        while (running)
        {
            running = Connection(client, nwStream, id);
        }
        server.Stop();
    }

    bool Connection(TcpClient client, NetworkStream nwStream, PortsID id)
    {
        switch (id) {
            // receivers
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
            case PortsID.gyro:
                Thread.Sleep(msPerTransmit);
                ParseSendData(id, nwStream);
                break;
        }
        return true;
    }
    void ParseSendData(PortsID id, NetworkStream nwStream){
        switch (id) {
            case PortsID.gyro:
                byte[] gyro_buf = new byte[12];
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        imu_script.imu.quaternion.eulerAngles.x), 0, gyro_buf, 0, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        imu_script.imu.quaternion.eulerAngles.y), 0, gyro_buf, 4, 4);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(
                                        imu_script.imu.quaternion.eulerAngles.z), 0, gyro_buf, 8, 4);
                nwStream.Write(gyro_buf,0,gyro_buf.Length);
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
        }
        return;
    }

    // Position is the data being received in this example
    //Vector3 position = Vector3.zero;
    //void Update()
    //{
    //    // Set this object's position in the scene according to the position received
    //    //transform.position = position;
    //}
}