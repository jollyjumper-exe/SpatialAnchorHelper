using UnityEngine;

public class MenuFollowCamera : MonoBehaviour
{
    [Header("Distance in front of camera")]
    public float distanceFromCamera = 2f;

    [Header("Smooth Movement")]
    public float followSpeed = 5f;

    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
            Debug.LogWarning("No Main Camera found in the scene!");
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }
}
