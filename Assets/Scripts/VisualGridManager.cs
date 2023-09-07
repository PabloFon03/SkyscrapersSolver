using System.Threading.Tasks;
using UnityEngine;

public abstract class VisualGridManager : MonoBehaviour
{
    [SerializeField] GameObject block;
    [SerializeField] GameObject edgeSign;
    protected GridSolver solver;
    protected Task task;
    protected bool waiting;
    // Start is called before the first frame update
    void Start()
    {
        EdgeSignScript[][] edgeSigns = new EdgeSignScript[4][];
        for (int i = 0; i < 4; i++)
        {
            edgeSigns[i] = new EdgeSignScript[VirtualRAM.gridData.size];
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(0);
            row.forward = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right }[i];
            row.localPosition = row.forward * (0.5f * VirtualRAM.gridData.size + 0.5f);
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                Transform cell = Instantiate(edgeSign, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridData.size - 1) * (VirtualRAM.gridData.size == 1 ? 0 : ((float)j / (VirtualRAM.gridData.size - 1) - 0.5f) * (i == 1 || i == 2 ? -1 : 1));
                edgeSigns[i][j] = cell.GetComponent<EdgeSignScript>();
            }
        }
        BlockScript[][] gridBlocks = new BlockScript[VirtualRAM.gridData.size][];
        for (int i = 0; i < VirtualRAM.gridData.size; i++)
        {
            gridBlocks[i] = new BlockScript[VirtualRAM.gridData.size];
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(1);
            row.localPosition = Vector3.forward * (VirtualRAM.gridData.size - 1) * (VirtualRAM.gridData.size == 1 ? 0 : (0.5f - (float)i / (VirtualRAM.gridData.size - 1)));
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                Transform cell = Instantiate(block, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridData.size - 1) * (VirtualRAM.gridData.size == 1 ? 0 : ((float)j / (VirtualRAM.gridData.size - 1) - 0.5f));
                gridBlocks[i][j] = cell.GetComponent<BlockScript>();
            }
        }
        transform.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().size = Vector2.one * (VirtualRAM.gridData.size + 2);
        for (int i = 0; i < 4; i++)
        {
            Transform wall = transform.GetChild(2).GetChild(i + 1);
            wall.localPosition = new Vector3[4] { Vector3.up, Vector3.right, Vector3.down, Vector3.left }[i] * (VirtualRAM.gridData.size * 0.5f + 1) + Vector3.forward * 5;
            wall.GetComponent<SpriteRenderer>().size = new Vector2(VirtualRAM.gridData.size + 2, 10);
        }
        solver = new GridSolver(VirtualRAM.gridData.size, edgeSigns, gridBlocks);
        OnStart();
    }
    protected abstract void OnStart();
    // Update is called once per frame
    void Update()
    {
        OnUpdate();
    }
    protected abstract void OnUpdate();
}