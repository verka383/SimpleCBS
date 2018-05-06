using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS
{
    /// <summary>
    /// Agent represented by id.
    /// Holds its start and goal position as NodeL.
    /// </summary>
    public class Agent
    {
        public Agent(int id, NodeL start, NodeL goal)
        {
            this.ID = id;
            this.start = start;
            this.goal = goal;
        }

        public int ID;
        public NodeL start;
        public NodeL goal;
    }

    /// <summary>
    /// Path for agent a found by LowLevelSearch
    /// </summary>
    public class Path
    {
        public Path(List<NodeL> path)
        {
            this.path = path;
        }
        public Path()
        {
            this.path = new List<NodeL>();
        }

        public int GetCost()
        {
            if (this.path == null) return int.MaxValue;
            return this.path.Count();
        }

        //Agent a;
        public List<NodeL> path;
        //public int cost;

        public override string ToString()
        {
            if (path == null) return "no solution";
            return string.Join(" ", path);
        }
    }


}
