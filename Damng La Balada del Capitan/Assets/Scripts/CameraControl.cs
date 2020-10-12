using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    [SerializeField] float cameraKeyboardMovementSpeed = 0.2f;
    [SerializeField] float cameraPanMovementSpeed = 5f;
    [SerializeField] float cameraZoomSpeed = 0.5f;
    [SerializeField] float panSpeed = 20f;

    [SerializeField] float maxCameraZoom = 10f;
    [SerializeField] float minCameraZoom = 3f;

    [SerializeField] GameObject topRightLimit;
    [SerializeField] GameObject bottomLeftLimit;

    private Camera cam;

    private Vector3 lastPanPosition;
    private int panFingerId;
    private bool zoomingLastFrame;
    private Vector2[] lastZoomPositions;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            HandleTouch();
        }
        else
        {
            HandleMouseAndKeyboard();
        }
    }

    private void HandleTouch()
    {
        switch (Input.touchCount)
        {

            case 1: // Panning
                zoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                }
                break;

            case 2: // Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!zoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    zoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, cameraZoomSpeed);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                zoomingLastFrame = false;
                break;
        }
    }

    private void HandleMouseAndKeyboard()
    {
        // Panning
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 v = new Vector3(transform.position.x,
                Mathf.Clamp(transform.position.y + cameraKeyboardMovementSpeed, bottomLeftLimit.transform.position.y, topRightLimit.transform.position.y),
                transform.position.z);
            transform.position = v;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 v = new Vector3(transform.position.x,
                Mathf.Clamp(transform.position.y - cameraKeyboardMovementSpeed, bottomLeftLimit.transform.position.y, topRightLimit.transform.position.y)
                , transform.position.z);
            transform.position = v;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 v = new Vector3(Mathf.Clamp(transform.position.x - cameraKeyboardMovementSpeed, bottomLeftLimit.transform.position.x, topRightLimit.transform.position.x),
                transform.position.y,
                transform.position.z);
            transform.position = v;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 v = new Vector3(Mathf.Clamp(transform.position.x + cameraKeyboardMovementSpeed, bottomLeftLimit.transform.position.x, topRightLimit.transform.position.x),
                transform.position.y,
                transform.position.z);
            transform.position = v;
        }
        if (Input.GetMouseButtonDown(2))
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            PanCamera(Input.mousePosition);
        }

        // Zooming
        if(Input.mouseScrollDelta.y != 0)
        {
            ZoomCamera(Input.mouseScrollDelta.y, cameraZoomSpeed);
        }
    }

    void PanCamera(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * cameraPanMovementSpeed,
            offset.y * cameraPanMovementSpeed,
            0);

        // Perform the movement
        transform.Translate(move, Space.World);

        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x,
            bottomLeftLimit.transform.position.x,
            topRightLimit.transform.position.x);
        pos.y = Mathf.Clamp(transform.position.y,
            bottomLeftLimit.transform.position.y,
            topRightLimit.transform.position.y);
        transform.position = pos;

        // Cache the position
        lastPanPosition = newPanPosition;
    }

    private void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (offset * speed), minCameraZoom, maxCameraZoom);
    }
}
