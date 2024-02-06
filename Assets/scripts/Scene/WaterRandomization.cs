using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
public class WaterRandomization : MonoBehaviour
{
    public WaterSurface waterScript;
    private const int BG_SUM_MAX = 224;
    private const int BG_SUM_MIN = 140;
    private const int BG_MAX = (BG_SUM_MAX + BG_SUM_MIN) / 4;
    public float centerVisibility = 5f;
    public float multiplierVisibility = 2.5f;

    //private bool waterColorChanged = false;
    // Start is called before the first frame update
    void Start()
    {
        print(waterScript.scatteringColor);
        print(waterScript.underWaterScatteringColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RandomScatteringColor();
            RandomVisibility();
        }
        //count += 1;
        //if (count > 100) {
        //    RandomScatteringColor();
        //    count = 0;
        //}
        //print(count);
    }
    public void RandomizeWater(){
        RandomScatteringColor();
        RandomVisibility();
    }
    void RandomScatteringColor(){
        int sum_color = Random.Range(BG_SUM_MIN, BG_SUM_MAX);
        int blue = Random.Range(BG_SUM_MIN/2, BG_MAX);
        int green = sum_color - blue;
        float brightness = Random.Range(0.3f, 1.0f);
        SetWaterColor(blue, green, brightness);
        //waterScript.scatteringColor.a = 0.35f;
        //waterColorChanged = true;
    }
    void RandomVisibility(){
        waterScript.absorptionDistance = centerVisibility * Random.Range(0.9f,1.1f); // surface visiblility (meters)
        waterScript.absorptionDistanceMultiplier = multiplierVisibility * Random.Range(0.9f,1.1f); // underwater visibility (multiplier of surface)
    }
    public void SetWaterColor(int blue, int green, float brightness){
        waterScript.scatteringColor.b = blue/255.0f;
        waterScript.scatteringColor.g = green/255.0f;
        waterScript.scatteringColor *= brightness;
    }
}
