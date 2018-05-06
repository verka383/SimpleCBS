using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new CBSSolver(@"C:\Users\noha\Documents\BioinformatikaMgr\AI Semniar\vera_test.txt", new AStarSearch());
            var path = solver.RunSearch();
            solver.WriteOutput("vera_test output", path);
        }
    }

    public class CBSSolver
    {
        ILowLevelSearch lowLevelSearch;
        /// <summary>
        /// Map of the world we are searching in.
        /// </summary>
        NodeL[] grid;

        /// <summary>
        /// Agents with start and goal positions.
        /// </summary>
        Agent[] agents;

        /// <summary>
        /// Opened nodes in high-level search, sorted by cost of paths.
        /// </summary>
        HashSet<TreeNode> openedNodes = new HashSet<TreeNode>();

        public CBSSolver(string input, ILowLevelSearch lowLevelSearch)
        {
            this.lowLevelSearch = lowLevelSearch;
            LoadMap(input);           
        }



        /// <summary>
        /// Main search function.
        /// </summary>
        public List<Path> RunSearch()
        {
            HashSet<TreeNode> visitedNodes = new HashSet<TreeNode>();
            var root = new TreeNode();
            root.constraints = new List<Constraint>();

            for (int i = 0; i < agents.Length; i++)
            {
                // Path p = new Path(LowLevelSearchBFS(agents[i], root.constraints));
                Path p = lowLevelSearch.LowLevelSearch(agents[i], root.constraints);
                root.solution.Add(p);
                root.cost += p.GetCost();
            }

            openedNodes.Add(root);

            while (openedNodes.Count > 0)
            {
                //   Console.WriteLine(openedNodes.Count);
                TreeNode p = findMinH();

                openedNodes.Remove(p);
                visitedNodes.Add(p);

                if (p.solution[0].path == null || p.solution[1].path == null)
                    continue;

                // detection of all conflicts
                Conflict conflict = null;

                for (int i = 0; i < p.solution.Count; i++)
                {
                    if (conflict != null) break;
                    for (int j = i + 1; j < p.solution.Count; j++)
                    {
                        conflict = detectConflict(agents[i], agents[j], p.solution[i], p.solution[j]);                        
                        if(conflict!=null)break;
                    }
                }

                if (conflict == null)
                {
                    return p.solution;
                }
                else
                {
                    var c = conflict;

                    foreach (Agent a in new[] { c.a1, c.a2 })
                    {
                        TreeNode newNode = new TreeNode();
                        newNode.solution = new List<Path>(p.solution);
                        newNode.constraints = new List<Constraint>(p.constraints);
                        var newConstrain = new Constraint(a, c.nodeId, c.timeStep);
                        if (newNode.constraints.Contains(newConstrain))
                            //visitedNodes.Add(newNode);
                           continue;
                        newNode.constraints.Add(newConstrain);
                        newNode.cost = p.cost;
                        newNode.cost -= (newNode.solution[a.ID]).GetCost();
                        // var paths = LowLevelSearchBFS(a, newNode.constraints);
                        // if (paths.Count == 0) continue;
                        //  newNode.solution[a.ID] = new Path(paths[0]);
                        var path = lowLevelSearch.LowLevelSearch(a, newNode.constraints);
                        if (path == null)
                        {
                            //visitedNodes.Add(newNode);
                            
                            continue;
                        }
                        newNode.solution[a.ID] = path;
                        newNode.cost += (newNode.solution[a.ID]).GetCost();
                        if (!visitedNodes.Contains(newNode))
                            openedNodes.Add(newNode);
                       // visitedNodes.Add(newNode);
                    }
                }
            }

            return null;
        }       

        Conflict detectConflict(Agent a1, Agent a2, Path p1, Path p2)
        {
            int min = Math.Min(p1.path.Count, p2.path.Count);
            // agents are atempting to switch positions
            for (int i = 0; i < min-1; i++)
            {
                if (p1.path[i].id == p2.path[i + 1].id && p2.path[i].id == p1.path[i + 1].id)
                {
                   return new Crash(a1, a2, p1.path[i].id, p1.path[i + 1].id, i + 1);
                }
            }


            for (int i = 0; i < min; i++)
            {
                if (p1.path[i].id == p2.path[i].id)
                {
                    return new Conflict(a1, a2, p1.path[i].id, i);
                }
            }

            return null;
        }

        // find node with min cost, prefer node with min constrains
        TreeNode findMinH()
        {
            int min = int.MaxValue;
            int minConstrains = int.MaxValue;
            TreeNode minIndex = null;

            foreach (TreeNode n in openedNodes)
            {
                if (n.cost < min)
                {
                    min = n.cost;
                    minConstrains = n.constraints.Count;
                    minIndex = n;
                }
                if (n.cost == min && n.constraints.Count < minConstrains)
                {
                    minIndex = n;
                    minConstrains = n.constraints.Count;
                }
            }


            return minIndex;
        }

        /// <summary>
        /// Loads map and start and goal positions from given file.
        /// </summary>
        /// <param name="file"></param>
        void LoadMap(string file)
        {
            StreamReader sr = new StreamReader(file);
            string line = sr.ReadLine();
            int width, height;
            parseLine(line, out width, out height);
            int size = width * height;
            grid = new NodeL[size];

            // for simplicity: initialize all fields of the grid - if any field is not available, than it will have no neighbours and it is unreachable
            for (int i = 0; i < size; i++)
            {
                grid[i] = new NodeL(i, i % width, i / width);
            }

            // add to all nodes their neighbours
            while ((line = sr.ReadLine()) != "X")
            {
                int firstNode, secondNode;
                parseLine(line, out firstNode, out secondNode);
                grid[firstNode].neighbours.Add(grid[secondNode]);
                grid[secondNode].neighbours.Add(grid[firstNode]);
            }

            // creates all agents
            List<Agent> agents = new List<Agent>();

            while ((line = sr.ReadLine()) != null)
            {
                int start, goal;
                parseLine(line, out start, out goal);
                agents.Add(new Agent(agents.Count, grid[start], grid[goal]));
            }

            this.agents = agents.ToArray();
        }

        /// <summary>
        /// Parses two integers from line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        void parseLine(string line, out int first, out int second)
        {
            string[] splitted;
            splitted = line.Split(' ');
            first = int.Parse(splitted[0]);
            second = int.Parse(splitted[1]);
        }

        #region LowLevel


       

        
        /// <summary>
        /// Reconstructs found path.
        /// </summary>
        /// <param name="cameFrom"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        Path recontructPath(Dictionary<NodeL, NodeL> cameFrom, NodeL current, /*List<Constraint> c,*/ int id)
        {
            Path p = new Path();
            p.path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                p.path.Add(current);
            }

            p.path.Reverse();

            /*for (int i = 0; i < p.path.Count; i++)
            {
                if (c.Find(item => item.a.ID == id && item.timeStep == i && item.nodeId == p.path[i].id) != null)
                {
                    p.path.Insert(i, p.path[i - 1]);
                    i++;
                }
            }*/

            return p;
        }

     

        #endregion
        /////////////////////////////////////////////
    

        /// <summary>
        /// Node of the Constraint Tree.
        /// </summary>
        class TreeNode : IEquatable<TreeNode>
        {
            public TreeNode()
            {
                solution = new List<Path>();
                cost = 0;
            }

            public List<Constraint> constraints;
            public List<Path> solution;
            public int cost;

            public override int GetHashCode()
            {
                return string.Join("",constraints.Distinct().Select(x => x.ToString())).GetHashCode(); // cost.GetHashCode() + (solution[0].ToString() + solution[1].ToString()).GetHashCode();
            }

            bool ConstrainsEqual(List<Constraint> a, List<Constraint> b)
            {
                if (a.Count != b.Count) return false;
                for (int i = 0; i < a.Count; i++)
                {
                    if (a[i].a != b[i].a) return false;
                    if (a[i].nodeId != b[i].nodeId) return false;
                    if (a[i].timeStep != b[i].timeStep) return false;
                }
                return true;
            }
            public bool Equals(TreeNode other)
            {
                return other.cost == this.cost
                    && other.solution[0].ToString() == this.solution[0].ToString()
                    && other.solution[1].ToString() == this.solution[1].ToString()
                    && ConstrainsEqual(other.constraints, this.constraints);
            }
        }

        /// <summary>
        /// Child classes represent possible view on the grid (like ozobot). The name of the child specifies the direction ozobot is looking from the beginning.
        /// </summary>
        class View
        {
            public static View detectView(NodeL from, NodeL to)
            {
                int first = to.x - from.x;
                int second = to.y - from.y;

                if (first > 0)
                {
                    return new DownView();
                }
                else if (first < 0)
                {
                    return new TopView();
                }
                else if (second > 0)
                {
                    return new RightView();
                }
                else
                {
                    return new LeftView();
                }
            }

            public virtual DIRECTION translateMove(NodeL from, NodeL to, ref View v)
            {
                return 0;
            }

        }

        class LeftView : View
        {
            public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
            {
                int second = to.x - from.x;
                int first = to.y - from.y;

                if (first > 0) // dolu
                {
                    v = new DownView();
                    return DIRECTION.DIRECTION_LEFT;
                }
                else if (first < 0) // nahoru
                {
                    v = new TopView();
                    return DIRECTION.DIRECTION_RIGHT;
                }
                else if (second > 0) // doprava
                {
                    v = new RightView();
                    return DIRECTION.DIRECTION_BACKWARD;
                }
                else if (second < 0) // doleva
                {
                    return DIRECTION.DIRECTION_FORWARD;
                }
                else
                {
                    return DIRECTION.WAIT;
                }
            }
        }

        class RightView : View
        {
            public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
            {
                int second = to.x - from.x;
                int first = to.y - from.y;

                if (first > 0) // dolu
                {
                    v = new DownView();
                    return DIRECTION.DIRECTION_RIGHT;
                }
                else if (first < 0) // nahoru
                {
                    v = new TopView();
                    return DIRECTION.DIRECTION_LEFT;
                }
                else if (second > 0) // doprava
                {
                    return DIRECTION.DIRECTION_FORWARD;
                }
                else if (second < 0) // doleva
                {
                    v = new LeftView();
                    return DIRECTION.DIRECTION_BACKWARD;
                }
                else
                {
                    return DIRECTION.WAIT;
                }
            }
        }

        class TopView : View
        {
            public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
            {
                int second = to.x - from.x;
                int first = to.y - from.y;

                if (first > 0) // dolu
                {
                    v = new DownView();
                    return DIRECTION.DIRECTION_BACKWARD;
                }
                else if (first < 0) // nahoru
                {
                    return DIRECTION.DIRECTION_FORWARD;
                }
                else if (second > 0) // doprava
                {
                    v = new RightView();
                    return DIRECTION.DIRECTION_RIGHT;
                }
                else if (second < 0)// doleva
                {
                    v = new LeftView();
                    return DIRECTION.DIRECTION_LEFT;
                }
                else
                {
                    return DIRECTION.WAIT;
                }
            }
        }

        class DownView : View
        {
            public override DIRECTION translateMove(NodeL from, NodeL to, ref View v)
            {
                int second = to.x - from.x;
                int first = to.y - from.y;

                if (first > 0) // dolu
                {
                    return DIRECTION.DIRECTION_FORWARD;
                }
                else if (first < 0) // nahoru
                {
                    v = new TopView();
                    return DIRECTION.DIRECTION_BACKWARD;
                }
                else if (second > 0) // doprava
                {
                    v = new RightView();
                    return DIRECTION.DIRECTION_LEFT;
                }
                else if (second < 0) // doleva
                {
                    v = new LeftView();
                    return DIRECTION.DIRECTION_RIGHT;
                }
                else
                {
                    return DIRECTION.WAIT;
                }
            }

        }

        enum DIRECTION
        {
            DIRECTION_LEFT = 2, DIRECTION_RIGHT = 4, DIRECTION_FORWARD = 1, DIRECTION_BACKWARD = 8, WAIT = 0
        }

        public void WriteHumanReadableOutput(string file, List<Path> result)
        {           
            //var longest = result.Max(x => x.path.Count);
            using (var writer = new StreamWriter(file))
            {
                //writer.WriteLine("t\t" + string.Join("\t", Enumerable.Range(0,longest)));
                foreach (var p in result)
                {
                    for (int i = 0; i < p.path.Count - 1; i++)
                    {
                        var dx = p.path[i].x - p.path[i + 1].x;
                        var dy = p.path[i].y - p.path[i + 1].y;
                        if (dx > 0) writer.Write(" < ");
                        else if (dx < 0) writer.Write(" > ");
                        else if (dy > 0) writer.Write(" ^ ");
                        else if (dy < 0) writer.Write(" V ");
                        else writer.Write(" o ");
                    }
                    writer.WriteLine();
                }
            }
        }

        public void WriteOutput(string file, List<Path> result)
        {
            int id = 0;

            foreach (Path p in result)
            {
                var fileName = file + "-" + id + ".txt";
                StreamWriter sw = new StreamWriter(fileName);
                //View v = View.detectView(p.path[0], p.path[1]);
                View v = new RightView();

                for (int i = 0; i < p.path.Count - 1; i++)
                {
                    sw.WriteLine((int)v.translateMove(p.path[i], p.path[i + 1], ref v));
                }

                sw.Close();
                id++;
            }
        }
    }

    /// <summary>
    /// Constraint that agent a can not be at node with nodeId at given timestep.
    /// </summary>
    public class Constraint : IEquatable<Constraint>
    {
        public Constraint(Agent a, int id, int time)
        {
            this.a = a;
            this.nodeId = id;
            this.timeStep = time;
        }
        public Agent a;
        public int nodeId;
        public int timeStep;

        public bool Equals(Constraint other)
        {
            if (a.ID != other.a.ID) return false;
            if (this.nodeId != other.nodeId) return false;
            if (this.timeStep != other.timeStep) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Constraint;
            if (other == null) return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() * (timeStep + 1) * (nodeId + 1);
        }
        public override string ToString()
        {
            return string.Format("Agent {0} time {1} cant go to {2}", a.ID, timeStep, nodeId);
        }
    }

    /// <summary>
    /// Agents a1 and a2 have conflict at nodeId at given timestep.
    /// </summary>
    public class Conflict
    {
        public Conflict(Agent a1, Agent a2, int id, int time)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.nodeId = id;
            this.timeStep = time;
        }

        public Agent a1;
        public Agent a2;
        public int nodeId;
        public int timeStep;
    }

    public class Crash : Conflict
    {
        public Crash(Agent a, Agent b, int f, int s, int t) : base(a, b, s, t)
        {
            //this.a = a;
            this.firstID = f;
            this.secondID = s;
            this.timeStep = t;
        }

        //public Agent a;
        public int firstID;
        public int secondID;
    }
}
