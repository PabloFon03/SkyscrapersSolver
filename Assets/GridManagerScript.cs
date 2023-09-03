using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class GridManagerScript : MonoBehaviour
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
            row.localPosition = row.forward * (0.5f * VirtualRAM.gridSize + 0.5f);
            for (int j = 0; j < VirtualRAM.gridSize; j++)
            {
                Transform cell = Instantiate(edgeSign, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridSize - 1) * ((float)j / (VirtualRAM.gridSize - 1) - 0.5f) * (i == 1 || i == 2 ? -1 : 1);
                cell.GetChild(0).GetComponent<TextMeshPro>().text = VirtualRAM.edgeNums[i][j] > 0 ? VirtualRAM.edgeNums[i][j].ToString() : "?";
            }
        }
        for (int i = 0; i < VirtualRAM.gridSize; i++)
        {
            Transform row = new GameObject("Row").transform;
            row.parent = transform.GetChild(1);
            row.localPosition = Vector3.forward * (VirtualRAM.gridSize - 1) * (0.5f - (float)i / (VirtualRAM.gridSize - 1));
            for (int j = 0; j < VirtualRAM.gridSize; j++)
            {
                Transform cell = Instantiate(block, row).transform;
                cell.localPosition = Vector3.right * (VirtualRAM.gridSize - 1) * ((float)j / (VirtualRAM.gridSize - 1) - 0.5f);
            }
        }
        solver = new GridSolver(VirtualRAM.gridSize, transform.GetChild(1));
        solveTask = new Task(() => solver.Solve(VirtualRAM.edgeNums, VirtualRAM.filledSlots));
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
                if (solver.solved) { solver.DisplayResult(); }
                waiting = false;
            }
            else { print($"Progress: {solver.progress}"); }
        }
    }
}

