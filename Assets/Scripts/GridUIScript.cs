using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GridUIScript : MonoBehaviour
{
    [SerializeField] GameObject gridSlot;
    [SerializeField] GameObject edgeArrow;
    // Start is called before the first frame update
    void Start()
    {
        VirtualRAM.edgeNums = new int[4][];
        for (int i = 0; i < 4; i++) { VirtualRAM.edgeNums[i] = new int[VirtualRAM.gridSize]; }
        VirtualRAM.filledSlots = new int[VirtualRAM.gridSize][];
        for (int i = 0; i < VirtualRAM.gridSize; i++) { VirtualRAM.filledSlots[i] = new int[VirtualRAM.gridSize]; }
        Camera.main.orthographicSize = VirtualRAM.gridSize + 1;
        for (int i = 0; i < 4; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(0);
            row.up = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right }[i];
            row.localPosition = row.up * (0.5f * VirtualRAM.gridSize + 0.5f);
            for (int j = 0; j < VirtualRAM.gridSize; j++)
            {
                Transform cell = Instantiate(edgeArrow, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridSize - 1) * ((float)j / (VirtualRAM.gridSize - 1) - 0.5f) * (i == 2 ? -1 : 1);
                cell.GetChild(0).rotation = Quaternion.identity;
            }
        }
        for (int i = 0; i < VirtualRAM.gridSize; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(1);
            row.localPosition = Vector3.up * (VirtualRAM.gridSize - 1) * (0.5f - (float)i / (VirtualRAM.gridSize - 1));
            for (int j = 0; j < VirtualRAM.gridSize; j++)
            {
                Transform cell = Instantiate(gridSlot, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridSize - 1) * ((float)j / (VirtualRAM.gridSize - 1) - 0.5f);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && ValidateInput())
        {
            SceneManager.LoadScene(2);
        }
    }
    bool ValidateInput()
    {
        for (int i = 0; i < 4; i++)
        {
            Transform row = transform.GetChild(0).GetChild(i);
            for (int j = 0; j < VirtualRAM.gridSize; j++)
            {
                if (int.TryParse(row.GetChild(j).GetComponent<TMP_InputField>().text, out int n) && n > 0 && n <= VirtualRAM.gridSize) { VirtualRAM.edgeNums[i][j] = n; }
                else if (true) { VirtualRAM.edgeNums[i][j] = 0; }
                else { return false; }
            }
        }
        for (int i = 0; i < VirtualRAM.gridSize; i++)
        {
            Transform row = transform.GetChild(1).GetChild(i);
            for (int j = 0; j < VirtualRAM.gridSize; j++)
            {
                if (int.TryParse(row.GetChild(j).GetComponent<TMP_InputField>().text, out int n) && n > 0 && n <= VirtualRAM.gridSize) { VirtualRAM.filledSlots[i][j] = n; }
                else { VirtualRAM.filledSlots[i][j] = 0; }
            }
        }
        return true;
    }
}