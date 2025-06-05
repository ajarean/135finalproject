using UnityEngine;

public class MirrorCamera : MonoBehaviour
{
    [Header("User References")]
    [Tooltip("The user's actual VR headset transform.")]
    public Transform vrHead;

    [Header("Mirror Setup")]
    [Tooltip("Transform of the object acting as the mirror plane. Its local .forward (blue Z-axis) should be the normal of the mirror's reflective surface, pointing OUT from the mirror towards where the user is.")]
    public Transform mirrorPlane;

    [Header("Mirrored Avatar References")]
    [Tooltip("The root Transform of the avatar that will act as the reflection. This is equivalent to the 'modelRoot' in your HeadCamera script.")]
    public Transform mirroredAvatarModelRoot;

    [Tooltip("Offset from the mirrored head position to the avatar's root. Should generally match the value used for the primary avatar.")]
    public Vector3 headToRootOffset = new Vector3(0, -1.6f, 0);

    [Header("Movement Settings")]
    [Tooltip("Speed for smoothing the mirrored avatar's body rotation. Set to 0 or less for no lerping.")]
    public float rotationLerpSpeed = 5f;

    void LateUpdate()
    {
        if (vrHead == null || mirrorPlane == null || mirroredAvatarModelRoot == null)
        {
            // Log warnings only once or if they change, to avoid spamming the console.
            if (vrHead == null && Time.frameCount % 60 == 0) Debug.LogWarning("MirroredAvatar: vrHead not assigned.", this);
            if (mirrorPlane == null && Time.frameCount % 60 == 0) Debug.LogWarning("MirroredAvatar: mirrorPlane not assigned.", this);
            if (mirroredAvatarModelRoot == null && Time.frameCount % 60 == 0) Debug.LogWarning("MirroredAvatar: mirroredAvatarModelRoot not assigned.", this);
            return;
        }

        // --- Define the Mirror Plane ---
        Vector3 planeNormal = mirrorPlane.forward; // Normal of the reflective surface
        Vector3 planePoint = mirrorPlane.position; // A point on the mirror plane

        // --- 1. Calculate Mirrored Head Position ---
        // Reflects the vrHead's position across the mirror plane.
        // Formula: P_reflected = P - 2 * N * dot(P - Q_on_plane, N)
        Vector3 vectorFromPlaneToHead = vrHead.position - planePoint;
        Vector3 mirroredHeadPosition = vrHead.position - 2 * Vector3.Dot(vectorFromPlaneToHead, planeNormal) * planeNormal;

        // --- 2. Calculate Mirrored Head Rotation (Full 3D) ---
        // Reflect the vrHead's forward and up directions across the mirror plane.
        Vector3 reflectedHeadForward = Vector3.Reflect(vrHead.forward, planeNormal);
        Vector3 reflectedHeadUp = Vector3.Reflect(vrHead.up, planeNormal);
        
        // Create the full 3D rotation for the mirrored head.
        // This represents how the head in the mirror would be oriented.
        Quaternion mirroredHeadFullRotation = Quaternion.LookRotation(reflectedHeadForward, reflectedHeadUp);

        // --- 3. Drive the Mirrored Avatar's Body (similar to your HeadCamera.cs) ---

        // --- Mirrored Body Rotation (Yaw only) ---
        // Project the mirrored head's forward direction onto the horizontal plane (world XZ)
        // to get the direction the mirrored body should face (yaw).
        Vector3 mirroredBodyForwardProjected = reflectedHeadForward; // This is already the world-space forward of the mirrored head
        mirroredBodyForwardProjected.y = 0;
        mirroredBodyForwardProjected.Normalize();

        Quaternion targetMirroredBodyYawRotation = mirroredAvatarModelRoot.rotation; // Default to current
        if (mirroredBodyForwardProjected.sqrMagnitude > 0.001f) // Check for valid direction
        {
            // The body's yaw rotation should be aligned with world's up vector.
            targetMirroredBodyYawRotation = Quaternion.LookRotation(mirroredBodyForwardProjected, Vector3.up);
        }

        // Smoothly rotate the mirrored avatar's body towards this target yaw.
        if (rotationLerpSpeed > 0f)
        {
            mirroredAvatarModelRoot.rotation = Quaternion.Slerp(mirroredAvatarModelRoot.rotation, targetMirroredBodyYawRotation, Time.deltaTime * rotationLerpSpeed);
        }
        else
        {
            mirroredAvatarModelRoot.rotation = targetMirroredBodyYawRotation; // No lerping
        }

        // --- Mirrored Body Position ---
        // Apply the headToRootOffset from the mirrored head's position.
        // The offset is transformed by the body's target yaw rotation.
        Vector3 bodyOffsetFromMirroredHead = targetMirroredBodyYawRotation * headToRootOffset;
        mirroredAvatarModelRoot.position = mirroredHeadPosition + bodyOffsetFromMirroredHead;
        
        // Optional: For more precise head articulation on the mirrored avatar (if it has a separate head bone):
        // Transform headBone = mirroredAvatarModelRoot.Find("Path/To/HeadBone"); // Find your head bone
        // if (headBone != null)
        // {
        //     headBone.rotation = mirroredHeadFullRotation;
        // }
    }
}