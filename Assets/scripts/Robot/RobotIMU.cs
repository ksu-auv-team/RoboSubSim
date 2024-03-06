using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct IMU{
        public Vector3 linearAccel;
        public Vector3 linearVel;
        public Quaternion quaternion;
        public Quaternion robotRotation;
        public float accelNoise;
        public float accelNoiseDrift;
        public Vector3 accumulatedAccelDrift;
        public float gyroNoise;
        public float gyroNoiseDrift;
        public Quaternion accumulatedGyroDrift;
        public float elapsedTime;
        public Vector3 position;
        public Vector3 robotPosition;
        public IMU(float accNoise,float accNoiseDrift, float gyNoise, float gyNoiseDrift){
            linearAccel = new Vector3(0,0,0);
            linearVel = new Vector3(0,0,0);
            quaternion = new Quaternion(0,0,0,1);
            accelNoise = accNoise;
            accelNoiseDrift = accNoiseDrift;
            accumulatedAccelDrift = new Vector3(0,0,0);
            gyroNoise = gyNoise;
            gyroNoiseDrift = gyNoiseDrift;
            accumulatedGyroDrift = new Quaternion(0,0,0,1);
            position = new Vector3(0,0,0);
            robotPosition = new Vector3(0,0,0);
            robotRotation = new Quaternion(0,0,0,1);
            elapsedTime = 0;
        }
        public void applyAccelNoise(){
            this.linearAccel += new Vector3(Random.Range(-this.accelNoise, this.accelNoise),
                                            Random.Range(-this.accelNoise, this.accelNoise),
                                            Random.Range(-this.accelNoise, this.accelNoise));
            this.accumulatedAccelDrift = new Vector3(accelNoiseDrift, accelNoiseDrift, accelNoiseDrift)
                                                        * Random.Range(0.95f, 1.05f) * elapsedTime;
            this.linearAccel += this.accumulatedAccelDrift;
            //this.linearAccel += new Vector3(accelNoiseDrift, accelNoiseDrift, accelNoiseDrift)
            //                    * Random.Range(0.8f, 1.2f) * elapsedTime;
        }
        public void applyGyroNoise(){
            this.quaternion *= Quaternion.Euler(new Vector3(Random.Range(-this.gyroNoise, this.gyroNoise),
                                                            Random.Range(-this.gyroNoise, this.gyroNoise),
                                                            Random.Range(-this.gyroNoise, this.gyroNoise)));
            this.accumulatedGyroDrift = Quaternion.Euler(new Vector3(gyroNoiseDrift, gyroNoiseDrift, gyroNoiseDrift)
                                                            * Random.Range(0.95f, 1.05f) * elapsedTime);
            this.quaternion *= this.accumulatedGyroDrift;
            //this.quaternion *= Quaternion.Euler(new Vector3(gyroNoiseDrift, gyroNoiseDrift, gyroNoiseDrift)
            //                                                * Random.Range(0.8f, 1.2f) * elapsedTime);
        }
        public void UnityToRobotPos(){
            robotPosition.x = -position.z;
            robotPosition.y = position.x;
            robotPosition.z = position.y;
        }
        public void UnityToRobotRot(){
            robotRotation.w = quaternion.w;
            robotRotation.z = quaternion.y;
            robotRotation.y = quaternion.x;
            robotRotation.x = -quaternion.z;

            //robotRotation.w = quaternion.w;
            //robotRotation.z = -quaternion.y;
            //robotRotation.y = -quaternion.x;
            //robotRotation.x = quaternion.z;
        }
}
public class RobotIMU : MonoBehaviour
{
    [Tooltip("typical range, ug/sqrt(Hz)")]
    public float accelNoise = 150f;
    [Tooltip("Drift, ug/sqrt(Hz)")]
    public float accelNoiseDrift = 0f;
    [Tooltip("window (s)")]
    public float avgWindow = 0.01f;
    [Tooltip("typical range, deg/s")]
    public float gyroNoise = 0.1f;
    [Tooltip("drift, deg/s")]
    public float gyroNoiseDrift = 1.5f;
    public IMU imu;
    Rigidbody m_rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        //resetIMUWithNoise(accelNoise, accelNoiseDrift, gyroNoise, gyroNoiseDrift);
        resetIMUWithoutNoise();
        m_rigidbody = this.GetComponent<Rigidbody>();
    }
    void resetIMUWithNoise(float accN = 150f, float accD = 0f, float gyroN = 0.1f, float gyroD = 1.5f){
        accelNoise = accN;
        accelNoiseDrift = accD;
        gyroNoise = gyroN;
        gyroNoiseDrift = gyroD;
        imu = new IMU(accelNoise * Mathf.Abs(Physics.gravity.y) * .000001f * Mathf.Sqrt(1/Time.fixedDeltaTime),
                    accelNoiseDrift * Mathf.Abs(Physics.gravity.y) * .000001f * Mathf.Sqrt(1/Time.fixedDeltaTime),
                    gyroNoise, gyroNoiseDrift);
    }
    void resetIMUWithoutNoise(){
        imu = new IMU(0,0,0,0);
    }
    // Update is called once per frame
    //void Update()
    //{
        
    //}
    void FixedUpdate(){
        imu.quaternion = transform.rotation;
        imu.position = transform.position;
        // v1 = v0 + acc * dt
        // acc = (v1 - v0) / dt
        imu.linearAccel = (m_rigidbody.velocity - imu.linearVel)/Time.fixedDeltaTime; 
        imu.linearVel = m_rigidbody.velocity;
        
        imu.applyAccelNoise();
        imu.applyGyroNoise();
        imu.elapsedTime += Time.fixedDeltaTime;
        //print(imu.quaternion.eulerAngles);

        imu.UnityToRobotPos();
        imu.UnityToRobotRot();
    }
    
}
