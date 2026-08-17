using UnityEngine;

namespace SAH
{
    public class MetaPlacementInput : MonoBehaviour, ISpatialAnchorPlacementInput
    {
        public bool TryGetPose(out Pose pose)
        {
            Vector3 position = OVRInput.GetLocalControllerPosition(
                OVRInput.Controller.RTouch);

            Quaternion rotation = OVRInput.GetLocalControllerRotation(
                OVRInput.Controller.RTouch);

            pose = new Pose(position, rotation);

            return true;
        }

        public bool WasConfirmedThisFrame()
        {
            return OVRInput.GetDown(
                OVRInput.Button.One,
                OVRInput.Controller.Hands);
        }
    }
}