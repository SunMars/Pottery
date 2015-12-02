using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    public float revolutionsPerSecond;

	// Use this for initialization
	void Start () {
        /*
        Debug.Log(Time.deltaTime.ToString());
        Debug.Log(Time.fixedDeltaTime.ToString());
        */
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(Vector3.up * Time.deltaTime * revolutionsPerSecond * 360);

        if(transform.rotation.y < 320 && transform.rotation.y > 40)
        {
            Debug.Log(Time.deltaTime);
            
        }
	}

}
