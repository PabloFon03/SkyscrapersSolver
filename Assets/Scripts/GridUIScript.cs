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
        VirtualRAM.gridData.edgeNums = new int[4][];
        for (int i = 0; i < 4; i++) { VirtualRAM.gridData.edgeNums[i] = new int[VirtualRAM.gridData.size]; }
        VirtualRAM.gridData.filledSlots = new int[VirtualRAM.gridData.size][];
        for (int i = 0; i < VirtualRAM.gridData.size; i++) { VirtualRAM.gridData.filledSlots[i] = new int[VirtualRAM.gridData.size]; }
        Camera.main.orthographicSize = VirtualRAM.gridData.size + 1;
        for (int i = 0; i < 4; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(0);
            row.up = new Vector3[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right }[i];
            row.localPosition = row.up * (0.5f * VirtualRAM.gridData.size + 0.5f);
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                Transform cell = Instantiate(edgeArrow, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridData.size - 1) * ((float)j / (VirtualRAM.gridData.size - 1) - 0.5f) * (i == 2 ? -1 : 1);
                cell.GetChild(0).rotation = Quaternion.identity;
            }
        }
        for (int i = 0; i < VirtualRAM.gridData.size; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(1);
            row.localPosition = Vector3.up * (VirtualRAM.gridData.size - 1) * (0.5f - (float)i / (VirtualRAM.gridData.size - 1));
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                Transform cell = Instantiate(gridSlot, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridData.size - 1) * ((float)j / (VirtualRAM.gridData.size - 1) - 0.5f);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && ValidateInput())
        {
            SceneManager.LoadScene(3);
        }
    }
    bool ValidateInput()
    {
        for (int i = 0; i < 4; i++)
        {
            Transform row = transform.GetChild(0).GetChild(i);
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                if (int.TryParse(row.GetChild(j).GetComponent<TMP_InputField>().text, out int n) && n > 0 && n <= VirtualRAM.gridData.size) { VirtualRAM.gridData.edgeNums[i][j] = n; }
                else if (true) { VirtualRAM.gridData.edgeNums[i][j] = 0; }
                else { return false; }
            }
        }
        for (int i = 0; i < VirtualRAM.gridData.size; i++)
        {
            Transform row = transform.GetChild(1).GetChild(i);
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                if (int.TryParse(row.GetChild(j).GetComponent<TMP_InputField>().text, out int n) && n > 0 && n <= VirtualRAM.gridData.size) { VirtualRAM.gridData.filledSlots[i][j] = n; }
                else { VirtualRAM.gridData.filledSlots[i][j] = 0; }
            }
        }
        return true;
    }
}