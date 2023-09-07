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
                angle += Input.GetAxis("Horizontal") * 90 * Time.deltaTime;
                while (angle < 0) { angle += 360; }
                while (angle >= 360) { angle -= 360; }
                UpdateCameraRotation();
                if (Input.GetKeyDown(KeyCode.UpArrow)) { SetState(States.Top); }
                else if (Input.GetKeyDown(KeyCode.DownArrow)) { SetState(States.Orthographic); }
                break;
            case States.Orthographic:
                if (angle != targetAngle)
                {
                    angle = Mathf.Lerp(angle, targetAngle, 0.04f);
                    if (Mathf.Abs(angle - targetAngle) < 1) { angle = targetAngle; }
                }
                else { targetAngle += Input.GetAxisRaw("Horizontal") * 90; }
                UpdateCameraRotation();
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
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = new float[9] { 5, 5, 5, 3.5f, 4, 4.5f, 5, 5.5f, 6 }[VirtualRAM.gridData.size - 1];
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
        float[] heights;
        switch (currentState)
        {
            case States.Perspective:
                heights = new float[9] { 1, 1, 1, 2, 2.5f, 3, 3.5f, 4, 4.5f };
                float[] depths = new float[9] { 4, 7.5f, 1, 7.5f, 9, 10.5f, 12, 14.5f, 15 };
                cam.localPosition = new Vector3(0, heights[VirtualRAM.gridData.size - 1], -depths[VirtualRAM.gridData.size - 1]);
                break;
            case States.Orthographic:
                heights = new float[9] { 1, 1, 1, 2, 2.5f, 3, 3.5f, 4, 4.5f };
                cam.localPosition = new Vector3(0, heights[VirtualRAM.gridData.size - 1], -VirtualRAM.gridData.size * 2);
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
                transform.rotation = Quaternion.Euler(-Vector3.up * angle);
                break;
            case States.Top:
                transform.rotation = Quaternion.Euler(Vector3.right * 90);
                break;
        }
    }
}