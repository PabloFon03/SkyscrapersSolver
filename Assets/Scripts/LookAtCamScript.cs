using UnityEngine;

public class LookAtCamScript : MonoBehaviour
{
    Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
    }
    // Update is called once per frame
    void Update()
    {
        transform.forward = cam.forward;
    }
}