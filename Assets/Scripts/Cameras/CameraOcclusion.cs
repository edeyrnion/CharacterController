using System;
using UnityEngine;

namespace Yaduu.Cameras
{
    public class CameraOcclusion
    {
        private readonly float _distanceAdjustment = 0.001f; // Don't set to 0 or lower.(Floating point inaccuracy counter)
        private readonly int _maxRaycastLoops = 100;

        private readonly Camera _camera;
        private float _ncpDistance, _ncpHalfHight, _ncpHalfWide, _ncpHalfDiagonal;
        private Vector3[] _clipPlanePoints;

        public CameraOcclusion(Camera camera)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));

            _camera = camera;

            _clipPlanePoints = new Vector3[4];

            UpdateCamera();
        }

        public void UpdateCamera()
        {
            _ncpDistance = _camera.nearClipPlane;
            _ncpHalfHight = Mathf.Tan((_camera.fieldOfView / 2) * Mathf.Deg2Rad) * _camera.nearClipPlane;
            _ncpHalfWide = _ncpHalfHight * _camera.aspect;

            _ncpHalfDiagonal = Mathf.Sqrt(Mathf.Pow(_ncpHalfHight, 2f) + Mathf.Pow(_ncpHalfWide, 2f));
        }

        private void UpdateClipPlanePoints(Quaternion rotation)
        {
            _clipPlanePoints[0] = rotation * new Vector3(-_ncpHalfWide, _ncpHalfHight, _ncpDistance);
            _clipPlanePoints[1] = rotation * new Vector3(_ncpHalfWide, _ncpHalfHight, _ncpDistance);
            _clipPlanePoints[2] = rotation * new Vector3(_ncpHalfWide, -_ncpHalfHight, _ncpDistance);
            _clipPlanePoints[3] = rotation * new Vector3(-_ncpHalfWide, -_ncpHalfHight, _ncpDistance);
        }

        public bool ClipPlaneCast(Vector3 from, Vector3 to, out float hitDistance, int layerMask = ~0)
        {
            var boxHalfExtendZ = 0;

            var halfExtends = new Vector3(_ncpHalfWide, _ncpHalfHight, boxHalfExtendZ);
            var direction = to - from;
            var center = from + (direction * boxHalfExtendZ);
            var orientation = Quaternion.LookRotation(direction, Vector3.up);
            var maxDistance = Vector3.Distance(from, to) - boxHalfExtendZ - _ncpDistance;

            LabelManager.Instance.NumberOfRayCasts = 1;

            RaycastHit hitinfo;
            if (Physics.BoxCast(center, halfExtends, direction, out hitinfo, orientation, maxDistance, layerMask))
            {
                hitDistance = hitinfo.distance + _ncpDistance;
                return true;
            }

            hitDistance = maxDistance + _ncpDistance;

            return false;
        }

        public bool IsOccluded(Vector3 from, Vector3 to, out float hitDistance, int layerMask = ~0)
        {
            var isOccluded = false;
            var direction = (from - to).normalized;

            var orientation = Quaternion.LookRotation(direction, Vector3.up);

            UpdateClipPlanePoints(orientation);

            hitDistance = Vector3.Distance(from, to);
            
            // DEBUG:
            LabelManager.Instance.NumberOfRayCasts = 1;

            int i = 0;
            while (ClipPlanePointCast(from, to, ref hitDistance, layerMask))
            {
                isOccluded = true;
                to = from - direction * hitDistance;

                // DEBUG:
                LabelManager.Instance.NumberOfRayCasts++;

                if (++i >= _maxRaycastLoops)
                {
                    break;
                }
            }

            return isOccluded;
        }

        private bool ClipPlanePointCast(Vector3 from, Vector3 to, ref float hitDistance, int layerMask = ~0)
        {
            var isColliding = false;

            RaycastHit hitinfo;
            for (int i = 0; i < _clipPlanePoints.Length; i++)
            {
                var point = _clipPlanePoints[i] + to;
                //var nextPoint = _clipPlanePoints[i+1] + to;
                if (Physics.Linecast(from, point, out hitinfo, layerMask))
                {
                    var a = Vector3.Distance(from, point);
                    var b = hitinfo.distance;
                    var c = Vector3.Distance(from, to) - _ncpDistance;

                    var preHitDistance = (c * b / a) + _ncpDistance;

                    if (preHitDistance < hitDistance)
                    {
                        hitDistance = preHitDistance;
                    }

                    isColliding = true;
                }
            }

            if (isColliding)
            {
                hitDistance -= _distanceAdjustment;
            }

            return isColliding;
        }

        public bool IsNearCollision(Vector3 from, out Vector3 offset, int layerMask = ~0)
        {
            offset = Vector3.zero;
            bool isNearCollision = false;
            float distance = _ncpHalfDiagonal + _distanceAdjustment;

            for (int i = 0; i < 360; i = i + 45)
            {
                var direction = Quaternion.Euler(0f, i, 0f) * Vector3.forward;
                Ray ray = new Ray(from, direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, distance, layerMask))
                {
                    offset += direction.normalized * (hit.distance - distance);
                    isNearCollision = true;

                    // DEBUG:
                    Debug.DrawLine(from, from + direction * distance, Color.red);
                }
            }

            // DEBUG:
            LabelManager.Instance.CameraPivotOffset = offset;

            return isNearCollision;
        }
    }
}
