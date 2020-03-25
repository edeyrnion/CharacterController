using UnityEngine;

namespace Yaduu.Characters
{
    [RequireComponent(typeof(ThirdPersonController))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        [SerializeField] private Transform _camera;

        private ThirdPersonController _characterController;
        private Vector3 _cameraForward;
        private Vector3 _moveVector;
        private bool _isSprinting;

        private void Start()
        {
            if (_camera == null)
            {
                if (Camera.main != null)
                {
                    _camera = Camera.main.transform;
                }
                else
                {
                    Debug.LogWarning("[" + gameObject.name + "] [" + GetType().Name + "] No camera assigned and no main camera found. Camera relative controlls switched off.");
                }
            }

            _characterController = GetComponent<ThirdPersonController>();
        }

        private void FixedUpdate()
        {
            float moveH = Input.GetAxis("Horizontal");
            float moveV = Input.GetAxis("Vertical");

            if (_camera != null)
            {
                _cameraForward = Vector3.Scale(_camera.forward, new Vector3(1, 0, 1)).normalized;
                _moveVector = moveV * _cameraForward + moveH * _camera.right;
            }
            else
            {
                _moveVector = moveV * Vector3.forward + moveH * Vector3.right;
            }

            _isSprinting = false;
            if (Input.GetKey(KeyCode.Space))
            {
                _isSprinting = true;
            }

            _characterController.Move(_moveVector, _isSprinting);
        }
    }
}
