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
    public bool waterColorChanged = false;
    // Start is called before the first frame update
    void Start()
    {
        print(waterScript.scatteringColor);
        print(waterScript.underWaterScatteringColor);
    }

    // Update is called once per frame
    int count = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RandomScatteringColor();          
        }
        //count += 1;
        //if (count > 100) {
        //    RandomScatteringColor();
        //    count = 0;
        //}
        //print(count);
    }
    void RandomScatteringColor(){
        int sum_color = Random.Range(BG_SUM_MIN, BG_SUM_MAX);
        int blue = Random.Range(BG_SUM_MIN/2, BG_MAX);
        int green = sum_color - blue;
        waterScript.scatteringColor.b = blue/255.0f;
        waterScript.scatteringColor.g = green/255.0f;
        float brightness = Random.Range(0.3f, 1.0f);
        waterScript.scatteringColor *= brightness;
        //waterScript.scatteringColor.a = 0.35f;
        waterColorChanged = true;
    }
}
