using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float moveSpeed = 5f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // Lock the cursor in the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Camera rotation with mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotation to the main camera
        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Camera movement with WASD
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = Camera.main.transform.right * moveX + Camera.main.transform.forward * moveZ;
        Camera.main.transform.position += move * moveSpeed * Time.deltaTime;
    }
}
