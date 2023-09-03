using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    Transform cam;
    float angle;
    enum States {  }
    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetChild(0);
        cam.localPosition = new Vector3(0, 2.5f, -7.5f);
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * -90 * Input.GetAxis("Horizontal") * Time.deltaTime);
    }
}