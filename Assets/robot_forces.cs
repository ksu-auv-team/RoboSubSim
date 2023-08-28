using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robot_forces : MonoBehaviour
{
    public float waterheight = 0;

    Rigidbody m_Rigidbody;
    bool underwater;

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
        float difference = transform.position.y - waterheight;

    }
}
