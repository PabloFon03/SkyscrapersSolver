using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SizeSelectMenuScript : MonoBehaviour
{
    int size;
    float angle;
    // Start is called before the first frame update
    void Start()
    {
        size = 1;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            size--;
            if (size < 1) { size = 9; }
            transform.GetChild(0).GetComponent<TextMeshPro>().text = $"<  {size} x {size}  >";
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            size++;
            if (size > 9) { size = 1; }
            transform.GetChild(0).GetComponent<TextMeshPro>().text = $"<  {size} x {size}  >";
        }
        angle += 45 * Time.deltaTime;
        transform.GetChild(1).rotation = Quaternion.Euler(new Vector3(90, angle));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            VirtualRAM.gridSize = size;
            SceneManager.LoadScene(1);
        }
    }
}