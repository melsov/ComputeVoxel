// This script is included with the VoxelPerformance asset to assist with moving around a voxel terrain for debugging.

using UnityEngine;


public class VPCamera : MonoBehaviour
{
    public bool locked = true;
    public float speed = 100f;

    float rampSpeed;

    float angleX;
    float angleY;


    void Update()
    {
        mouse();
        move();
    }


    void mouse()
    {
        bool cursorLock = Input.GetKey(KeyCode.Tab) ? false : locked;

        Cursor.lockState = cursorLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLock;

        angleX += Input.GetAxis("Mouse X");
        angleY += Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(-angleY, angleX, 0);
    }


    void move()
    {

        var axes = new Vector3(Input.GetAxis("Horizontal"), Input.GetKey(KeyCode.Q) ? 1f : (Input.GetKey(KeyCode.Z) ? -1f : 0f), Input.GetAxis("Vertical"));
        if (axes.sqrMagnitude > 0f)
        {
            rampSpeed = Mathf.Lerp(rampSpeed, speed, .5f);
        }
        else
        {
            rampSpeed = 0f;
        }
        Vector3 velocity = axes * rampSpeed;

        transform.Translate(velocity * Time.deltaTime);
    }
}

