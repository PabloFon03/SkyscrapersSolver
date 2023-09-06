using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class GridGeneratorManagerScript : MonoBehaviour
{
    [SerializeField] GameObject block;
    [SerializeField] GameObject edgeSign;
    GridSolver solver;
    Task solveTask;
    bool waiting;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(0);
            row.forward = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right }[i];
            row.localPosition = row.forward * (0.5f * VirtualRAM.gridData.size + 0.5f);
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                Transform cell = Instantiate(edgeSign, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridData.size - 1) * ((float)j / (VirtualRAM.gridData.size - 1) - 0.5f) * (i == 1 || i == 2 ? -1 : 1);
                cell.GetChild(0).GetComponent<TextMeshPro>().text = " ";
            }
        }
        for (int i = 0; i < VirtualRAM.gridData.size; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(1);
            row.localPosition = Vector3.forward * (VirtualRAM.gridData.size - 1) * (0.5f - (float)i / (VirtualRAM.gridData.size - 1));
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                Transform cell = Instantiate(block, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridData.size - 1) * ((float)j / (VirtualRAM.gridData.size - 1) - 0.5f);
            }
        }
        transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().size = Vector2.one * (VirtualRAM.gridData.size + 2);
        for (int i = 0; i < 4; i++)
        {
            Transform wall = transform.GetChild(2).GetChild(i + 1);
            wall.localPosition = new Vector3[4] { Vector3.up, Vector3.right, Vector3.down, Vector3.left }[i] * (VirtualRAM.gridData.size * 0.5f + 1) + Vector3.forward * 5;
            wall.GetComponent<SpriteRenderer>().size = new Vector2(VirtualRAM.gridData.size + 2, 10);
        }
        solver = new GridSolver(VirtualRAM.gridData.size, transform.GetChild(1));
        solveTask = new Task(solver.GenerateGrid);
        solveTask.Start();
        waiting = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (waiting)
        {
            if (solveTask.IsCompleted)
            {
                print(solveTask.IsFaulted ? solveTask.Exception.ToString() : solveTask.Status.ToString());
                if (solver.finished)
                {
                    solver.DisplayResult();
                    solver.UpdateEdgeSigns(transform.GetChild(0));
                }
                waiting = false;
            }
            else { solver.DisplayResult(); }
        }
    }
}