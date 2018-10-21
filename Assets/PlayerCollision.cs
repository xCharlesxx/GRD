
using UnityEngine;

public class PlayerCollision : MonoBehaviour {
    public playermovement movement;
void OnCollisionEnter(Collision collisionInfo)
    {
        Debug.Log(collisionInfo.collider.name);
        if (collisionInfo.collider.tag == "wall")
        {
            movement.enabled = false;
        }
    }
}
