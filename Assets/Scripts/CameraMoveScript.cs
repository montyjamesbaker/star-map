using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMoveScript : MonoBehaviour {
    float moveSpeed = 50f;
    float cameraSensitivity = 0.5f;
    bool cameraLocked = false;  
    Vector3 move = Vector3.zero;
    Vector2 cameraRotation;
    PlayerInput input;
    private void Awake() {
        input = GetComponent<PlayerInput>();
        SetCameraLocked(false);
    }
    public void Move(InputAction.CallbackContext context) {
        move = context.ReadValue<Vector3>().normalized;
    }
    public void RotateCamera(InputAction.CallbackContext context) {
        cameraRotation = context.ReadValue<Vector2>();
    }
    public void Quit(InputAction.CallbackContext context) {
        Application.Quit();
    }
    public void ToggleMode(InputAction.CallbackContext context) {
        if (context.started) {
            if (input.currentActionMap.name == input.defaultActionMap) {
                input.currentActionMap = input.actions.actionMaps[1];
                SetCameraLocked(true);
            }
            else {
                input.currentActionMap = input.actions.actionMaps[0];
                SetCameraLocked(false);
            }
        }
    }
    private void SetCameraLocked(bool locked) {
        if (locked) {
            cameraLocked = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else {
            cameraLocked = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void Update() {
        if (!cameraLocked) {
            // Move the camera
            Vector3 localMove = transform.rotation * move;
            transform.position += localMove * moveSpeed * Time.deltaTime;
            // Rotate the camera
            float newRotationX = transform.localEulerAngles.x - cameraRotation.y * cameraSensitivity;
            float newRotationY = transform.localEulerAngles.y + cameraRotation.x * cameraSensitivity;
            // Clamp the X rotation, so that the player cant look directly up or down
            if (newRotationX > 80 && newRotationX < 180) {
                newRotationX = 80;
            }
            else if (newRotationX < 280 && newRotationX > 180) {
                newRotationX = 280;
            }
            transform.localEulerAngles = new Vector3(newRotationX, newRotationY, 0f);
        }
    }
}