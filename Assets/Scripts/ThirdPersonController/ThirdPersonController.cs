using UnityEngine;

namespace Yaduu.Characters
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonController : MonoBehaviour
    {
        private Rigidbody _rBody;
        private Animator _animator;

        private void Start()
        {
            _rBody = GetComponent<Rigidbody>();
            _rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            _animator = GetComponent<Animator>();
        }

        public void Move(Vector3 _moveVector, bool _isSprinting)
        {
            if (_moveVector.magnitude > 1f)
            {
                _moveVector.Normalize();
            }

            if (_isSprinting)
            {
                _animator.SetBool("Sprint", true);
                _animator.SetFloat("Speed", _moveVector.magnitude, 0.2f, Time.deltaTime);
            }
            else
            {
                _animator.SetBool("Sprint", false);
                _animator.SetFloat("Speed", _moveVector.magnitude, 0.2f, Time.deltaTime);
            }

            if (_moveVector != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_moveVector, Vector3.up), Time.deltaTime * 20f);
            }
        }

    }
}