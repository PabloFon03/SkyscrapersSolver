using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class RowGeneratorScript : MonoBehaviour
{
    Task task;
    // Start is called before the first frame update
    void Start()
    {
        task = new Task(GenerateAllRows);
        task.Start();
    }
    // Update is called once per frame
    void Update()
    {
        if (task.IsCompleted)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                SceneManager.LoadScene(1);
            }
            else { print(task.Exception.ToString()); }
        }
    }
    void GenerateAllRows()
    {
        VirtualRAM.validRows = new HashSet<VirtualRAM.GridRow>[9];
        FullRowSet rowSet = new FullRowSet();
        if (rowSet.Load()) { VirtualRAM.validRows = rowSet.hashSets; }
        else
        {
            for (int i = 0; i < VirtualRAM.validRows.Length; i++) { GenerateRows(i + 1); }
            rowSet = new FullRowSet(VirtualRAM.validRows);
            rowSet.Save();
        }
    }
    void GenerateRows(int size)
    {
        VirtualRAM.validRows[size - 1] = new HashSet<VirtualRAM.GridRow>();
        for (int i = 0; i < Mathf.Pow(size, size); i++)
        {
            int[] vals = new int[size - 1 + 1];
            HashSet<int> usedNums = new HashSet<int>();
            bool uniqueNums = true;
            int colVal = 1;
            for (int j = 0; j < size - 1 + 1 && uniqueNums; j++)
            {
                vals[j] = ((i / colVal) % (size - 1 + 1)) + 1;
                colVal *= size - 1 + 1;
                uniqueNums = usedNums.Add(vals[j]);
            }
            if (uniqueNums)
            {
                VirtualRAM.GridRow row = new VirtualRAM.GridRow();
                row.row = vals;
                row.leftSum = GridSolver.HeightSum(vals);
                row.rightSum = GridSolver.HeightSumReverse(vals);
                VirtualRAM.validRows[size - 1].Add(row);
            }
        }
    }
    [Serializable]
    class FullRowSet
    {
        [Serializable]
        public class RowSet
        {
            public VirtualRAM.GridRow[] rows;
            public RowSet() { }
            public RowSet(HashSet<VirtualRAM.GridRow> hashRows) { rows = hashRows.ToArray(); }
        }
        static string path { get { return Path.Combine(Application.streamingAssetsPath, "row_sets.json"); } }
        public RowSet[] rowSets;
        public HashSet<VirtualRAM.GridRow>[] hashSets
        {
            get
            {
                HashSet<VirtualRAM.GridRow>[] returnVal = new HashSet<VirtualRAM.GridRow>[rowSets.Length];
                for (int i = 0; i < returnVal.Length; i++)
                {
                    returnVal[i] = new HashSet<VirtualRAM.GridRow>();
                    for (int j = 0; j < rowSets[i].rows.Length; j++) { returnVal[i].Add(rowSets[i].rows[j]); }
                }
                return returnVal;
            }
        }
        public FullRowSet() { }
        public FullRowSet(HashSet<VirtualRAM.GridRow>[] hashSets)
        {
            rowSets = new RowSet[hashSets.Length];
            for (int i = 0; i < rowSets.Length; i++) { rowSets[i] = new RowSet(hashSets[i]); }
        }
        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(path))) { Directory.CreateDirectory(Path.GetDirectoryName(path)); }
            using (StreamWriter sw = new StreamWriter(path)) { sw.Write(JsonUtility.ToJson(this)); }
        }
        public bool Load()
        {
            if (!File.Exists(path)) { return false; }
            try
            {
                string jsonData = "";
                using (StreamReader sr = new StreamReader(path)) { jsonData = sr.ReadToEnd(); }
                if (string.IsNullOrEmpty(jsonData)) { return false; };
                rowSets = JsonUtility.FromJson<FullRowSet>(jsonData).rowSets;
                if (rowSets.Length != 9) { return false; }
                int n = 1;
                for (int i = 0; i < 9; i++)
                {
                    n *= i + 1;
                    if (rowSets[i].rows.Length != n) { return false; }
                }
            }
            catch { return false; }
            return true;
        }
    }
}