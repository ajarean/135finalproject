using UnityEngine;

public class VRMirror : MonoBehaviour
{
    public Transform centerEyeAnchor;  // Assign in inspector
    private bool isMirroring = false;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two)) // B button
        {
            isMirroring = !isMirroring;
            Debug.Log($"[CS135] B button pressed — Mirror mode: {isMirroring}");
        }

        Vector3 cameraPos = centerEyeAnchor.position;
        Vector3 cameraRot = centerEyeAnchor.eulerAngles;

        Vector3 mirrorTargetPos;
        Vector3 mirrorTargetRot;

        if (isMirroring)
        {
            // Mirror behavior — like a reflection
            mirrorTargetPos = cameraPos;
            mirrorTargetPos.x *= -1;
            mirrorTargetPos.z *= -1;

            mirrorTargetRot = cameraRot;
            mirrorTargetRot.y += 180f;
        }
        else
        {
            // Match behavior — like a puppet
            mirrorTargetPos = cameraPos;
            mirrorTargetRot = cameraRot;
        }

        transform.SetPositionAndRotation(
            mirrorTargetPos,
            Quaternion.Euler(mirrorTargetRot)
        );
    }
}
