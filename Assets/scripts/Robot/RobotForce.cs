using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RobotForce : MonoBehaviour
{
    public GameObject[] thrusters;
    public float strength = 2.36f;
    public float SubmergeDepth;
    public bool forceVisual = true;
    public bool tcpControlled = false;
    public Material forceVisualMaterial;
    Rigidbody m_rigidBody;
    Vector3[] thruster_directions;
    const float KGF_TO_N = 9.80665f;
    const float MAX_FORCE = 2.36f*KGF_TO_N;
    public float[] thrust_strengths = {0f,0f,0f,0f,0f,0f,0f,0f};
    float timer = 0;
    float random_thrust = 0;
    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        if (forceVisualMaterial != null){
            foreach (var thruster in thrusters) {
                thruster.transform.Find("force_visual").gameObject.GetComponent<MeshRenderer>().material = forceVisualMaterial;
            }
        }
        if (!forceVisual){
            foreach (var thruster in thrusters) {
                thruster.transform.Find("force_visual").gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        stop_thrust();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 2) {
            timer = 0;
            random_thrust = Random.Range(-1f,1f);
        }
        timer += Time.deltaTime;
    }
    // t1-8 from [-1,1] please
    public void set_thrusts_strengths(){
        add_thruster_force(0, thrust_strengths[0] * strength * KGF_TO_N);
        add_thruster_force(1, thrust_strengths[1] * strength * KGF_TO_N);
        add_thruster_force(2, thrust_strengths[2] * strength * KGF_TO_N);
        add_thruster_force(3, thrust_strengths[3] * strength * KGF_TO_N);
        add_thruster_force(4, thrust_strengths[4] * strength * KGF_TO_N);
        add_thruster_force(5, thrust_strengths[5] * strength * KGF_TO_N);
        add_thruster_force(6, thrust_strengths[6] * strength * KGF_TO_N);
        add_thruster_force(7, thrust_strengths[7] * strength * KGF_TO_N);
    }
    void stop_thrust(){
        add_thruster_force(0, 0);
        add_thruster_force(1, 0);
        add_thruster_force(2, 0);
        add_thruster_force(3, 0);
        add_thruster_force(4, 0);
        add_thruster_force(5, 0);
        add_thruster_force(6, 0);
        add_thruster_force(7, 0);
    }
    void submerge(float force){
        add_thruster_force(0, -force);
        add_thruster_force(1, -force);
        add_thruster_force(2, -force);
        add_thruster_force(3, -force);   
    }
    void forward(float force){
        add_thruster_force(4, force);
        add_thruster_force(5, force);
        add_thruster_force(6, -force);
        add_thruster_force(7, -force);
    }

    /// force: positive CCW
    void spin(float force){
        add_thruster_force(4, force );
        add_thruster_force(5, -force);
        add_thruster_force(6, force );
        add_thruster_force(7, -force);
    }

    void add_thruster_force(int id, float force){
        if (force < 0) {
            force = force * 1.85f / 2.36f; // max forward/reverse spec sheet: https://www.thingbits.in/products/t100-thruster-no-esc
        }
        if (force > MAX_FORCE) {
            force = MAX_FORCE;
        }
        if (force < -MAX_FORCE * 1.85f/2.36f) {
            force = -MAX_FORCE * 1.85f/2.36f;
        }
        m_rigidBody.AddForceAtPosition(force * thrusters[id].transform.forward, thrusters[id].transform.position);
        if (forceVisual) {
            Transform force_visual = thrusters[id].transform.Find("force_visual");
            if (force_visual != null) {
                //print(force_visual.position);
                //if (force < 0){
                //    force_visual.localRotation = Quaternion.Euler(180,0,0);
                //}
                //force_visual.localPosition = new Vector3(0,0,(force/2)/MAX_FORCE);
                var arrow_scale = (force)/MAX_FORCE;
                force_visual.localScale = new Vector3(arrow_scale/5,arrow_scale/5,arrow_scale);
            }
        }
    }

    void FixedUpdate(){
        //print(transform.rotation.eulerAngles); // (roll,yaw,pitch)
        //SubmergeDepth = GetComponent<buoyancy_forces>().underwater;
        if (!tcpControlled){
            if (transform.position.y <= SubmergeDepth) {
                submerge(0);            
                //spin(-strength * KGF_TO_N * random_thrust);
            } else {
                submerge(strength * KGF_TO_N);
            }
        } else {
            set_thrusts_strengths();
            //print(thrust_strengths);
        }
        //forward_force(strength * m_rigidBody.mass);

    }
}
