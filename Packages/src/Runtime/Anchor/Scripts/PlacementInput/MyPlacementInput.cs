using UnityEngine;
using UnityEngine.InputSystem;

namespace SAH
{
    public class MyPlacementInput : MonoBehaviour, ISpatialAnchorPlacementInput
    {
        [SerializeField] private Transform rightHand;
        [SerializeField] private InputActionReference selectAction;

        public bool TryGetPose(out Pose pose)
        {
            pose = new Pose(rightHand.position, rightHand.rotation);
            return rightHand.gameObject.activeInHierarchy;
        }

        public bool WasConfirmedThisFrame()
        {
            return selectAction.action.WasPressedThisFrame();
        }
    }
}