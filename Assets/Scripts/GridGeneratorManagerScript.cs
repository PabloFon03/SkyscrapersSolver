using System.Threading.Tasks;

public class GridGeneratorManagerScript : VisualGridManager
{
    protected override void OnStart()
    {
        task = new Task(solver.GenerateGrid);
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
                if (solver.status == GridSolver.Status.Finished)
                {
                    solver.DisplayResult();
                    solver.DisplayEdgeSigns();
                }
                waiting = false;
            }
            else
            {
                solver.DisplayResult();
                solver.DisplayEdgeSigns();
            }
        }
    }
}