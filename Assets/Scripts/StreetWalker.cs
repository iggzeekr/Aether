using UnityEngine;

public class StreetWalker : MonoBehaviour
{
    public Vector3 Direction = Vector3.forward;
    public float Speed = 1.35f;
    public float Limit = 110f;
    public bool Paused;

    void Update()
    {
        if (Paused)
            return;

        transform.position += Direction * Speed * Time.deltaTime;
        if (Direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);

        Vector3 position = transform.position;
        if (Mathf.Abs(Direction.x) > Mathf.Abs(Direction.z))
        {
            if (position.x < -Limit || position.x > Limit)
                Direction = -Direction;
        }
        else if (position.z < -Limit || position.z > Limit)
        {
            Direction = -Direction;
        }
    }
}
