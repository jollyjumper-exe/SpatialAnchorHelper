using System;
using UnityEngine.XR.ARFoundation;

namespace SAH
{
    public enum SpatialAnchorBackendType
    {
        Meta,
        ARFoundation
    }

    public static class SpatialAnchorBackendFactory
    {
        public static ISpatialAnchorBackend Create(
            SpatialAnchorBackendType backendType,
            ARAnchorManager arAnchorManager)
        {
            switch (backendType)
            {
                case SpatialAnchorBackendType.Meta:
                    return new MetaSpatialAnchorBackend();

                case SpatialAnchorBackendType.ARFoundation:
                    if (arAnchorManager == null)
                    {
                        throw new InvalidOperationException(
                            "The AR Foundation backend requires an ARAnchorManager.");
                    }

                    return new ARFoundationSpatialAnchorBackend(arAnchorManager);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(backendType),
                        backendType,
                        "Unsupported spatial-anchor backend.");
            }
        }
    }
}
