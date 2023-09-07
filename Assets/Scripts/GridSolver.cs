using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GridSolver
{
    // The Size Of The Grid: Since The Grid Is Always Square, It Instead Stores The Size Of Its Side
    readonly int size;
    // A List Of All Rows With Non-Repeating Numbers
    VirtualRAM.GridRow[] allRows;
    // A List Of All Possible Rows Per Row Slot: A Row Is Deemed As 'Possible' If There Are No Repeating Numbers And The Height Sums On Both Directions Match With Their Respective Row Slot's
    List<int>[] possibleRows;
    // A Square Grid Of Numbers: When The Grid Is Solved, The Winning Combination Is Left Stored
    int[][] grid;
    public enum Status { Idle, Running, Finished, Stopped }
    public Status status { get; private set; } = Status.Idle;
    ulong total;
    ulong tested;
    public double progress { get { return total > 0 ? tested / (double)total : 0; } }
    readonly EdgeSignScript[][] edgeSigns;
    readonly BlockScript[][] gridBlocks;
    bool displayGrid;
    bool displayEdgeSigns;
    public GridSolver(int _size, EdgeSignScript[][] _edgeSigns, BlockScript[][] _gridBlocks)
    {
        size = _size;
        allRows = VirtualRAM.validRows[size - 1].ToArray();
        edgeSigns = _edgeSigns;
        gridBlocks = _gridBlocks;
    }
    bool FindPossibleRows(in int[][] _heightSums, in int[][] _filledSlots)
    {
        possibleRows = new List<int>[size];
        for (int i = 0; i < size; i++)
        {
            possibleRows[i] = new List<int>();
            for (int j = 0; j < allRows.Length; j++)
            {
                int[] currentRow = allRows[j].row;
                bool leftSum = _heightSums[2][i] == 0 || allRows[j].leftSum == _heightSums[2][i];
                bool rightSum = _heightSums[3][i] == 0 || allRows[j].rightSum == _heightSums[3][i];
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
    public void SolveGrid(in int[][] _heightSums, in int[][] _filledSlots)
    {
        status = Status.Running;
        tested = 0;
        grid = new int[size][];
        int[] seeds = new int[size];
        if (FindPossibleRows(_heightSums, _filledSlots))
        {
            total = 1;
            for (int i = 0; i < size; i++) { total *= (ulong)possibleRows[i].Count; }
            while (status == Status.Running && seeds[0] < possibleRows[0].Count)
            {
                BuildGrid(seeds);
                if (IsValidSolution(_heightSums)) { status = Status.Finished; } else { UpdateSeed(ref seeds); }
                tested++;
                DisplayChecks();
            }
        }
        else { Debug.Log("a"); }
    }
    public void GenerateGrid()
    {
        status = Status.Running;
        grid = new int[size][];
        possibleRows = new List<int>[size];
        System.Random r = new System.Random((int)System.DateTime.Now.Ticks);
        for (int i = 0; i < size; i++)
        {
            possibleRows[i] = new List<int>();
            for (int j = 0; j < allRows.Length; j++) { possibleRows[i].Add(j); }
            grid[i] = new int[size];
        }
        total = (ulong)size;
        for (int i = 0; i < size; i++)
        {
            tested = (ulong)i;
            bool validRow = false;
            while (!validRow)
            {
                int n = r.Next(0, possibleRows[i].Count);
                CopyRow(i, n);
                validRow = CheckColumns(i);
                for (int j = i; j < size; j++) { possibleRows[j].RemoveAt(n); }
                DisplayChecks();
            }
            possibleRows[i].Clear();
        }
        status = Status.Finished;
    }
    void BuildGrid(in int[] seeds)
    {
        for (int i = 0; i < size; i++)
        {
            grid[i] = new int[size];
            CopyRow(i, seeds[i]);
        }
    }
    void CopyRow(in int row, in int seed) { for (int i = 0; i < size; i++) { grid[row][i] = allRows[possibleRows[row][seed]].row[i]; } }
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
            bool upCheck = heightSumUp == 0 || HeightSum(column) == heightSumUp;
            bool downCheck = heightSumDown == 0 || HeightSumReverse(column) == heightSumDown;
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
    bool CheckColumns() { return CheckColumns(size - 1); }
    bool CheckColumns(int rowCount)
    {
        if (rowCount < 1) { return true; }
        for (int i = 0; i < size; i++)
        {
            HashSet<int> numSet = new HashSet<int>();
            for (int j = 0; j <= rowCount; j++) { if (!numSet.Add(grid[j][i])) { return false; } }
        }
        return true;
    }
    public static int HeightSum(int[] _row)
    {
        int maxHeight = 0;
        int seenBuildings = 0;
        for (int i = 0; i < _row.Length; i++)
        {
            if (_row[i] > maxHeight)
            {
                maxHeight = _row[i];
                seenBuildings++;
            }
        }
        return seenBuildings;
    }
    public static int HeightSumReverse(int[] _row)
    {
        int maxHeight = 0;
        int seenBuildings = 0;
        for (int i = _row.Length - 1; i >= 0; i--)
        {
            if (_row[i] > maxHeight)
            {
                maxHeight = _row[i];
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
    void DisplayChecks()
    {
        if (displayGrid)
        {
            DisplayResult(true);
            displayGrid = false;
        }
        if (displayEdgeSigns)
        {
            DisplayEdgeSigns(true);
            displayEdgeSigns = false;
        }
    }
    public void DisplayResult(bool _force = false)
    {
        if (status == Status.Running && !_force)
        {
            displayGrid = true;
            return;
        }
        try { for (int i = 0; i < size; i++) { for (int j = 0; j < size; j++) { gridBlocks[i][j].SetTargetHeight(grid[i][j], size); } } }
        catch { displayGrid = true; }
    }
    public void DisplayEdgeSigns(bool _force = false)
    {
        if (status == Status.Running && !_force)
        {
            displayEdgeSigns = true;
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int[] data = i < 2 ? GetGridColumn(j) : grid[j];
                int heightSum = i % 2 == 0 ? HeightSum(data) : HeightSumReverse(data);
                edgeSigns[i][j].SetValue(heightSum);
            }
        }
    }
}