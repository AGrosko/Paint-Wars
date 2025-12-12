using UnityEngine;

public class PlayerHelper : MonoBehaviour
{
    public PlayerController parentController;

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision detected in PlayerHelper with " + other.gameObject.name);
        parentController?.HandleCollision(other);
    }


}
