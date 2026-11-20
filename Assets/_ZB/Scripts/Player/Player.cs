using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public Camera Camera { get; private set; }

    public Vector3 PlayerPosition => transform.position;
    public Quaternion PlayerRotation => transform.rotation;
}
