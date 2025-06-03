using UnityEngine;

[ExecuteInEditMode] // Optional: to see updates in editor while not in Play mode
public class VRMirrorReflection : MonoBehaviour
{
    [Tooltip("Assign your dedicated Reflection Camera here.")]
    public Camera reflectionCamera;

    [Tooltip("Assign your main VR Camera (e.g., the Camera component under 'XR Rig', or OVRCameraRig's CenterEyeAnchor). Manual assignment is most reliable.")]
    public Camera mainVRCamera;

    void Start()
    {
        // --- Locate the Main VR Camera ---
        // 1. Prioritize manually assigned mainVRCamera in the Inspector
        if (mainVRCamera == null)
        {
            // 2. Try Camera.main (works if your VR camera is tagged "MainCamera" and is active)
            if (Camera.main != null && Camera.main.stereoEnabled)
            {
                mainVRCamera = Camera.main;
                Debug.Log("VRMirrorReflection: Found mainVRCamera via Camera.main: " + mainVRCamera.name);
            }
        }

        // 3. If still not found, try finding a GameObject named "XR Rig" and get a Camera from its children
        if (mainVRCamera == null)
        {
            GameObject xrRigObject = GameObject.Find("XR Rig"); // This is case-sensitive
            if (xrRigObject != null)
            {
                mainVRCamera = xrRigObject.GetComponentInChildren<Camera>(true);
                if (mainVRCamera != null)
                {
                    Debug.Log("VRMirrorReflection: Found mainVRCamera under an object named 'XR Rig': " + mainVRCamera.name);
                }
            }
        }

        // 4. If still not found, try to find an OVRCameraRig (Meta XR SDK specific)
        if (mainVRCamera == null)
        {
            OVRCameraRig ovrCameraRig = FindObjectOfType<OVRCameraRig>();
            if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
            {
                mainVRCamera = ovrCameraRig.centerEyeAnchor.GetComponent<Camera>();
                if (mainVRCamera != null)
                {
                    Debug.Log("VRMirrorReflection: Found mainVRCamera via OVRCameraRig: " + mainVRCamera.name);
                }
            }
        }

        // --- Validate Cameras ---
        if (mainVRCamera == null)
        {
            Debug.LogError("VRMirrorReflection: Main VR Camera NOT FOUND for mirror on '" + gameObject.name +
                           "'. Please assign it manually in the Inspector. Searched Camera.main, 'XR Rig', and OVRCameraRig.");
        }

        if (reflectionCamera == null)
        {
            Debug.LogError("VRMirrorReflection: Reflection Camera not assigned to mirror on '" + gameObject.name +
                           "'. This script cannot function without it. Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        if (reflectionCamera.targetTexture == null)
        {
            Debug.LogWarning("VRMirrorReflection: Reflection Camera on '" + gameObject.name +
                             "' does not have a Target Texture assigned. Reflections will not be visible.");
        }

        // It's good practice to ensure the reflection camera has similar FOV/aspect to the main camera
        // if not copying the entire projection matrix. You can do this once in Start() or before rendering.
        if (mainVRCamera != null && reflectionCamera != null)
        {
            reflectionCamera.fieldOfView = mainVRCamera.fieldOfView;
            reflectionCamera.aspect = mainVRCamera.aspect;
            // Near/Far clipping planes might also need to be considered or matched,
            // but CalculateObliqueMatrix primarily modifies based on the clipPlane argument.
        }
    }

    void OnWillRenderObject()
    {
        if (mainVRCamera == null || reflectionCamera == null || reflectionCamera.targetTexture == null)
            return;

        if (Camera.current != mainVRCamera)
        {
            return;
        }

        Transform mirrorTransform = transform;
        Plane mirrorPlane = new Plane(mirrorTransform.forward, mirrorTransform.position);

        float camDistanceToMirrorPlane = mirrorPlane.GetDistanceToPoint(mainVRCamera.transform.position);
        Vector3 reflectedPosition = mainVRCamera.transform.position - 2.0f * mirrorPlane.normal * camDistanceToMirrorPlane;
        reflectionCamera.transform.position = reflectedPosition;

        Vector3 reflectedLookDirection = Vector3.Reflect(mainVRCamera.transform.forward, mirrorPlane.normal);
        Vector3 reflectedUpDirection = Vector3.Reflect(mainVRCamera.transform.up, mirrorPlane.normal);
        reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectedLookDirection, reflectedUpDirection);

        Vector4 clipPlaneCameraSpace = CalculateCameraSpacePlane(reflectionCamera, mirrorTransform.position, mirrorTransform.forward, 1.0f);
        
        // CORRECTED LINE: Call CalculateObliqueMatrix on the reflectionCamera instance
        reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);

        if (!reflectionCamera.enabled)
        {
            reflectionCamera.enabled = true;
        }
    }

    private Vector4 CalculateCameraSpacePlane(Camera cam, Vector3 worldPos, Vector3 worldNormal, float side)
    {
        // Ensure the normal is normalized
        worldNormal.Normalize();
        // Plane equation: Ax + By + Cz + D = 0. Normal is (A,B,C). D = -Dot(Normal, PointOnPlane)
        // We want the plane in camera's view space.
        Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
        Vector3 viewSpacePos = worldToCameraMatrix.MultiplyPoint3x4(worldPos);
        Vector3 viewSpaceNormal = worldToCameraMatrix.MultiplyVector(worldNormal).normalized * side;
        float viewSpaceD = -Vector3.Dot(viewSpaceNormal, viewSpacePos);
        return new Vector4(viewSpaceNormal.x, viewSpaceNormal.y, viewSpaceNormal.z, viewSpaceD);
    }

    void OnBecameInvisible()
    {
        if (reflectionCamera != null && reflectionCamera.targetTexture != null)
        {
            // reflectionCamera.enabled = false;
        }
    }

    void OnBecameVisible()
    {
        if (reflectionCamera != null && reflectionCamera.targetTexture != null)
        {
            // if (!reflectionCamera.enabled) reflectionCamera.enabled = true;
        }
    }
}