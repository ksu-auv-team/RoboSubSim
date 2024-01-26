using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public class CommandPacket{
    public byte[] body; // include id, body, crc
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
    // write to this
    public CommandPacket(int length, SceneManagement sceneM){
        commandState = states.Waiting;
        bodyLen = length;
        body = new byte[bodyLen];
        body_counter = 0;
        EscapeNextByte = false;
        sceneManagement = sceneM;
    }
    // process from this
    public CommandPacket(SceneManagement sceneM, byte[] bytedata, int length){
        commandState = states.Waiting;
        bodyLen = length;
        body = new byte[length];
        System.Buffer.BlockCopy(bytedata, 0, body, 0, length);
        body_counter = 0;
        EscapeNextByte = false;
        sceneManagement = sceneM;
    }
    // send from this
    public CommandPacket(SceneManagement sceneM, ushort id, byte[] header, byte[] data){
        commandState = states.Waiting;
        bodyLen = 2 + header.Length + data.Length + 2;
        body = new byte[bodyLen];
        
        byte[] id_bytes = System.BitConverter.GetBytes(id);
        System.Array.Reverse(id_bytes);
        System.Buffer.BlockCopy(id_bytes, 0, body, 0, 2);
        System.Buffer.BlockCopy(header, 0, body, 2, header.Length);
        System.Buffer.BlockCopy(data, 0, body, 2 + header.Length, data.Length);
        System.Buffer.BlockCopy(crc_itt16_false(2 + header.Length + data.Length), 0, body, 2 + header.Length + data.Length, 2);
        Debug.Log("New Send Packet Created");
        body_counter = 0;
        EscapeNextByte = false;
        sceneManagement = sceneM;
    }
    public byte[] crc_itt16_false(int length){
        ushort crc = 0xFFFF;
        int pos = 0;
        while (pos < length){
            byte b = body[pos];
            for (int i = 0; i < 8; i++){
                int bit = ((b >> (7-i) & 1) == 1) ? 1 : 0;
                int c15 = ((crc >> 15 & 1) == 1) ? 1 : 0;
                crc <<= 1;
                crc &= 0xFFFF;
                if (c15 != bit){
                    crc ^= 0x1021;
                }
            }
            pos += 1;
        }
        byte[] crc_bytes = System.BitConverter.GetBytes(crc);
        System.Array.Reverse(crc_bytes);
        return crc_bytes;
    }
    public bool processByte(byte data){
        //Debug.Log("Byte: " + data + ", Count: " + body_counter + ", bodyLen: " + bodyLen + ", Escape: " + EscapeNextByte);
        switch(commandState){
            case states.Reading:
                // buffer overflow (increase bodyLen or endByte missed)
                if (body_counter >= bodyLen){
                    Debug.Log("Command length overflown");
                    return true;
                }
                // terminate message
                if (data == END) {
                    if (EscapeNextByte) {
                        body[body_counter] = data;  // also load the end byte
                        body_counter += 1;
                        EscapeNextByte = false;
                    }
                    else {
                        //Debug.Log("Count: " + body_counter);
                        byte[] expected_crc = crc_itt16_false(body_counter-2);
                        if (expected_crc[0] != body[body_counter-2] && expected_crc[1] != body[body_counter-1]) {
                            Debug.Log("Expected CRC: " + System.String.Join(',', expected_crc));
                        }
                        return true;
                    }
                    break;
                }
                // escape next byte
                else if (data == ESCAPE && !EscapeNextByte) {
                    EscapeNextByte = true; 
                    break;
                }
                // load data
                body[body_counter] = data;
                //Debug.Log("Add byte to body: " + body[body_counter]);
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
    private bool isCommand(byte[] command){
        int index = 0;
        while (index < command.Length) {
            if (body[index + 2] != command[index]){
                return false;
            }
            index += 1;
        }
        return true;
    }
    public byte processCommand(Dictionary<string, byte[]> commands){
        byte error_code = 0;
        //Debug.Log("Length: " + body.Length + " Bytes: " + ToString());
        bool is_command = false;
        foreach (KeyValuePair<string, byte[]> command in commands) {
            if (isCommand(command.Value)) {
                is_command = true;
                Debug.Log("Received Command: " + command.Key);
                switch(command.Key){
                    case "RAW":
                        float[] motor_power = ParseFloats(body, 8, 5);
                        Debug.Log("Vals: " + string.Join(",", motor_power));
                        sceneManagement.setMotorPower(  motor_power[0],motor_power[1],motor_power[2],motor_power[3],
                                                        motor_power[4],motor_power[5],motor_power[6],motor_power[7]);
                        break;
                    case "LOCAL":
                        goto default;
                    case "GLOBAL":
                        goto default;
                    case "RELDOF":
                        goto default;
                    case "BNO055P":
                        goto default;
                    case "MS5837P":
                        goto default;
                    case "WDGF":
                        goto default;
                    case "BNO055R":
                        goto default;
                    case "MS5837R":
                        goto default;
                    case "MMATS":
                        goto default;
                    case "MMATU":
                        goto default;
                    case "TINV":
                        goto default;
                    case "BNO055A":
                        goto default;
                    // simulation environment commands
                    case "CAPTUREU":
                        goto default;
                    case "RESETU":
                        sceneManagement.sceneSelect = body[8];
                        sceneManagement.sceneRefresh = true;
                        break;
                    case "ROBOTSELU":
                        goto default;
                    
                    default:
                        Debug.Log("Unimplemented command: " + command.Key);
                        break;
                }
            }
            if (is_command) {
                break;
            }
        }
        if (!is_command) {
            Debug.Log("Unknown Command. Bytes: " + ToString());
        }
        
        //ParseFloats(body,1,0);
        //Debug.Log(ParseFloats(body,1,0)[0]);
        return error_code;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nwStream"></param>
    /// <param name="bytes_to_send"></param>
    /// <returns> 
    /// True: END byte sent
    /// False: END byte not sent
    /// </returns>
    public bool sendPacket(NetworkStream nwStream, int bytes_to_send){
        while (bytes_to_send > 0){
            bytes_to_send -= 1;
            //Debug.Log("count: " + body.Length + " i: " + body_counter);
            if (body_counter == 0) {
                nwStream.Write(new byte[] {START}, 0 ,1);
                //Debug.Log("Wrote START");
            }
            byte writebyte = body[body_counter];
            //Debug.Log("write byte" + writebyte);
            if (writebyte == START || writebyte == ESCAPE || writebyte == END) {
                nwStream.Write(new byte[] {ESCAPE, writebyte}, 0, 2);
                //Debug.Log("Wrote ESCAPE + byte");
            } else {
                nwStream.Write(new byte[] {writebyte}, 0, 1);
                //Debug.Log("Wrote byte");
            }

            body_counter += 1;
            if (body_counter == body.Length) {
                //Debug.Log("Sent: " + ToString());
                nwStream.Write(new byte[] {END}, 0 ,1);
                //Debug.Log("Wrote END");
                return true;
            }
        }
        return false;
    }
    public void reset(){
        System.Array.Clear(body, 0, bodyLen);
        body_counter = 0;
        EscapeNextByte = false;
        commandState = states.Waiting;
    }
    public string ToString(){
        return System.String.Join(",", body);
        //return System.Text.Encoding.Default.GetString(body);
    }
    public static float[] ParseFloats(byte[] byteValues, int numFloats, int startIndex){
        float[] floatValues = new float[numFloats];
        int count = 0;
        while (count < numFloats){
            //Debug.Log(byteValues[0]+","+byteValues[1]+","+byteValues[2]+","+byteValues[3]+",");
            floatValues[count] = System.BitConverter.ToSingle(byteValues, startIndex);
            startIndex += 4;
            count += 1;
        }
        return floatValues;
    }
}
public class TCPServer : MonoBehaviour
{
    readonly string[] POSSIBLE_COMMANDS = new string[] {
            // control board commands
            "RAW", "LOCAL", "GLOBAL", "RELDOF", "BNO055P", "MS5837P", "WDGS", "BNO055R", "MS5837R",
            "MMATS", "MMATU", "TINV", "BNO055A",
            // Other commands
            "CAPTUREU", "RESETU", "CAMCFGU", "ROBOTSELU",
            // acknowledge
            "ACK"
        };
    Dictionary<string, byte[]> commandsHeader = new Dictionary<string, byte[]>();
    SceneManagement sceneManagement;
    public string IPAddr = "127.0.0.1";
    public int port = -1;
    public int msPerTransmit = 100;
    private float currentTime = 0;
    public string ui_message = "Closed";
    private Thread threadRust;
    public bool runServer = false;
    private bool serverStarted = false;
    private byte[] buffer;
    List<CommandPacket> receiveCommandsPool = new List<CommandPacket>(64);
    List<CommandPacket> sendCommandsPool = new List<CommandPacket>(64);
    CommandPacket receiveLoadingPacket;
    CommandPacket sendLoadingPacket;

    private Thread threadSimCB;
    public bool simCB_Connect = false;
    public string simCB_IPAddr = "127.0.0.1";
    public int simCB_Port = 5014;
    public bool simCB_Connected = false;
    TcpClient simCB_client;
    private byte[] simCB_buffer;
    List<CommandPacket> simCB_receiveCommandsPool = new List<CommandPacket>(64);
    List<CommandPacket> simCB_sendCommandsPool = new List<CommandPacket>(64);
    CommandPacket simCB_receiveLoadingPacket;
    CommandPacket simCB_sendLoadingPacket;
    void Start()
    {
        foreach (string command in POSSIBLE_COMMANDS) {
            commandsHeader[command] = Encoding.UTF8.GetBytes(command);
        }
        Debug.Log(commandsHeader.Keys.Count);
        sceneManagement = GetComponent<SceneManagement>();
        receiveLoadingPacket = new CommandPacket(128, sceneManagement);
        sendLoadingPacket = new CommandPacket(128, sceneManagement);
        buffer = new byte[10];
        threadRust = new Thread(() => GetData(port));

        simCB_buffer = new byte[10];
        simCB_receiveLoadingPacket = new CommandPacket(128, sceneManagement);
        simCB_sendLoadingPacket = new CommandPacket(128, sceneManagement);
        
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
        //Debug.Log("Received Count: " + receiveCommandsPool.Count + " Send Count: " + sendCommandsPool.Count);
        //print(runServer);

        if (simCB_Connect && !simCB_Connected) {
            simCB_client = new TcpClient(simCB_IPAddr, simCB_Port);
            simCB_client.ReceiveBufferSize = 8;
            simCB_Connected = true;
            threadSimCB = new Thread(() => GetDataSimCB());
            threadSimCB.Start();
            Debug.Log("SimCB connected");
        }
        if (!simCB_Connect && simCB_Connected) {
            Debug.Log("SimCB disconnect");
            simCB_Connected = false;
            simCB_client.Close();
            threadSimCB.Join();
        }
    }
    void OnDestroy(){
        stopThread();
    }
    void OnApplicationQuit(){
        stopThread();
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
            running = Connection(client, nwStream, false);
        }
        Debug.Log("Port:" + port + ", closed on " + IPAddr);
        server.Stop();
        ui_message = "Closed:"+IPAddr+":"+port;
        // Create the server
        
    }
    void GetDataSimCB(){
        NetworkStream nwStream = simCB_client.GetStream();
        while (simCB_Connected)
        {
            Connection(simCB_client, nwStream, true);
        }
    }
    bool Connection(TcpClient client, NetworkStream nwStream, bool isSimCB)
    {
        if (currentTime > msPerTransmit){
            //Debug.Log(currentTime);
            if (isSimCB) {
                if (simCB_sendCommandsPool.Count > 0) {
                    ParseSendData(nwStream, isSimCB);
                }
            } else {
                if (sendCommandsPool.Count > 0) {
                    ParseSendData(nwStream, isSimCB);
                }
            }
            currentTime = 0;
        }
        // Read data from the network stream
        if (nwStream.DataAvailable){
            int bytesRead = 0;
            if (isSimCB){
                bytesRead = nwStream.Read(simCB_buffer, 0, client.ReceiveBufferSize);
            } else {
                bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
            }
            //print(bytesRead);
            ParseReceivedData(bytesRead, isSimCB);
        }
        else{
            // process the latest received command (Last in first out)
            if (receiveCommandsPool.Count > 0){
                // received too many commands, clear oldest half of commands
                if (receiveCommandsPool.Count > 128) {
                    print("TCP Receive Command Pool Overflow");
                    receiveCommandsPool.RemoveRange(0, receiveCommandsPool.Count / 2);
                }
                
                
                if (isSimCB){
                    // SimCB
                    // copy
                    if(simCB_Connected){
                        Debug.Log("Copying rust command to simCB send pool");
                        simCB_sendCommandsPool.Add(
                            new CommandPacket(sceneManagement, 
                                            receiveCommandsPool[receiveCommandsPool.Count-1].body,
                                            receiveCommandsPool[receiveCommandsPool.Count-1].body_counter)
                                            );
                        receiveCommandsPool.RemoveAt(receiveCommandsPool.Count-1);
                    } else {
                        return false; // disconnect for simCB, join thread
                    }
                    return true;
                } else {
                    // Rust only
                    byte error_code = receiveCommandsPool[receiveCommandsPool.Count-1].processCommand(commandsHeader);
                    if (error_code < 5) {
                        sendCommandsPool.Add(new CommandPacket(sceneManagement, 
                                                                id: 0,
                                                                header: commandsHeader["ACK"], 
                                                                data: new byte[] {
                                                                    receiveCommandsPool[receiveCommandsPool.Count-1].body[0],
                                                                    receiveCommandsPool[receiveCommandsPool.Count-1].body[1],
                                                                    error_code
                                                                }));
                        sendCommandsPool.Add(new CommandPacket(sceneManagement, id: 10, 
                                                                    header: commandsHeader["WDGS"], 
                                                                    data: new byte[] {1}//imu_buf
                                                                    ));
                    }
                    receiveCommandsPool.RemoveAt(receiveCommandsPool.Count-1);
                }
            }
            
        }
        
            //Debug.Log("No msg received");
        return true;
    }
    void ParseSendData(NetworkStream nwStream, bool isSimCB){
        if (isSimCB) {
            if (simCB_sendCommandsPool.Count > 128) {
                print("simCB: TCP Send Command Pool Overflow");
                simCB_sendCommandsPool.RemoveRange(1, simCB_sendCommandsPool.Count / 2);
            }
            if (simCB_Connected) {
                Debug.Log("Sending simCB packet");
                if (simCB_sendCommandsPool[0].sendPacket(nwStream, 2)) {
                    simCB_sendCommandsPool.RemoveAt(0);
                };
                return;
            }
            return;
        }
        // have too many sends in the pool, clear the oldest half of commands (but leave zero index for sending)
        if (sendCommandsPool.Count > 128) {
            print("TCP Send Command Pool Overflow");
            sendCommandsPool.RemoveRange(1, sendCommandsPool.Count / 2);
        }
        // process the earlies send command (Last in last out)
        // because new send may be added to the pool
        if (sendCommandsPool[0].sendPacket(nwStream, 2)) {
            //Debug.Log("Complete Send");
            sendCommandsPool.RemoveAt(0);
        } else {
            //Debug.Log("current count: " + sendCommandsPool[0].body_counter);
        }
        //byte[] imu_buf = new byte[24];
        //IMU imu = sceneManagement.getRobotIMU();
        //System.Buffer.BlockCopy(System.BitConverter.GetBytes(
        //                        imu.quaternion.eulerAngles.z), 0, imu_buf, 0, 4);
        //System.Buffer.BlockCopy(System.BitConverter.GetBytes(
        //                        360-imu.quaternion.eulerAngles.x), 0, imu_buf, 4, 4);
        //System.Buffer.BlockCopy(System.BitConverter.GetBytes(
        //                        360-imu.quaternion.eulerAngles.y), 0, imu_buf, 8, 4);
        //System.Buffer.BlockCopy(System.BitConverter.GetBytes(
        //                        -imu.linearAccel.z), 0, imu_buf, 12, 4);
        //System.Buffer.BlockCopy(System.BitConverter.GetBytes(
        //                        imu.linearAccel.x), 0, imu_buf, 16, 4);
        //System.Buffer.BlockCopy(System.BitConverter.GetBytes(
        //                        imu.linearAccel.y), 0, imu_buf, 20, 4);
        //if (sendCommandsPool.Count < 16){
            //sendLoadingPacket = new CommandPacket(sceneManagement, id: 10, 
            //                                                    header: commandsHeader["BNO055R"], 
            //                                                    data: new byte[] {0}//imu_buf
            //                                                    );
            //CommandPacket newPacket = new CommandPacket(sceneManagement, sendLoadingPacket.body, sendLoadingPacket.bodyLen);
            //sendCommandsPool.Add(newPacket);
            
        //}
        //nwStream.Write(imu_buf,0,imu_buf.Length);
                //Debug.Log(imu.quaternion.eulerAngles);
                //Debug.Log(System.BitConverter.IsLittleEndian);
    }
    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    void ParseReceivedData(int bytesRead, bool isSimCB)
    {
        int i = 0;
        if (isSimCB){
            // SimCB
            while (i < bytesRead){
                if (simCB_receiveLoadingPacket.processByte(simCB_buffer[i])){
                    //print(receiveLoadingPacket.ToString());
                    Debug.Log("SimCB received command packet");
                    CommandPacket newPacket = new CommandPacket(sceneManagement, simCB_receiveLoadingPacket.body, simCB_receiveLoadingPacket.body_counter);
                    simCB_receiveCommandsPool.Add(newPacket);
                    simCB_receiveLoadingPacket.reset();
                }
                //print("received");
                i += 1;
            }
            return;
        } else {
            // Robot (Rust)
            while (i < bytesRead){
                if (receiveLoadingPacket.processByte(buffer[i])){
                    //print(receiveLoadingPacket.ToString());
                    Debug.Log("RustServer received command packet");
                    CommandPacket newPacket = new CommandPacket(sceneManagement, receiveLoadingPacket.body, receiveLoadingPacket.body_counter);
                    receiveCommandsPool.Add(newPacket);
                    receiveLoadingPacket.reset();
                }
                //print("received");
                i += 1;
            }
        }
    }

    void startThread(){
        threadRust.Start();
    }
    void stopThread(){
        runServer = false;
        threadRust.Join();
        threadRust = new Thread(() => GetData(port));
    }
    
    // Position is the data being received in this example
    //Vector3 position = Vector3.zero;
    //void Update()
    //{
    //    // Set this object's position in the scene according to the position received
    //    //transform.position = position;
    //}
}