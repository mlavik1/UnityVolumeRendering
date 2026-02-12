using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Camera controller for sample scene.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // Normal movement speed
        public float movementSpeed = 1.2f;
        // Rotation speed
        public float rotationSpeed = 2.0f;
        // Mouse wheel scroll speed
        public float scrollSpeed = 100.0f;
        // Multiplied applied to movement speed shen shift key is held down
        public float shiftSpeedMultiplier = 3.0f;
        // Speed at which to interpolate between movement positions and directions
        public float smoothingSpeed = 15.0f;

        private Vector3 positionDelta = Vector3.zero;
        private Vector2 rotationDelta = Vector3.zero;

        private void Update()
        {
            float actualMovementSpeed = movementSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftSpeedMultiplier : 1.0f);

            Vector3 movementDir = Vector3.zero;
            movementDir.z += Input.GetKey(KeyCode.W) ? actualMovementSpeed : 0.0f;
            movementDir.z -= Input.GetKey(KeyCode.S) ? actualMovementSpeed : 0.0f;
            movementDir.x += Input.GetKey(KeyCode.D) ? actualMovementSpeed : 0.0f;
            movementDir.x -= Input.GetKey(KeyCode.A) ? actualMovementSpeed : 0.0f;
            movementDir.y += Input.GetKey(KeyCode.E) ? actualMovementSpeed : 0.0f;
            movementDir.y -= Input.GetKey(KeyCode.Q) ? actualMovementSpeed : 0.0f;
            movementDir.z += Input.mouseScrollDelta.y * scrollSpeed;

            Vector3 worldMovementDir = transform.TransformDirection(movementDir);
            Vector3 targetPositionDelta = worldMovementDir * movementSpeed * Time.deltaTime;
            this.positionDelta = Vector3.Lerp(this.positionDelta, targetPositionDelta, Time.deltaTime * smoothingSpeed);
            transform.position += this.positionDelta;

            Vector2 mousePosition = Input.mousePosition;
            if (Input.GetMouseButton(1))
            {
                Vector2 targetRotationDelta = new Vector2(Input.GetAxis("Mouse X") * rotationSpeed, -Input.GetAxis("Mouse Y") * rotationSpeed);
                this.rotationDelta = Vector2.Lerp(this.rotationDelta, targetRotationDelta, Time.deltaTime * smoothingSpeed);
                transform.Rotate(new Vector3(this.rotationDelta.y, 0.0f, 0.0f), Space.Self);
                transform.Rotate(new Vector3(0.0f, this.rotationDelta.x, 0.0f), Space.World);
            }

        }
    }
}
