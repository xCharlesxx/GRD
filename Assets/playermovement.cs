using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermovement : MonoBehaviour {
    public Rigidbody rb;
	// Use this for initialization
	
	
	// Update is called once per frame
	void FixedUpdate () {
        rb.AddForce(0, 0, 2000 * Time.deltaTime);
	}
}
