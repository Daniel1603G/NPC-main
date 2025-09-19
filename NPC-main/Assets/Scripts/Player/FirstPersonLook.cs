using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f; 
    [SerializeField] private float minPitch = -80f;         
    [SerializeField] private float maxPitch = 80f;           
    [SerializeField] private bool lockCursor = true;         

    [SerializeField] private Transform cameraTransform; 

    private float yaw;  
    private float pitch; 

    private void Awake()
    {
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }
    }

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleMouseLook();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
