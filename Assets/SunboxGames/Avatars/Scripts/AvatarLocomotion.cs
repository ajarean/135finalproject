using UnityEngine;

namespace Sunbox.Avatars {

    [RequireComponent(typeof(CharacterController))] // Ensures CharacterController is present
    public class AvatarLocomotion : MonoBehaviour {
        
        public float MovementAcceleration = 1f;
        public float MovementDamping = 1f;
        public float MoveSpeed = 5f; // Speed of the avatar

        private AvatarCustomization _avatar;
        private CharacterController _controller; // Reference to the CharacterController

        public Vector2 _inputVector;

        void Start()  {
            _avatar = GetComponent<AvatarCustomization>();
            _controller = GetComponent<CharacterController>(); // Get the CharacterController
        }

        void Update()  {
            // --- Input and Animation Parameter Logic (Same as before) ---
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Apply damping
            _inputVector.x = Mathf.MoveTowards(_inputVector.x, 0, Time.deltaTime * MovementDamping);
            _inputVector.y = Mathf.MoveTowards(_inputVector.y, 0, Time.deltaTime * MovementDamping);

            // Apply acceleration
            _inputVector.x += MovementAcceleration * Time.deltaTime * horizontalInput;
            _inputVector.y += MovementAcceleration * Time.deltaTime * verticalInput;

            // Clamp input vector
            _inputVector.x = Mathf.Clamp(_inputVector.x, -1, 1);
            _inputVector.y = Mathf.Clamp(_inputVector.y, -1, 1);

            _avatar.Animator.SetFloat("MoveX", _inputVector.x);
            _avatar.Animator.SetFloat("MoveY", _inputVector.y);

            // --- Actual Movement ---
            // Create a movement vector based on the input.
            // This assumes forward movement is along the Z-axis and sideways along X.
            // You might need to adjust this based on your camera setup (e.g., camera-relative movement).
            Vector3 moveDirection = new Vector3(_inputVector.x, 0, _inputVector.y); 
            
            // Make movement world-relative or camera-relative if needed
            // For example, to make it relative to the camera:
            // moveDirection = Camera.main.transform.TransformDirection(moveDirection);
            // moveDirection.y = 0; // Keep movement on the ground plane

            moveDirection.Normalize(); // Ensure consistent speed regardless of diagonal input

            // Apply movement
            _controller.Move(moveDirection * MoveSpeed * Time.deltaTime);

            // Optional: Add gravity if your character isn't grounded by other means
            if (!_controller.isGrounded) {
                _controller.Move(Physics.gravity * Time.deltaTime);
            }
        }

        public void Dance(){
            _avatar.Animator.SetTrigger("Dance01");
        }

        public void Wave(){
            _avatar.Animator.SetTrigger("Wave");
        }

        public void Clap(){
            _avatar.Animator.SetTrigger("Clap");
        }
    }
}