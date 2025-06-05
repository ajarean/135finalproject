using UnityEngine;

public class OVRMovement : MonoBehaviour
{
    public float speed = 3.0f;
    private CharacterController _characterController;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Move in the direction the HMD is facing, ignoring Y rotation
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = Camera.main.transform.right;
        right.y = 0;
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x) * speed;

        _characterController.Move(move * Time.deltaTime);
    }
}
