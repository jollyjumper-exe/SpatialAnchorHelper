using UnityEngine;

namespace SAH
{
    public interface ISpatialAnchorPlacementInput
    {
        bool TryGetPose(out Pose pose);
        bool WasConfirmedThisFrame();
    }
}