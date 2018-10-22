using UnityEngine;

public class CubeCollision : MonoBehaviour {

    public CubeMovement movement;

    void OnCollisionEnter (Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "Obstacle")
        {
            movement.enabled = false;
        }
    }

}