class GridSolver
{
    // The Size Of The Grid: Since The Grid Is Always Square, It Instead Stores The Size Of Its Side
    readonly int size;
    // A List Of All Rows With Non-Repeating Numbers
    List<int[]> allRows;
    // A List Of All Possible Rows Per Row Slot: A Row Is Deemed As 'Possible' If There Are No Repeating Numbers And The Height Sums On Both Directions Match With Their Respective Row Slot's
    List<int>[] possibleRows;
    // A Square Grid Of Numbers: When The Grid Is Solved, The Winning Combination Is Left Stored
    int[][] grid;
    readonly BlockScript[][] blockGrid;
    public bool solved { get; private set; }
    ulong total;
    ulong tested;
    public double progress { get { return total > 0 ? tested / (double)total : 0; } }
    public GridSolver(int _size, Transform _gridParent)
    {
        size = _size;
        blockGrid = new BlockScript[size][];
        for (int i = 0; i < size; i++)
        {
            blockGrid[i] = new BlockScript[size];
            for (int j = 0; j < size; j++) { blockGrid[i][j] = _gridParent.GetChild(i).GetChild(j).GetComponent<BlockScript>(); }
        }
        CalculateAllRows();
    }
    int CalculateFactorial(int n)
    {
        int f = 1;
        while (n > 1)
        {
            f *= n;
            n--;
        }
        return f;
    }
    int CalculatePower(int a, int b)
    {
        int p = 1;
        while (b > 0)
        {
            p *= a;
            b--;
        }
        return p;
    }
    /// <summary>
    /// This function checks whether there are duplicates or not in a given set of elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="set">The set to check for duplicates inside of.</param>
    /// <returns><b>true</b> if a duplicate was found, otherwise <b>false</b>.</returns>
    bool DuplicateCheck<T>(IEnumerable<T> set)
    {
        HashSet<T> values = new HashSet<T>();
        foreach (T t in set) { if (!values.Add(t)) { return true; } }
        return false;
    }
    /// <summary>
    /// Calculates and stores all possible rows with non-repeating numbers.
    /// </summary>
    void CalculateAllRows()
    {
        allRows = new List<int[]>();
        for (int i = 0; i < CalculatePower(size, size); i++)
        {
            int[] vals = new int[size];
            int colVal = 1;
            for (int j = 0; j < size; j++)
            {
                vals[j] = i / colVal % size + 1;
                colVal *= size;
            }
            if (!DuplicateCheck(vals)) { allRows.Add(vals); }
        }
    }
    bool FindPossibleRows(in int[][] _heightSums, in int[][] _filledSlots)
    {
        possibleRows = new List<int>[size];
        for (int i = 0; i < size; i++)
        {
            possibleRows[i] = new List<int>();
            for (int j = 0; j < allRows.Count; j++)
            {
                int[] currentRow = allRows[j];
                bool leftSum = _heightSums[2][i] == 0 || HeightSumLeft(currentRow) == _heightSums[2][i];
                bool rightSum = _heightSums[3][i] == 0 || HeightSumRight(currentRow) == _heightSums[3][i];
                bool columnCheck = true;
                bool gridCheck = true;
                for (int k = 0; k < size && columnCheck && gridCheck; k++)
                {
                    // Pre-Filled Slots Check
                    if (_filledSlots[i][k] != 0) { gridCheck = currentRow[k] == _filledSlots[i][k]; }
                    // Single Visible Building Check
                    if ((i == 0 && _heightSums[0][k] == 1) || (i == size - 1 && _heightSums[1][k] == 1)) { columnCheck = currentRow[k] == size; }
                    // All Visible Buildings Check
                    if (_heightSums[0][k] == size) { columnCheck = currentRow[k] == i + 1; }
                    if (_heightSums[1][k] == size) { columnCheck = currentRow[k] == size - i; }
                }
                if (leftSum && rightSum && columnCheck && gridCheck) { possibleRows[i].Add(j); }
            }
            if (possibleRows[i].Count == 0) { return false; }
        }
        return true;
    }
    public void Solve(in int[][] _heightSums, in int[][] _filledSlots)
    {
        solved = false;
        tested = 0;
        grid = new int[size][];
        int[] seeds = new int[size];
        if (FindPossibleRows(_heightSums, _filledSlots))
        {
            total = 1;
            for (int i = 0; i < size; i++) { total *= (ulong)possibleRows[i].Count; }
            while (!solved && seeds[0] < possibleRows[0].Count)
            {
                if (true || !DuplicateCheck(seeds))
                {
                    BuildGrid(seeds);
                    if (IsValidSolution(_heightSums)) { solved = true; } else { UpdateSeed(ref seeds); }
                }
                else { UpdateSeed(ref seeds); }
                tested++;
            }
        }
        else { Debug.Log("a"); }
    }
    void BuildGrid(in int[] seeds)
    {
        for (int i = 0; i < size; i++)
        {
            grid[i] = new int[size];
            for (int j = 0; j < size; j++) { grid[i][j] = allRows[possibleRows[i][seeds[i]]][j]; }
        }
    }
    void UpdateSeed(ref int[] seeds)
    {
        for (int i = size - 1; i >= 0; i--)
        {
            if (i == size - 1) { seeds[i]++; }
            else if (seeds[i + 1] == possibleRows[i + 1].Count)
            {
                seeds[i + 1] = 0;
                seeds[i]++;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_heightSums"></param>
    /// <returns><b>true</b> if the solution is valid, otherwise <b>false</b>.</returns>
    bool IsValidSolution(in int[][] _heightSums)
    {
        if (!CheckColumns()) { return false; }
        for (int i = 0; i < size; i++)
        {
            int heightSumUp = _heightSums[0][i];
            int heightSumDown = _heightSums[1][i];
            int[] column = GetGridColumn(i);
            bool upCheck = heightSumUp == 0 || HeightSumUp(column) == heightSumUp;
            bool downCheck = heightSumDown == 0 || HeightSumDown(column) == heightSumDown;
            if (!upCheck || !downCheck) { return false; }
        }
        return true;
    }
    int[] GetGridColumn(int _colIndex)
    {
        int[] colVals = new int[size];
        for (int j = 0; j < size; j++) { colVals[j] = grid[j][_colIndex]; }
        return colVals;
    }
    /// <summary>
    /// This functions checks that there are no duplicates within any of the grid's columns.
    /// </summary>
    /// <returns><b>true</b> if there are no duplicates in any of the columns, otherwise <b>false</b>.</returns>
    bool CheckColumns()
    {
        for (int i = 0; i < size; i++) { if (DuplicateCheck(GetGridColumn(i))) { return false; } }
        return true;
    }
    int HeightSumLeft(int[] _row)
    {
        int maxHeight = 0;
        int seenBuildings = 0;
        for (int i = 0; i < size; i++)
        {
            if (_row[i] > maxHeight)
            {
                maxHeight = _row[i];
                seenBuildings++;
            }
        }
        return seenBuildings;
    }
    int HeightSumRight(int[] _row)
    {
        int maxHeight = 0;
        int seenBuildings = 0;
        for (int i = size - 1; i >= 0; i--)
        {
            if (_row[i] > maxHeight)
            {
                maxHeight = _row[i];
                seenBuildings++;
            }
        }
        return seenBuildings;
    }
    int HeightSumUp(int[] _col)
    {
        int maxHeight = 0;
        int seenBuildings = 0;
        for (int i = 0; i < size; i++)
        {
            if (_col[i] > maxHeight)
            {
                maxHeight = _col[i];
                seenBuildings++;
            }
        }
        return seenBuildings;
    }
    int HeightSumDown(int[] _col)
    {
        int maxHeight = 0;
        int seenBuildings = 0;
        for (int i = size - 1; i >= 0; i--)
        {
            if (_col[i] > maxHeight)
            {
                maxHeight = _col[i];
                seenBuildings++;
            }
        }
        return seenBuildings;
    }
    void PrintGrid()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                sb.Append(grid[i][j]);
                sb.Append(' ');
            }
            sb.Append('\n');
        }
        Debug.Log(sb.ToString());
    }
    public void DisplayResult() { for (int i = 0; i < size; i++) { for (int j = 0; j < size; j++) { blockGrid[i][j].SetTargetHeight(grid[i][j], size); } } }
}