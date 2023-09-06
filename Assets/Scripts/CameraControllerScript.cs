using UnityEngine;
using UnityEngine.Android;

public class CameraControllerScript : MonoBehaviour
{
    Transform cam;
    float angle;
    float targetAngle;
    enum States { Perspective, Orthographic, Top };
    States currentState;
    // Start is called before the first frame update
    void Start()
    {
        angle = 0;
        targetAngle = 0;
        currentState = States.Perspective;
        cam = transform.GetChild(0);
        SetCameraPosition();
    }
    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.Perspective:
                angle += Input.GetAxis("Horizontal") * Time.deltaTime;
                while (angle < 0) { angle += 360; }
                while (angle >= 360) { angle -= 360; }
                transform.rotation = Quaternion.Euler(Vector3.up * -90 * angle);
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    currentState = States.Top;
                    Camera.main.orthographic = true;
                    transform.rotation = Quaternion.Euler(Vector3.right * 90);
                    SetCameraPosition();
                }
                break;
            case States.Orthographic:
                break;
            case States.Top:
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    currentState = States.Perspective;
                    Camera.main.orthographic = false;
                    SetCameraPosition();
                }
                break;
        }
    }
    void SetCameraPosition()
    {
        switch (currentState)
        {
            case States.Perspective:
                cam.localPosition = new Vector3(0, 0.875f + 0.375f * VirtualRAM.gridData.size, -2.75f - 1.325f * VirtualRAM.gridData.size);
                break;
            case States.Orthographic:
                cam.localPosition = new Vector3(0, 0.75f + 0.25f * VirtualRAM.gridData.size, -2.5f - 1.25f * VirtualRAM.gridData.size);
                break;
            case States.Top:
                cam.localPosition = Vector3.back * (VirtualRAM.gridData.size + 1);
                break;
        }
    }
}