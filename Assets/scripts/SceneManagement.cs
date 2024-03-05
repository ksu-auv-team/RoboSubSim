using System; // Add this line if it's missing
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SceneManagement : MonoBehaviour
{
    private RobotForce robotForce;

    float d1 = Mathf.Sqrt((0.5f * 0.5f) + (0.3f * 0.3f)) * Mathf.Sin(Mathf.Deg2Rad * 45f);
    float d2 = Mathf.Sqrt((0.25f * 0.25f) + (0.3f * 0.3f));
    float[][] motors1_4 = new float[3][];
    float[][] motors5_8 = new float[3][];

    private void Start()
    {
        motors1_4[0] = new float[] { d1, d1, -d1, -d1 };
        motors1_4[1] = new float[] { d1, -d1, -d1, d1 };
        motors1_4[2] = new float[] { d1, d1, d1, d1 };

        motors5_8[0] = new float[] { d2, d2, d2, d2 };
        motors5_8[1] = new float[] { -d2, -d2, d2, d2 };
        motors5_8[2] = new float[] { d2, -d2, -d2, d2 };

        // If RobotForce is attached to the same GameObject as this script
        robotForce = GetComponent<RobotForce>();

        // If RobotForce is attached to a different GameObject named "Robot"
        // robotForce = GameObject.Find("Robot").GetComponent<RobotForce>();

        if (robotForce == null)
        {
            Debug.LogError("RobotForce component not found.");
        }
    }

    public float[] ComputeMovement(Dictionary<string, float> data)
    {
        float[] values = new float[8];
        if (Math.Abs(data["X"]) >= 0.1f || Math.Abs(data["Y"]) >= 0.1f || Math.Abs(data["Yaw"]) >= 0.1f)
        {
            for (int i = 0; i < 4; i++)
            {
                values[i] = Mathf.Round(data["X"] * motors1_4[0][i] * 100) / 100f;
                values[i] += Mathf.Round(data["Y"] * motors1_4[1][i] * 100) / 100f;
                values[i] += Mathf.Round(data["Yaw"] * motors1_4[2][i] * 100) / 100f;
            }
        }

        if (Math.Abs(data["Z"]) >= 0.1f || Math.Abs(data["Pitch"]) >= 0.1f || Math.Abs(data["Roll"]) >= 0.1f)
        {
            for (int i = 0; i < 4; i++)
            {
                values[i + 4] = Mathf.Round(data["Z"] * motors5_8[0][i] * 100) / 100f;
                values[i + 4] += Mathf.Round(data["Pitch"] * motors5_8[1][i] * 100) / 100f;
                values[i + 4] += Mathf.Round(data["Roll"] * motors5_8[2][i] * 100) / 100f;
            }
        }

        // Map the values to the correct range [-1, 1]
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Map(values[i], -0.5f, 0.5f, -1f, 1f);
        }

        return values;
    }

    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    private float[] MapAxis(float[] joy_data, bool[] button_data) {
        float[] mapped_data = new float[6];
        mapped_data[0] = joy_data[0]; // Left Y = Z
        mapped_data[1] = joy_data[1]; // Left X = Rz
        if (button_data[0]) {
            mapped_data[4] = joy_data[2]; // Right Y = Y
            mapped_data[5] = joy_data[3]; // Right X = X
        } else {
            mapped_data[2] = joy_data[2]; // Right Y = Rx
            mapped_data[3] = joy_data[3]; // Right X = Ry
        }
        return mapped_data;
    }

    void Update()
    {
        float Left_Y = Input.GetAxis("Vertical2");
        float Left_X = Input.GetAxis("Horizontal2");
        float Right_Y = Input.GetAxis("Vertical");
        float Right_X = Input.GetAxis("Horizontal");
        bool JoyButton0 = Input.GetButton("Fire1");
        bool JoyButton1 = Input.GetButton("Fire2");
        bool JoyButton2 = Input.GetButton("Fire3");
        float[] axis = {Left_Y, Left_X, Right_Y, Right_X};
        bool[] buttons = {JoyButton0, JoyButton1, JoyButton2};
        float[] mapped_data = MapAxis(axis, buttons);
        
        // Convert mapped data to thruster values
        float[] thrusterValues = ComputeMovement(new Dictionary<string, float>
        {
            {"X", mapped_data[3]}, // Assign correctly based on your mapping
            {"Y", mapped_data[2]},
            {"Z", mapped_data[0]},
            {"Pitch", mapped_data[4]},
            {"Roll", mapped_data[5]},
            {"Yaw", mapped_data[1]}
        });

        string debugstring = "Left_Y: " + Left_Y + " Left_X: " + Left_X + 
                      " Right_Y: " + Right_Y + " Right_X: " + Right_X + 
                      " JoyButton0: " + JoyButton0 + " JoyButton1: " + JoyButton1 +
                      " JoyButton2: " + JoyButton2 + " |    Mapped: " + mapped_data[0] +
                        " " + mapped_data[1] + " " + mapped_data[2] + " " + mapped_data[3] +
                        " " + mapped_data[4] + " " + mapped_data[5] + " |    Thrusters: " + thrusterValues[0] +
                        " " + thrusterValues[1] + " " + thrusterValues[2] + " " + thrusterValues[3] +
                        " " + thrusterValues[4] + " " + thrusterValues[5] + " " + thrusterValues[6] +
                        " " + thrusterValues[7];
        Debug.Log(debugstring);

        // robotForce.set_thrusts_strengths(thrusterValues);
        robotForce.set_thrusts_strengths(thrusterValues);
    }
}