using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyForces : MonoBehaviour
{
    public float waterHeight = 0f;
    public float waterDensity = 1.025f; // kg/L
    public float volumeDisplaced = 0;
    Rigidbody m_Rigidbody;
    public bool forceVisual = true;
    public Material forceVisualMaterial;
    [HideInInspector] public bool underwater;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        if (forceVisualMaterial != null){
            transform.Find("env_force_visual").gameObject.GetComponent<MeshRenderer>().material = forceVisualMaterial;
        }
        if (!forceVisual){
            transform.Find("env_force_visual").gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate(){
        //print(m_Rigidbody.centerOfMass);
        float difference = transform.position.y + m_Rigidbody.centerOfMass.y - waterHeight;
        float buoyancy_force = 0;
        if (difference < 0){
            underwater = true;
        } else {
            underwater = false;
        }
        if (underwater){
            buoyancy_force = waterDensity * Physics.gravity.y * volumeDisplaced; // F = density * gravity accel * Volume
            //print(buoyancy_force);
            //print(Physics.gravity);
            m_Rigidbody.AddForce(new Vector3(0, -buoyancy_force, 0));
        }
        if (forceVisual){
            float environment_force_diff = (-buoyancy_force + m_Rigidbody.mass * Physics.gravity.y)/Mathf.Abs(Physics.gravity.y);
            //print(environment_force_diff);
            var force_visual = transform.Find("env_force_visual");
            if (force_visual != null){
                force_visual.localScale = new Vector3(environment_force_diff/10,environment_force_diff/10,environment_force_diff/2);
            }
        }
    }
}
