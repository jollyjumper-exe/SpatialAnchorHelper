using UnityEngine;
public class LeftHandPinchMenu : MonoBehaviour
{
    [Header("References")]
    public OVRHand hand;
    public Camera xrCamera;
    public GameObject menu; 
    [SerializeField] bool activeOnStart; 

    [Header("Settings")]
    [Range(0f, 1f)] public float palmFacingThreshold = 0.1f;

    private bool isActive = false;
    private bool processedPinch = false;

    void Awake()
    {
        if (xrCamera == null)
        {
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null) xrCamera = rig.centerEyeAnchor.GetComponent<Camera>();
        }
    }

    void Start()
    {
        if (activeOnStart)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
        
        menu.SetActive(isActive);
    }

    void Update()
    {
        if (hand == null || xrCamera == null || menu == null) return;

        Vector3 palmCenter = transform.position;
        Vector3 palmNormal = hand.PointerPose.up;

        Vector3 toCamera = (xrCamera.transform.position - palmCenter).normalized;
        float dot = Vector3.Dot(palmNormal, toCamera);
        bool isLookingAt = dot > palmFacingThreshold;

        bool isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        if (isPinching && isLookingAt && !processedPinch)
        {
            isActive = !isActive;
            menu.SetActive(isActive);

            Vector3 position = xrCamera.transform.position + xrCamera.transform.forward * 0.5f;
            menu.transform.position = position;

            Vector3 lookDirection = menu.transform.position - xrCamera.transform.position;
            menu.transform.rotation = Quaternion.LookRotation(lookDirection);

            processedPinch = true;
        }

        if (!isPinching && processedPinch) processedPinch = false;
    }
}
