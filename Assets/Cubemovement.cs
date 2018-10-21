using UnityEngine;

public class CubeMovement : MonoBehaviour {

    public Rigidbody RB;

    public float forwardForce = 2000f;
    public float sidewayForce = 500f;

    //这行只是注释，没有功能

    void FixedUpdate()
    {
        RB.AddForce(0, 0, forwardForce * Time.deltaTime);
        
        if ( Input.GetKey("d"))
        {
            RB.AddForce(sidewayForce * Time.deltaTime, 0,0);
        }
        if (Input.GetKey("a"))
        {
            RB.AddForce(-sidewayForce * Time.deltaTime, 0,0);
        }
    }
}
