using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagement : MonoBehaviour
{
    string[] tagNames = {"Robot", "Pool", "2023Objective", "Environment"};
    List<GameObject[]> gameObjects = new List<GameObject[]>();
    int tagSelects;
    int[] objectSelects;
    // Start is called before the first frame update
    void Start()
    {
        registerAllSceneObjects();
        displayAllRegisteredObjects();
        
    }

    public void registerAllSceneObjects(){
        //int tagNum = 0;
        gameObjects.Clear();
        foreach (string tag in tagNames){
            gameObjects.Add(GameObject.FindGameObjectsWithTag(tag));
        }
    }
    public void displayAllRegisteredObjects(){
        int tagNum = 0;
        foreach (GameObject[] objects in gameObjects){
            foreach(GameObject obj in objects){
                print(tagNames[tagNum] + ": " + obj);
            }
            tagNum+=1;
        }
    }
    void copyNewObject(GameObject existingObject){
        GameObject newObject = Instantiate( existingObject, 
                                            existingObject.transform.position, 
                                            existingObject.transform.rotation);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
