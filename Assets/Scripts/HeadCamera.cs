using UnityEngine;

public class HeadCamera : MonoBehaviour
{
    public Transform vrHead;       // Your headset transform
    public Transform modelRoot;    // The root of your 3D model
    public Vector3 headToRootOffset = new Vector3(0, -1.6f, 0); // Adjust height if needed
    public float rotationLerpSpeed = 5f;

    void LateUpdate()
    {
        if (vrHead && modelRoot)
        {
            // Match position
            modelRoot.position = vrHead.position + vrHead.TransformVector(headToRootOffset);

            // Smoothly rotate body to face same horizontal direction as the headset
            Vector3 flatForward = Vector3.ProjectOnPlane(vrHead.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatForward, Vector3.up);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
            }
        }
    }
}

