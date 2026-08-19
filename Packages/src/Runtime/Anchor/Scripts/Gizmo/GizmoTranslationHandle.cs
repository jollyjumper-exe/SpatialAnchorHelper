using System.Collections;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace SAH
{
    public class GizmoTranslationHandle : GizmoHandle
    {
        public Gizmo gizmo;
        private bool _isActive = false;

        [SerializeField] private GizmoHandleAxis axis;

        [SerializeField] private HandGrabInteractable _handGrabInteractable;
        public Transform _io;
        private Vector3 lastHandPos;


        void Update()
        {
            if (!_isActive) return;

            Vector3 delta = _io.position - lastHandPos;

            Vector3 localDelta = Vector3.zero;
            switch (axis)
            {
                case GizmoHandleAxis.X:
                    localDelta = Vector3.Project(delta, gizmo.target.right);
                    break;
                case GizmoHandleAxis.Y:
                    localDelta = Vector3.Project(delta, gizmo.target.up);
                    break;
                case GizmoHandleAxis.Z:
                    localDelta = Vector3.Project(delta, gizmo.target.forward);
                    break;
            }

            gizmo.TranslateTarget(localDelta);

            lastHandPos = _io.position;
        }

        public void OnGrab()
        {
            if (gizmo == null) return;
            _isActive = gizmo.TryActivateHandle(this);
            if (!_isActive) return;
            if (_handGrabInteractable == null) return;

            _io = _handGrabInteractable.SelectingInteractors.First().WristPoint;
            lastHandPos = _io.position;
        }

        public void OnRelease()
        {
            if (!_isActive) return;
            _isActive = false;
        }
    }
}
