using UnityEngine;
using System.Collections;

/// <summary>
/// script to move object in 3d space
/// with the mouse position x,y of the object is affected, the depth can be changed with the keys "w"(to back) and "s" (to front)
/// attach script to the object you want to move
/// </summary>
public class LeapSimulation : MonoBehaviour {
    private int scaling = 50;
    float currentdepth;
	// Use this for initialization
	void Start () {
        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y,0f);
	}
	
	// Update is called once per frame
	void Update () {
        currentdepth = transform.position.z;
        if (Input.GetKey("w"))
        {
            transform.position = new Vector3((Input.mousePosition.x-Screen.width/2)/scaling, (Input.mousePosition.y-Screen.height/2)/scaling, currentdepth+0.1f);
        }else if (Input.GetKey("s"))
        {
            transform.position = new Vector3((Input.mousePosition.x - Screen.width / 2) / scaling, (Input.mousePosition.y - Screen.height / 2) / scaling, currentdepth - 0.1f);
        }
        else
        {
            transform.position = new Vector3((Input.mousePosition.x - Screen.width / 2) / scaling, (Input.mousePosition.y - Screen.height / 2) / scaling, currentdepth);
        }
	}
}
