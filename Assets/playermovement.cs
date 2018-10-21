using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermovement : MonoBehaviour {
    public Rigidbody rb;
    public float forwardForce = 2000f;
    public float sidewwaysForce = 500f;
	// Use this for initialization
	
	
	// Update is called once per frame
	void FixedUpdate () {
        //Add a forward force
        rb.AddForce(0, 0, forwardForce * Time.deltaTime);
        if (Input.GetKey("d"))
        {
            rb.AddForce(sidewwaysForce * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey("a"))
        {
            rb.AddForce(-sidewwaysForce * Time.deltaTime, 0, 0);
        }
    }
}
