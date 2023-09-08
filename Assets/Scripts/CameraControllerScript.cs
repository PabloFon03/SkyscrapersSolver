using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraControllerScript : MonoBehaviour
{
    Transform cam;
    Light camLight;
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
        camLight = cam.GetChild(0).GetComponent<Light>();
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
        Camera.main.orthographic = currentState != States.Perspective;
        switch (currentState)
        {
            case States.Orthographic:
                Camera.main.orthographicSize = 0.5f * VirtualRAM.gridData.size + 1.5f;
                targetAngle = Mathf.RoundToInt(angle / 90) * 90;
                break;
            case States.Top:
                Camera.main.orthographicSize = 0.5f * VirtualRAM.gridData.size + 1.5f;
                break;
        }
        UpdateCameraPosition();
        UpdateCameraRotation();
        UpdateLight();
    }
    void UpdateCameraPosition()
    {
        switch (currentState)
        {
            case States.Perspective:
                float[] depths = new float[9] { 4, 7.5f, 6.5f, 7.5f, 9, 10.5f, 12, 14.5f, 15 };
                cam.localPosition = new Vector3(0, Mathf.Clamp(0.5f * VirtualRAM.gridData.size, 1, 4.5f), -depths[VirtualRAM.gridData.size - 1]);
                break;
            case States.Orthographic:
                cam.localPosition = new Vector3(0, Mathf.Clamp(0.5f * VirtualRAM.gridData.size, 1, 4.5f), -VirtualRAM.gridData.size * 2);
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
    void UpdateLight()
    {
        camLight.intensity = currentState == States.Top ? 0.75f : 1;
        switch (currentState)
        {
            case States.Perspective:
                camLight.transform.localRotation = Quaternion.Euler(new Vector3(30, -30));
                break;
            case States.Orthographic:
                camLight.transform.localRotation = Quaternion.Euler(new Vector3(30, 0));
                break;
            case States.Top:
                camLight.transform.localRotation = Quaternion.identity;
                break;
        }
    }
}