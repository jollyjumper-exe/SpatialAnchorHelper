using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Linq;

namespace SAH
{
    public class GizmoRotationHandle : GizmoHandle
    {
        public Gizmo gizmo;
        private bool _isActive = false;

        [SerializeField] private GizmoHandleAxis axis;
        
        [SerializeField] private HandGrabInteractable _handGrabInteractable;
        public Transform _io;
        private Vector3 lastDir;
        private Quaternion initialTargetRot;


        void Update()
        {
            if (!_isActive || axis == null) return;

            Vector3 handPos = _io.position;
            Vector3 targetPos = gizmo.target.position;

            Vector3 currentDir = handPos - targetPos;

            if (currentDir.sqrMagnitude < 0.0001f) return;

            Quaternion deltaRot = Quaternion.FromToRotation(lastDir, currentDir);

            Vector3 worldAxis = Vector3.zero;
            switch (axis)
            {
                case GizmoHandleAxis.X: worldAxis = gizmo.transform.right; break;
                case GizmoHandleAxis.Y: worldAxis = gizmo.transform.up; break;
                case GizmoHandleAxis.Z: worldAxis = gizmo.transform.forward; break;
            }

            deltaRot.ToAngleAxis(out float angle, out Vector3 rotAxis);
            float projectedAngle = Vector3.Dot(rotAxis, worldAxis) * angle;

            Quaternion rotation = Quaternion.AngleAxis(projectedAngle, worldAxis);

            gizmo.RotateTarget(rotation);

            lastDir = currentDir.normalized;
        }

        public void OnGrab()
        {
            if(gizmo == null) return;
            _isActive = gizmo.TryActivateHandle(this);
            if(!_isActive) return;
            if(_handGrabInteractable == null) return;
            
            _io = _handGrabInteractable.SelectingInteractors.First().WristPoint;
            
            Vector3 targetPos = gizmo.target.position;
            lastDir = (_io.position - targetPos).normalized;
            initialTargetRot = gizmo.target.rotation;
        }

        public void OnRelease()
        {
            if(!_isActive) return;
            _isActive = false;
        }
    }
}
