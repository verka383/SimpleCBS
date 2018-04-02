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
            new CBS();
        }
    }

    class CBS
    {
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

        public CBS()
        {
            LoadMap("input2.txt");
            //List<Constraint> blabla = new List<Constraint>(new[] { new Constraint(agents[0], 6, 2), new Constraint(agents[1], 2, 4) });
            //Path r = LowLevelSearch(agents[0], blabla);
            //Path rr = LowLevelSearch(agents[1], blabla);
            List<Path> result = RunSearch();
            WriteOutput("output2", result);
        }

        /// <summary>
        /// Main search function.
        /// </summary>
        List<Path> RunSearch()
        {
            HashSet<TreeNode> visitedNodes = new HashSet<TreeNode>();
            TreeNode root = new TreeNode();
            root.constraints = new List<Constraint>();

            for (int i = 0; i < agents.Length; i++)
            {
                Path p = LowLevelSearch(agents[i], root.constraints);
                root.solution.Add(p);
                root.cost += p.GetCost();
            }

            openedNodes.Add(root);

            while (openedNodes.Count > 0)
            {

                Console.WriteLine(openedNodes.Count);
                TreeNode p = findMinH();
                if (p.solution[0].ToString().StartsWith("3 3") && p.solution[1].ToString().StartsWith("5 4"))
                {
                }
                /* bool find = p.solution[0].path.Where(x => x.id == 2).Count() > 0;
                 bool find2 = p.solution[1].path.Where(x => x.id == 2).Count() > 0;
                 if (find || find2)
                     break;

             */
                openedNodes.Remove(p);
                visitedNodes.Add(p);

                if (p.solution[0].path == null || p.solution[1].path == null) continue;
                // detection of all conflicts
                List<Conflict> conflicts = new List<Conflict>();

                for (int i = 0; i < p.solution.Count; i++)
                {
                    for (int j = i+1; j < p.solution.Count; j++)
                    {
                        conflicts.AddRange(detectConflicts(agents[i], agents[j], p.solution[i], p.solution[j]));
                    }
                }

                if (conflicts.Count == 0)
                {
                    return p.solution;
                }
                else
                {
                    foreach (Conflict c in conflicts)
                    {
                        if (typeof(Crash) != c.GetType())
                        {
                            foreach (Agent a in new[] { c.a1, c.a2 })
                            {
                                TreeNode newNode = new TreeNode();
                                newNode.solution = new List<Path>(p.solution);
                                newNode.constraints = new List<Constraint>(p.constraints);
                                newNode.constraints.Add(new Constraint(a, c.nodeId, c.timeStep));
                                newNode.cost = p.cost;
                                newNode.cost -= (newNode.solution[a.ID]).GetCost();
                                var paths = LowLevelSearchBFS(a, newNode.constraints);
                                newNode.solution[a.ID] = new Path(paths[0]);
                                newNode.cost += (newNode.solution[a.ID]).GetCost();
                                if(!visitedNodes.Contains(newNode))
                                    openedNodes.Add(newNode);
                            }
                        }
                        else
                        {
                            Crash cc = (Crash)c;                            
                            {
                                var newConstrains = new List<Constraint>(p.constraints);
                                ;
                                newConstrains.Add(new Constraint(c.a1, cc.secondID, cc.timeStep));
                                // lets not allow second robot to stay at its place
                                newConstrains.Add(new Constraint(c.a2, cc.secondID, cc.timeStep+1));
                                newConstrains.Add(new Constraint(c.a2, cc.firstID, cc.timeStep));
                                newConstrains.Add(new Constraint(c.a2, cc.secondID, cc.timeStep));
                               
                                var paths1 = LowLevelSearchBFS(cc.a1, newConstrains);
                                var paths2 = LowLevelSearchBFS(cc.a2, newConstrains);
                                foreach (var p1 in paths1)
                                {
                                    foreach (var p2 in paths2)
                                    {
                                        TreeNode newNode = new TreeNode();
                                        newNode.solution = new List<Path>(p.solution);
                                        newNode.constraints = new List<Constraint>(newConstrains);
                                        newNode.cost = p.cost;
                                        newNode.cost -= (newNode.solution[cc.a1.ID]).GetCost();
                                        newNode.cost -= (newNode.solution[cc.a2.ID]).GetCost();
                                        newNode.solution[cc.a1.ID] = new Path(p1);
                                        newNode.solution[cc.a2.ID] = new Path(p2);
                                        newNode.cost += (newNode.solution[cc.a1.ID]).GetCost();
                                        newNode.cost += (newNode.solution[cc.a2.ID]).GetCost();
                                        if (!visitedNodes.Contains(newNode))
                                            openedNodes.Add(newNode);
                                    }
                                }
                               
                            }
                           /* {
                                TreeNode newNode = new TreeNode();
                                newNode.solution = new List<Path>(p.solution);
                                newNode.constraints = new List<Constraint>(p.constraints);
                                //newNode.constraints.Add(new Constraint(c.a1, cc.secondID, cc.timeStep));
                                // lets not allow second robot to stay at its place
                                newNode.constraints.Add(new Constraint(c.a2, cc.firstID, cc.timeStep));
                                newNode.constraints.Add(new Constraint(c.a2, cc.secondID, cc.timeStep));
                                newNode.cost = p.cost;
                                newNode.cost -= (newNode.solution[cc.a1.ID]).GetCost();
                                newNode.cost -= (newNode.solution[cc.a2.ID]).GetCost();
                                
                                newNode.solution[cc.a1.ID] = new Path(LowLevelSearchBFS(cc.a1, newNode.constraints));
                                newNode.solution[cc.a2.ID] = new Path(LowLevelSearchBFS(cc.a2, newNode.constraints));
                                newNode.cost += (newNode.solution[cc.a1.ID]).GetCost();
                                newNode.cost += (newNode.solution[cc.a2.ID]).GetCost();
                                if (!visitedNodes.Contains(newNode))
                                    openedNodes.Add(newNode);
                            }*/
                        }
                    }
                }
            }

            return null;
        }

        List<Conflict> detectConflicts(Agent a1, Agent a2, Path p1, Path p2)
        {
            int min = p1.path.Count > p2.path.Count ? p2.path.Count : p1.path.Count;
            List<Conflict> conflict = new List<Conflict>();


            // agents are atempting to switch positions
            for (int i = 0; i < min - 1; i++)
            {
                if (p1.path[i].id == p2.path[i+1].id && p2.path[i].id == p1.path[i+1].id)
                {
                    conflict.Add(new Crash(a1,a2, p1.path[i].id, p1.path[i + 1].id, i+1));
                    //conflict.Add(new Crash(a2, p2.path[i].id, p2.path[i + 1].id, i));
                    //return conflict;
                }
            }


            for (int i = 0; i < min; i++)
            {
                if (p1.path[i].id == p2.path[i].id)
                {
                    conflict.Add(new Conflict(a1, a2, p1.path[i].id, i));
                    //return conflict;
                }
            }

            return conflict;
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
                    grid[i] = new NodeL(i,  i%width, i / width);
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

            while((line = sr.ReadLine()) != null)
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
        /// Node of the grid (low level).
        /// </summary>
        class NodeL
        {
            public NodeL(int id, int x, int y)
            {
                this.id = id;
                this.x = x;
                this.y = y;
                this.neighbours = new List<NodeL>();
            }

            public int id;
            public int x;
            public int y;
            public List<NodeL> neighbours;
            public NodeL previous;
            public int time;

            public override string ToString()
            {
                return id.ToString();
            }
        }
        
        /// <summary>
        /// Agent represented by id.
        /// Holds its start and goal position as NodeL.
        /// </summary>
        class Agent
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
        class Path
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

        /// <summary>
        /// Searches path in grid for agent a.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        Path LowLevelSearch(Agent a, List<Constraint> cs)
        {
            // counter of time steps
            int time = -1;

            // closed nodes
            HashSet<NodeL> closed = new HashSet<NodeL>();

            // opened nodes
            HashSet<NodeL> opened = new HashSet<NodeL>();
            opened.Add(a.start);

            // key node can be reached from value node the most efficiently (most efficient previous step)
            Dictionary<NodeL, NodeL> cameFrom = new Dictionary<NodeL, NodeL>();

            // for each node, the cost of getting from the start node to that node
            Dictionary<NodeL, int> gScore = new Dictionary<NodeL, int>();
            gScore.Add(a.start, 0);

            // for each node, total cost of getting from start to goal through that node
            // consists of gScore and heuristic estimate
            Dictionary<NodeL, int> fScore = new Dictionary<NodeL, int>();
            fScore.Add(a.start, heuristic(a.start, a.goal));

            // main loop
            while(opened.Count > 0)
            {
                time++; 

                NodeL current = findMin(fScore, opened);

                if (current.id == a.goal.id)
                {
                    return recontructPath(cameFrom, a.goal, cs, a.ID);
                }

                opened.Remove(current);
                closed.Add(current);
                bool allConstraintsOK = true;

                var nodes = new List<NodeL>(current.neighbours);
               // nodes.Add(current);
                // go through all neighbours of the current node
                foreach (NodeL neighbour in nodes)
                {
                    // control if it is not closed
                    if (closed.Contains(neighbour))
                    {
                        continue;
                    }

                    bool isOK = true;

                    // control if there is not conflict with constraints
                    foreach (Constraint c in cs)
                    {
                        if (c.a == a && c.timeStep == time && c.nodeId == neighbour.id)
                        {
                            isOK = false;
                            allConstraintsOK = false;
                        }
                    }

                    if (!isOK)
                    {
                        continue;
                    }

                    // add node to opened
                    if (!opened.Contains(neighbour))
                    {
                        opened.Add(neighbour);
                    }

                    // recomputation
                    int tentative_gScore = gScore[current] + 1;

                    if (gScore.ContainsKey(neighbour) && tentative_gScore >= gScore[neighbour])
                    {
                        continue;
                    }
                    else // found better past - record it
                    {
                        // refresh cameFrom
                        if (cameFrom.ContainsKey(neighbour))
                        {
                            cameFrom[neighbour] = current;
                        }
                        else
                        {
                            cameFrom.Add(neighbour, current);
                        }

                        // refresh gScore
                        if (gScore.ContainsKey(neighbour))
                        {
                            gScore[neighbour] = tentative_gScore;
                        }
                        else
                        {
                            gScore.Add(neighbour, tentative_gScore);
                        }

                        // refresh fScore
                        if (fScore.ContainsKey(neighbour))
                        {
                            fScore[neighbour] = gScore[neighbour] + heuristic(neighbour, a.goal);
                        }
                        else
                        {
                            fScore.Add(neighbour, gScore[neighbour] + heuristic(neighbour, a.goal));
                        }
                    }

                }

                if (!allConstraintsOK)
                {
                    closed.Remove(current);
                    opened.Add(current);
                    gScore[current]++;
                    fScore[current]++;
                }
            }

            return null;
        }


        List<List<NodeL>> LowLevelSearchBFS(Agent a, List<Constraint> cs)
        {
            int solutionTime = int.MaxValue;
            var paths = new List<List<NodeL>>();
            int time = 0;           
            Queue<NodeL> open = new Queue<NodeL>();
            open.Enqueue(a.start);
            while (open.Any())
            {
                time++;
                var current = open.Dequeue();

                if (current.time > solutionTime) continue;
                if (current.id == a.goal.id)
                {
                    if (current.time > solutionTime) continue;
                    solutionTime = current.time;
                    // found path
                   
                    var path = new List<NodeL>();
                   
                    var temp = current;
                    while (temp != null)
                    {
                        path.Add(temp);
                        temp = temp.previous;
                    }
                    path.Reverse();
                    paths.Add(path);
                }
                else
                {
                    foreach (var item in current.neighbours)
                    {
                        bool found = cs.Any(x => x.timeStep == (current.time+1) && a.ID == x.a.ID && x.nodeId == item.id);
                        if (found)
                            continue;
                        if (item.id == a.goal.id)
                        {
                            if (current.time > solutionTime) continue;
                            solutionTime = current.time;                           
                            // found path
                            var path = new List<NodeL>();
                            path.Add(item);
                            var temp = current;
                            while (temp != null)
                            {
                                path.Add(temp);
                                temp = temp.previous;
                            }
                            path.Reverse();
                            paths.Add(path);

                        }
                        else
                        {
                            var newNode1 = new NodeL(item.id, item.x, item.y);
                            newNode1.neighbours = new List<NodeL>(item.neighbours);
                            newNode1.previous = current;
                            newNode1.time = current.time + 1;
                            open.Enqueue(newNode1);
                        }
                    }
                    if (!cs.Any(x => x.timeStep == (current.time + 1) && a.ID == x.a.ID && x.nodeId == current.id))
                    {
                        // cycle also fine
                        var newNode = new NodeL(current.id, current.x, current.y);
                        newNode.previous = current;
                        newNode.neighbours = new List<NodeL>(current.neighbours);
                        newNode.time = current.time + 1;
                        open.Enqueue(newNode);
                    }
                }
            }
            return paths;
        }

        /// <summary>
        /// Heuristic function - count Manhattan metrics between given nodes.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        int heuristic(NodeL n, NodeL goal)
        {
            return Math.Abs(n.x - goal.x) + Math.Abs(n.y - goal.y);
        }

        /// <summary>
        /// Reconstructs found path.
        /// </summary>
        /// <param name="cameFrom"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        Path recontructPath(Dictionary<NodeL, NodeL> cameFrom, NodeL current, List<Constraint> c, int id)
        {
            Path p = new Path();
            p.path.Add(current);

            while(cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                p.path.Add(current);
            }

            p.path.Reverse();

            for (int i = 0; i < p.path.Count; i++)
            {
                if (c.Find(item => item.a.ID == id && item.timeStep == i && item.nodeId == p.path[i].id) != null)
                {
                    p.path.Insert(i, p.path[i-1]);
                    i++;
                }
            }

            return p;
        }

        /// <summary>
        /// Finds node in opened with the lowest fScore value.
        /// </summary>
        /// <param name="fScore"></param>
        /// <param name="opened"></param>
        /// <returns></returns>
        NodeL findMin(Dictionary<NodeL, int> fScore, HashSet<NodeL> opened)
        {
            int min = int.MaxValue;
            NodeL minIndex = null;

            foreach (NodeL n in fScore.Keys)
            {
                if (opened.Contains(n) && fScore[n] < min)
                {
                    min = fScore[n];
                    minIndex = n;
                }
            }

            return minIndex;
        }

#endregion
        /////////////////////////////////////////////

        /// <summary>
        /// Constraint that agent a can not be at node with nodeId at given timestep.
        /// </summary>
        class Constraint: IEquatable<Constraint>
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

            public override string ToString()
            {
                return string.Format("Agent {0} time {1} cant go to {2}",a.ID,timeStep,nodeId);
            }
        }

        /// <summary>
        /// Agents a1 and a2 have conflict at nodeId at given timestep.
        /// </summary>
        class Conflict
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

        class Crash : Conflict
        {
            public Crash(Agent a, Agent b, int f, int s, int t): base(a, b, s, t)
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
        
        /// <summary>
        /// Node of the Constraint Tree.
        /// </summary>
        class TreeNode:IEquatable<TreeNode>
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
                return cost.GetHashCode() + (solution[0].ToString() + solution[1].ToString()).GetHashCode();
            }

            bool ConstrainsEqual(List<Constraint> a, List<Constraint> b)
            {
                if (a.Count != b.Count) return false;
                for(int i=0;i<a.Count;i++)
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
                int first = to.x - from.x;
                int second = to.y - from.y;

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
                int first = to.x - from.x;
                int second = to.y - from.y;

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
                int first = to.x - from.x;
                int second = to.y - from.y;

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
                int first = to.x - from.x;
                int second = to.y - from.y;

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

        void WriteOutput(string file, List<Path> result)
        {
            int id = 0;

            foreach (Path p in result)
            {
                StreamWriter sw = new StreamWriter(file + "-" + id + ".txt");
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
}
