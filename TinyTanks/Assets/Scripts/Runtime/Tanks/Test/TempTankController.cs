using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Testing
{
    public class TempTankController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraOBJ;

        private Rigidbody _rb;
        private Camera _cam;

        [Header("Variables")]
        [SerializeField] private float speed;
        [SerializeField] private float rotationSpeed;

        [Header("MoveInput")]
        private Vector2 _moveVector;


        
        private void Start()
        {
            _rb = gameObject.GetComponent<Rigidbody>();
            SetCamera();
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            _rb.AddForce(Physics.gravity, ForceMode.Acceleration);

            float moveAmount = (_moveVector.y + _moveVector.x) * 0.5f * speed;
            Vector3 move = transform.forward * moveAmount * Time.deltaTime;
            Vector3 modifiedMove = new Vector3(move.x, 0f, move.z);
            _rb.MovePosition(_rb.position + modifiedMove);

            float rotationAmount = (_moveVector.y - _moveVector.x) * rotationSpeed * Time.deltaTime;
            Quaternion turnOffset = Quaternion.Euler(0, rotationAmount, 0);
            _rb.MoveRotation(_rb.rotation * turnOffset);
        }

        private void SetCamera()
        {
            _cam = Camera.main;
            _cam.transform.parent = cameraOBJ;
            _cam.transform.position = cameraOBJ.position;
            _cam.transform.eulerAngles = cameraOBJ.eulerAngles;
        }

        #region InputFunctions

        public void MoveInput(InputAction.CallbackContext context)
        {
            _moveVector = context.ReadValue<Vector2>();
        }

        #endregion
    }
}
