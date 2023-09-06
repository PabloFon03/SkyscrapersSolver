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
        UpdateSize();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            size--;
            if (size < 1) { size = 9; }
            UpdateSize();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            size++;
            if (size > 9) { size = 1; }
            UpdateSize();
        }
        angle += 45 * Time.deltaTime;
        transform.GetChild(1).rotation = Quaternion.Euler(new Vector3(90, angle));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            VirtualRAM.gridData.size = size;
            SceneManager.LoadScene(2);
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            VirtualRAM.gridData.size = size;
            SceneManager.LoadScene(4);
        }
    }
    void UpdateSize()
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = $"<  {size} x {size}  >";
        transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().size = Vector2.one * (size + 1);
        for (int i = 0; i < 4; i++)
        {
            Transform wall = transform.GetChild(1).GetChild(i + 1);
            wall.localPosition = new Vector3[4] { Vector3.up, Vector3.right, Vector3.down, Vector3.left }[i] * ((size + 1) * 0.5f) + Vector3.forward * 5;
            wall.GetComponent<SpriteRenderer>().size = new Vector2(size + 1, 10);
        }
    }
}