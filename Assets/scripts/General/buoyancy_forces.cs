using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buoyancy_forces : MonoBehaviour
{
    public float waterHeight = 0f;
    public float waterDensity = 1.025f; // kg/L
    public float volumeDisplaced = 0;
    Rigidbody m_Rigidbody;
    [HideInInspector] public bool underwater;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate(){
        //print(m_Rigidbody.centerOfMass);
        float difference = transform.position.y + m_Rigidbody.centerOfMass.y - waterHeight;
        if (difference < 0){
            underwater = true;
        } else {
            underwater = false;
        }
        if (underwater){
            float buoyancy_force = waterDensity * Physics.gravity.y * volumeDisplaced; // F = density * gravity accel * Volume
            //print(buoyancy_force);
            //print(Physics.gravity);
            m_Rigidbody.AddForce(new Vector3(0, -buoyancy_force, 0));
        }
    }
}
