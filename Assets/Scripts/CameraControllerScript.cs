using UnityEngine;
using UnityEngine.SceneManagement;

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
        UpdateCameraPosition();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { SceneManager.LoadScene(1); }
        switch (currentState)
        {
            case States.Perspective:
                angle += Input.GetAxis("Horizontal") * Time.deltaTime;
                while (angle < 0) { angle += 360; }
                while (angle >= 360) { angle -= 360; }
                UpdateCameraRotation();
                if (Input.GetKeyDown(KeyCode.UpArrow)) { SetState(States.Top); }
                else if (Input.GetKeyDown(KeyCode.DownArrow)) { SetState(States.Orthographic); }
                break;
            case States.Orthographic:
                if (Input.GetKeyDown(KeyCode.UpArrow)) { SetState(States.Top); }
                else if (Input.GetKeyDown(KeyCode.DownArrow)) { SetState(States.Perspective); }
                break;
            case States.Top:
                if (Input.GetKeyDown(KeyCode.UpArrow)) { SetState(States.Perspective); }
                else if (Input.GetKeyDown(KeyCode.DownArrow)) { SetState(States.Orthographic); }
                break;
        }
    }
    void SetState(States _nextState)
    {
        currentState = _nextState;
        switch (currentState)
        {
            case States.Perspective:
                Camera.main.orthographic = false;
                break;
            case States.Orthographic:
                Camera.main.orthographic = false;
                targetAngle = Mathf.RoundToInt(angle / 90) * 90;
                break;
            case States.Top:
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = Mathf.Clamp(VirtualRAM.gridData.size * 0.9f - 0.5f, 3, 9);
                break;
        }
        UpdateCameraPosition();
        UpdateCameraRotation();
    }
    void UpdateCameraPosition()
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
    void UpdateCameraRotation()
    {
        switch (currentState)
        {
            case States.Perspective:
            case States.Orthographic:
                transform.rotation = Quaternion.Euler(Vector3.up * -90 * angle);
                break;
            case States.Top:
                transform.rotation = Quaternion.Euler(Vector3.right * 90);
                break;
        }
    }
}