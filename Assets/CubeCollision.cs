using UnityEngine;

public class CubeCollision : MonoBehaviour {

    public CubeMovement movement;

    void OnCollisionEnter (Collision collisionInfo)
    {
        if (collisionInfo.GetComponent<Collider>().tag == "Obstacle")
        {
            movement.enabled = false;
        }
    }

}
