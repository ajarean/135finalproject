using UnityEngine;

public class VRMirrorController : MonoBehaviour
{
    public Transform centerEyeAnchor;   // OVRCameraRig/TrackingSpace/CenterEyeAnchor
    public Transform mirrorHead;        // The cube/head object

    private Vector3 resetPosition = new Vector3(0f, 0.675f, 0f);
    private bool isMirroring = false;

    // Tracking toggle state
    private bool positionTrackingDisabled = false;
    private bool rotationTrackingDisabled = false;
    private Vector3 frozenLocalPosition;
    private Quaternion frozenLocalRotation;

    void Update()
    {
        HandleInput();
        ApplyTrackingOverride();
        UpdateMirrorHead();
    }

    void HandleInput()
    {
        // Left trigger → reset position
        if (OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) >= 0.5f)
        {
            transform.position = resetPosition;
            Debug.Log($"[CS135] Camera reset to: {transform.position}");
        }

        // A button → flip 180°
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            transform.Rotate(0, 180f, 0);
            Debug.Log($"[CS135] Camera rotated to: {transform.rotation.eulerAngles}");
        }

        // B button → toggle mirror mode
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            isMirroring = !isMirroring;
            Debug.Log($"[CS135] Mirror mode toggled: {isMirroring}");
        }

        if (OVRInput.GetDown(OVRInput.Button.Three)) // X
        {
            positionTrackingDisabled = !positionTrackingDisabled;
            if (positionTrackingDisabled) 
            {
                frozenLocalPosition = centerEyeAnchor.position;
                Debug.Log($"Position tracking disabled: {positionTrackingDisabled}");
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.Four)) // Y
        {
            rotationTrackingDisabled = !rotationTrackingDisabled;
            if (rotationTrackingDisabled) 
            {
                frozenLocalRotation = centerEyeAnchor.rotation;
                Debug.Log($"Rotation tracking disabled: {rotationTrackingDisabled}");
            }
        }
    }

    void ApplyTrackingOverride()
    {
        // Apply inverse of tracking delta to freeze apparent motion
        if (positionTrackingDisabled)
        {
            // Calculate the movement delta of the centerEyeAnchor
            Vector3 deltaPosition = centerEyeAnchor.position - frozenLocalPosition;

            // Apply the inverse delta to the parent's position to counteract the movement
            transform.position -= deltaPosition;  // Subtract delta to lock position

            // Optionally, store the current local position for future deltas
            frozenLocalPosition = centerEyeAnchor.position;
        }

        if (rotationTrackingDisabled)
        {
            // Calculate the delta rotation of the centerEyeAnchor
            Quaternion deltaRotation = centerEyeAnchor.rotation * Quaternion.Inverse(frozenLocalRotation);

            // Apply the inverse delta to the parent's rotation to counteract the movement
            transform.rotation = Quaternion.Inverse(deltaRotation) * transform.rotation;

            // Optionally, store the current local rotation for future deltas
            frozenLocalRotation = centerEyeAnchor.rotation;
        }
    }

    void UpdateMirrorHead()
    {
        if (mirrorHead == null || centerEyeAnchor == null) return;

        Vector3 camera_current_pos = centerEyeAnchor.position;
        Vector3 mirror_target_pos = camera_current_pos;

        // ----- Position Mirroring -----
        if (!positionTrackingDisabled)
        {
            if (isMirroring)
            {
                // Mirror behavior
                mirror_target_pos.z = 2 - camera_current_pos.z;
                mirror_target_pos.x = camera_current_pos.x - 0.2f;
            }
            else
            {
                // Replica behavior (match movement)
                mirror_target_pos += Vector3.forward * 2;
            }
        }
        else
        {
            mirror_target_pos = mirrorHead.position; // Hold current position
        }

        // ----- Rotation Mirroring -----
        Vector3 camera_current_rot = centerEyeAnchor.eulerAngles;
        Vector3 mirror_target_rot = camera_current_rot;

        if (!rotationTrackingDisabled)
        {
            if (isMirroring)
            {
                mirror_target_rot.y *= -1;
                mirror_target_rot.z *= -1;
                mirror_target_rot.y += 180f;
            }
            else
            {
                // mirror_target_rot.y *= -1;
                // mirror_target_rot.x *= -1;
            }
        }

        // ----- Apply Transform -----
        mirrorHead.transform.SetPositionAndRotation(
            mirror_target_pos,
            Quaternion.Euler(mirror_target_rot)
        );

        // Adjust for cube's default forward facing direction
        mirrorHead.transform.Rotate(0, -90f, 0);
    }
}
