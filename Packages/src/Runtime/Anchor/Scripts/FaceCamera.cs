using UnityEngine;

namespace SpatialAnchorHelper
{
    public class FaceCamera : MonoBehaviour
    {
        private Transform mainCamera;

        void Start()
        {
            mainCamera = Camera.main.transform;
        }

        void LateUpdate()
        {
            transform.LookAt(mainCamera);
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.position);
        }
    }
}