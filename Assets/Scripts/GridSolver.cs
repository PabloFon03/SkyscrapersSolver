using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
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
    readonly BlockScript[][] blockGrid;
    public bool finished { get; private set; }
    ulong total;
    ulong tested;
    public double progress { get { return total > 0 ? tested / (double)total : 0; } }
    public GridSolver(int _size, Transform _gridParent)
    {
        size = _size;
        allRows = VirtualRAM.validRows[size - 1].ToArray();
        blockGrid = new BlockScript[size][];
        for (int i = 0; i < size; i++)
        {
            blockGrid[i] = new BlockScript[size];
            for (int j = 0; j < size; j++) { blockGrid[i][j] = _gridParent.GetChild(i).GetChild(j).GetComponent<BlockScript>(); }
        }
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
        finished = false;
        tested = 0;
        grid = new int[size][];
        int[] seeds = new int[size];
        if (FindPossibleRows(_heightSums, _filledSlots))
        {
            total = 1;
            for (int i = 0; i < size; i++) { total *= (ulong)possibleRows[i].Count; }
            while (!finished && seeds[0] < possibleRows[0].Count)
            {
                BuildGrid(seeds);
                if (IsValidSolution(_heightSums)) { finished = true; } else { UpdateSeed(ref seeds); }
                tested++;
            }
        }
        else { Debug.Log("a"); }
    }
    public void GenerateGrid()
    {
        finished = false;
        grid = new int[size][];
        possibleRows = new List<int>[size];
        System.Random r = new System.Random(System.DateTime.Now.Millisecond);
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
            }
            possibleRows[i].Clear();
        }
        finished = true;
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
    bool CheckColumns()
    {
        for (int i = 0; i < size; i++) { if (DuplicateCheck(GetGridColumn(i))) { return false; } }
        return true;
    }
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
    public void DisplayResult() { for (int i = 0; i < size; i++) { for (int j = 0; j < size; j++) { blockGrid[i][j].SetTargetHeight(grid[i][j], size); } } }
    public void UpdateEdgeSigns(Transform _parent)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int[] data = i < 2 ? GetGridColumn(j) : grid[j];
                int heightSum = i % 2 == 0 ? HeightSum(data) : HeightSumReverse(data);
                _parent.GetChild(i).GetChild(j).GetComponent<EdgeSignScript>().SetValue(heightSum);
            }
        }
    }
}