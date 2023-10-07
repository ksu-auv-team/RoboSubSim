using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderParams : MonoBehaviour
{
    public WaterRandomization waterColor;
    // Start is called before the first frame update
    void Start()
    {
        waterColor = GetComponentInChildren<WaterRandomization>();
        //print(waterColor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerStay(Collider collision){
        //print(collision.gameObject.name);
        //bool everything_changed = false;
        if (waterColor.waterColorChanged) {
            //print(collision.gameObject.name);
            if(collision.gameObject.tag == "WaterColorDependent") {
                var light_ray = collision.gameObject.transform.Find("front_camera/light_rays");
                if (light_ray != null) {
                    var ray_particle = light_ray.GetComponent<ParticleSystem>().main;
                    var ray_color = waterColor.waterScript.scatteringColor.maxColorComponent;
                    ray_particle.startColor = new Color(ray_color, ray_color, ray_color, 0.45f);
                    //print(ray_particle.startColor.color);
                    //waterColor.waterColorChanged = false;   // TODO: make this after all objects in collision instead of after first
                }
            }
        }
    }
}
