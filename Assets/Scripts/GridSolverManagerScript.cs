using System.Threading.Tasks;

public class GridSolverManagerScript : VisualGridManager
{
    protected override void OnStart()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < VirtualRAM.gridData.size; j++)
            {
                transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<EdgeSignScript>().SetValue(VirtualRAM.gridData.edgeNums[i][j]);
            }
        }
        task = new Task(() => solver.SolveGrid(VirtualRAM.gridData.edgeNums, VirtualRAM.gridData.filledSlots));
        task.Start();
        waiting = true;
    }
    protected override void OnUpdate()
    {
        if (waiting)
        {
            if (task.IsCompleted)
            {
                print(task.IsFaulted ? task.Exception.ToString() : task.Status.ToString());
                if (solver.status == GridSolver.Status.Finished) { solver.DisplayResult(); }
                waiting = false;
            }
            else { print($"Progress: {solver.progress}"); }
        }
    }
}