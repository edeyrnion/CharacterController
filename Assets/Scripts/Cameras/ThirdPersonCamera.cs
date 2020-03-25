using UnityEngine;

namespace Yaduu.Cameras
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] private LayerMask _collisionLayerMask;
        [SerializeField] private bool _advancedCollision = false;

        public Transform Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public Vector3 TargetOffset
        {
            get { return _targetOffset; }
            set { _targetOffset = value; }
        }

        public LayerMask CollisionLayerMask
        {
            get { return _collisionLayerMask; }
            set { _collisionLayerMask = value; }
        }

        // Camera controll sensitivities.
        private readonly float _xRotationSens = 6.0f;
        private readonly float _yRotationSens = 4.0f;
        private readonly float _scrollSens = 5.5f;

        // Camera movement smoothness.
        private readonly float _rotationSmooth = 0.03f;
        private readonly float _scrollSmooth = 0.2f;

        private class PivotSettings
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public float RotationY = 0.0f;
            public float RotationX = 0.0f;
            public float MinAngleX = -65.0f;
            public float MaxAngleX = 65.0f;
            public float VelocityY;
            public float VelocityX;
            public float DesiredRotationX;
            public float DesiredRotationY;
        }

        private class CameraSettings
        {
            public float Distance = 3f;
            public float MaxDistance = 4f;
            public float MinDistance = 1.5f;
            public float ZoomVelocity;
            public float AdjustVelocity;
            public float DesiredDistance;
            public float AdjustedDistance;
        }

        private PivotSettings _pivot = new PivotSettings();
        private CameraSettings _camera = new CameraSettings();

        // Camera collision.
        private CameraOcclusion _occlusion;

        private void Start()
        {
            if (_target == null)
            {
                if (GameObject.FindGameObjectWithTag("Player").transform != null)
                {
                    _target = GameObject.FindGameObjectWithTag("Player").transform;
                }
                else
                {
                    Debug.LogError("[" + gameObject.name + "] [" + GetType().Name + "] No target assigned and no player found.");
                }
            }

            // Set pivot position.
            _pivot.Position = _target.position + _target.TransformDirection(_targetOffset);

            // Set rotation Y offset to match target Y rotation.
            _pivot.DesiredRotationY = _pivot.RotationY += _target.rotation.eulerAngles.y;
            _pivot.DesiredRotationX = _pivot.RotationX;

            _camera.DesiredDistance = _camera.AdjustedDistance = _camera.Distance;

            // Initialize camera collision.
            _occlusion = new CameraOcclusion(GetComponent<Camera>());

            // TODO: Should not be handled here.
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            // TODO: Direct input calls should not be handled here.
            ControllPivotRotation(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            ControllCameraDistance(Input.GetAxisRaw("Mouse ScrollWheel"));

            // Update pivot rotation.
            _pivot.RotationY = Mathf.SmoothDampAngle(_pivot.RotationY, _pivot.DesiredRotationY, ref _pivot.VelocityY, _rotationSmooth);
            _pivot.RotationX = Mathf.SmoothDampAngle(_pivot.RotationX, _pivot.DesiredRotationX, ref _pivot.VelocityX, _rotationSmooth);

            if (_pivot.RotationY > 360)
            {
                _pivot.DesiredRotationY -= 360;
                _pivot.RotationY -= 360;
            }
            else if (_pivot.RotationY < 0)
            {
                _pivot.DesiredRotationY += 360;
                _pivot.RotationY += 360;
            }

            _pivot.Rotation = Quaternion.Euler(_pivot.RotationX, _pivot.RotationY, _pivot.Rotation.z);

            // Update camera distance.
            Vector3 desiredCameraPosition = _pivot.Position + (_pivot.Rotation * (-Vector3.forward * _camera.DesiredDistance));

            float collisionDistance;
            if (_advancedCollision? _occlusion.IsOccluded(_pivot.Position, desiredCameraPosition, out collisionDistance, _collisionLayerMask) : _occlusion.ClipPlaneCast(_pivot.Position, desiredCameraPosition, out collisionDistance, _collisionLayerMask))
            {
                _camera.ZoomVelocity = 0f;
                if (collisionDistance > _camera.AdjustedDistance)
                {
                    _camera.AdjustedDistance = Mathf.SmoothDamp(_camera.AdjustedDistance, collisionDistance, ref _camera.AdjustVelocity, _scrollSmooth);
                }
                else
                {
                    _camera.AdjustVelocity = 0f;
                    _camera.AdjustedDistance = collisionDistance;
                }
            }
            else
            {
                _camera.AdjustVelocity = 0f;
                _camera.AdjustedDistance = Mathf.SmoothDamp(_camera.AdjustedDistance, _camera.DesiredDistance, ref _camera.ZoomVelocity, _scrollSmooth);
            }

            // Update camera postion.
            transform.position = _pivot.Position + (_pivot.Rotation * (-Vector3.forward * _camera.AdjustedDistance));

            // Update camera rotation.
            transform.rotation = _pivot.Rotation;
        }

        private void FixedUpdate()
        {
            Vector3 offset;
            if (_occlusion.IsNearCollision(_target.position + _target.TransformDirection(_targetOffset), out offset, _collisionLayerMask))
            {
                _pivot.Position = Vector3.Lerp(_pivot.Position, _target.position + _target.TransformDirection(_targetOffset) + offset, Time.fixedDeltaTime * 20f);
            }
            else
            {
                // Update pivot position.
                // Target movement handled in a FixedUpdate causes camera jitter, if camera movement is not handled in FixedUpdate too.
                _pivot.Position = Vector3.Lerp(_pivot.Position, _target.position + _target.TransformDirection(_targetOffset), Time.fixedDeltaTime * 20f);
            }
        }

        private void ControllPivotRotation(float axisX, float axisY)
        {
            _pivot.DesiredRotationY += axisX * _xRotationSens;
            _pivot.DesiredRotationX += axisY * -1 * _yRotationSens;
            _pivot.DesiredRotationX = Mathf.Clamp(_pivot.DesiredRotationX, _pivot.MinAngleX, _pivot.MaxAngleX);
        }

        private void ControllCameraDistance(float scroll)
        {
            _camera.DesiredDistance += -1 * scroll * _scrollSens;
            _camera.DesiredDistance = Mathf.Clamp(_camera.DesiredDistance, _camera.MinDistance, _camera.MaxDistance);
        }
    }
}
