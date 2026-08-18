using System.Linq;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace SAH
{
    public class GizmoScaleHandle : GizmoHandle
    {
        public Gizmo gizmo;

        [SerializeField] private GizmoHandleAxis axis;
        [SerializeField] private HandGrabInteractable _handGrabInteractable;

        [SerializeField] private float sensitivity = 1.0f;
        [SerializeField] private float minScale = 0.01f;

        public Transform _io;

        private bool _isActive = false;
        private Vector3 lastHandPos;

        void Update()
        {
            if (!_isActive || _io == null || gizmo == null || gizmo.target == null)
                return;

            Vector3 handDelta = _io.position - lastHandPos;

            Vector3 scaleDelta = Vector3.zero;

            switch (axis)
            {
                case GizmoHandleAxis.X:
                    {
                        float movement = Vector3.Dot(
                            handDelta,
                            gizmo.target.right
                        );

                        scaleDelta.x = movement * sensitivity;
                        break;
                    }

                case GizmoHandleAxis.Y:
                    {
                        float movement = Vector3.Dot(
                            handDelta,
                            gizmo.target.up
                        );

                        scaleDelta.y = movement * sensitivity;
                        break;
                    }

                case GizmoHandleAxis.Z:
                    {
                        float movement = Vector3.Dot(
                            handDelta,
                            gizmo.target.forward
                        );

                        scaleDelta.z = movement * sensitivity;
                        break;
                    }

                case GizmoHandleAxis.ALL:
                    {
                        // Use the hand's movement relative to the gizmo center.
                        // Moving away from the center scales up.
                        // Moving toward the center scales down.

                        Vector3 center = gizmo.target.position;

                        Vector3 previousHandPosition = lastHandPos;
                        Vector3 currentHandPosition = _io.position;

                        float previousDistance =
                            Vector3.Distance(previousHandPosition, center);

                        float currentDistance =
                            Vector3.Distance(currentHandPosition, center);

                        float movement =
                            currentDistance - previousDistance;

                        float uniformDelta =
                            movement * sensitivity;

                        scaleDelta = new Vector3(
                            uniformDelta,
                            uniformDelta,
                            uniformDelta
                        );

                        break;
                    }
            }

            gizmo.ScaleTarget(scaleDelta, minScale);

            lastHandPos = _io.position;
        }
        public void OnGrab()
        {
            if (gizmo == null)
                return;

            _isActive = gizmo.TryActivateHandle(this);

            if (!_isActive)
                return;

            if (_handGrabInteractable == null)
                return;

            _io = _handGrabInteractable
                .SelectingInteractors
                .First()
                .WristPoint;

            lastHandPos = _io.position;
        }

        public void OnRelease()
        {
            if (!_isActive)
                return;

            _isActive = false;
        }
    }
}