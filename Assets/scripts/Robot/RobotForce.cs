using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotForce : MonoBehaviour
{
    public GameObject[] thrusters;
    public float strength = 2.36f;
    public float SubmergeDepth;

    Rigidbody m_rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void submerge_force(float force){
        var forceDirection = new Vector3(0, force, 0);
        m_rigidBody.AddForceAtPosition(forceDirection, thrusters[0].transform.position);
        m_rigidBody.AddForceAtPosition(forceDirection, thrusters[1].transform.position);
        m_rigidBody.AddForceAtPosition(forceDirection, thrusters[2].transform.position);
        m_rigidBody.AddForceAtPosition(forceDirection, thrusters[3].transform.position);
        //if (transform.rotation.eulerAngles.z/180 > 0) {
        //    m_rigidBody.AddForceAtPosition(forceDirection/transform.rotation.eulerAngles.z, thrusters[1].transform.position);
        //    m_rigidBody.AddForceAtPosition(forceDirection/transform.rotation.eulerAngles.z, thrusters[3].transform.position);
        //}
        //print(force);
    }
    void forward_force(float force){
        var forceDirection_front = new Vector3(force, 0, 0);
        var forceDirection_back = new Vector3(force, 0, 0);
        m_rigidBody.AddForceAtPosition(forceDirection_front, thrusters[4].transform.position);
        m_rigidBody.AddForceAtPosition(forceDirection_front, thrusters[5].transform.position);
        m_rigidBody.AddForceAtPosition(forceDirection_back, thrusters[6].transform.position);
        m_rigidBody.AddForceAtPosition(forceDirection_back, thrusters[7].transform.position);
    }
    void FixedUpdate(){
        //print(transform.rotation.eulerAngles); // (roll,yaw,pitch)
        //SubmergeDepth = GetComponent<buoyancy_forces>().underwater;
        if (transform.position.y <= SubmergeDepth) {
            submerge_force(0);
            forward_force(strength * m_rigidBody.mass/4);// * m_rigidBody.mass);
        } else {
            submerge_force(-strength * m_rigidBody.mass);
        }
        //forward_force(strength * m_rigidBody.mass);

    }
}
