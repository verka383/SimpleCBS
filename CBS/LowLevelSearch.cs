using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CBS.CBSSolver;

namespace CBS
{
    public interface ILowLevelSearch
    {
        Path LowLevelSearch(Agent a, List<Constraint> cs);
    }

    /// <summary>
    /// Node of the grid (low level).
    /// </summary>
    public class NodeL
    {
        public NodeL(int id, int x, int y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.neighbours = new List<NodeL>();
        }

        public NodeL(int id, int x, int y, int time, IEnumerable<NodeL> neighbours, NodeL previous)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.time = time;
            this.previous = previous;
            this.neighbours = new List<NodeL>(neighbours);
        }

        public readonly int id;
        public readonly int x;
        public readonly int y;
        public List<NodeL> neighbours;
        public NodeL previous;
        public readonly int time;

        public override string ToString()
        {
            return id.ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as NodeL;
            return (other.id == this.id) && (other.time == this.time);
            //return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (1 + id) * (1 + time);
        }
    }


    public class AStarSearch : ILowLevelSearch
    {
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
        /// <summary>
        /// Searches path in grid for agent a.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Path LowLevelSearch(Agent a, List<Constraint> cs)
        {
            const int MAX = 100;
            // closed nodes
            HashSet<NodeL> closed = new HashSet<NodeL>();

            // opened nodes
            HashSet<NodeL> opened = new HashSet<NodeL>();
            opened.Add(a.start);

            // key node can be reached from value node the most efficiently (most efficient previous step)
            //Dictionary<NodeL, NodeL> cameFrom = new Dictionary<NodeL, NodeL>();

            // for each node, the cost of getting from the start node to that node
            Dictionary<NodeL, int> gScore = new Dictionary<NodeL, int>();
            gScore.Add(a.start, 0);

            // for each node, total cost of getting from start to goal through that node
            // consists of gScore and heuristic estimate
            Dictionary<NodeL, int> fScore = new Dictionary<NodeL, int>();
            fScore.Add(a.start, heuristic(a.start, a.goal));

            // main loop
            while (opened.Count > 0)
            {

                NodeL current = findMin(fScore, opened);

                // goal test
                if (current.id == a.goal.id)
                {
                    var path = new List<NodeL>();

                    var temp = current;
                    while (temp != null)
                    {
                        path.Add(temp);
                        temp = temp.previous;
                    }
                    // path.Reverse();
                    return new Path(path);
                    // return recontructPath(cameFrom, a.start,/* cs,*/ a.ID);
                }

                opened.Remove(current);

                if (current.time > MAX) continue;
                // A* will never finish if there is no solution
                // because of the time, we will never visit all nodes
                // solution ?? limit time to max time of conflict? after that keep time constant => no duplicate nodes

                closed.Add(current);
                
                var nodes = current.neighbours.Select(x => new NodeL(x.id, x.x, x.y, current.time + 1, x.neighbours, current)).ToList();

                var currentNode = new NodeL(current.id, current.x, current.y, current.time + 1, current.neighbours, current);

                nodes.Add(currentNode);
                // go through all neighbours of the current node
                foreach (NodeL neighbour in nodes)
                {

                    if (cs.Any(x => x.timeStep == (current.time + 1) && a.ID == x.a.ID && x.nodeId == neighbour.id))
                        continue;

                    // goal test
                    if (neighbour.id == a.goal.id)
                    {
                        var path = new List<NodeL>();

                        var temp = neighbour;
                        while (temp != null)
                        {
                            path.Add(temp);
                            temp = temp.previous;
                        }
                         path.Reverse();
                        return new Path(path);
                        // return recontructPath(cameFrom, a.start,/* cs,*/ a.ID);
                    }
                    //neighbour.time++;
                    // control if it is not closed
                    if (closed.Contains(neighbour))
                    {
                        continue;
                    }

                    /*bool isOK = true;

                    // control if there is not conflict with constraints
                    foreach (Constraint c in cs)
                    {
                        if (c.a == a && c.timeStep == neighbour.time && c.nodeId == neighbour.id)
                        {
                            isOK = false;
                            allConstraintsOK = false;
                            break;
                        }
                    }

                    if (!isOK)
                    {
                        continue;
                    }
                    */
                    //neighbour.previous = currentNode;
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
                        /*if (cameFrom.ContainsKey(neighbour))
                        {
                            cameFrom[neighbour] = current;
                        }
                        else
                        {
                            cameFrom.Add(neighbour, current);
                        }*/

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
                            fScore[neighbour] = gScore[neighbour] + heuristic(neighbour, a.start);
                        }
                        else
                        {
                            fScore.Add(neighbour, gScore[neighbour] + heuristic(neighbour, a.start));
                        }
                    }

                }

                /*if (!allConstraintsOK)
                {
                    closed.Remove(current);
                    opened.Add(current);
                    gScore[current]++;
                    fScore[current]++;
                }*/
            }

            return null;
        }
    }

    public class BFSSearch : ILowLevelSearch
    {
        public Path LowLevelSearch(Agent a, List<Constraint> cs)
        {
            int solutionTime = int.MaxValue;
            var paths = new List<List<NodeL>>();
            int time = 0;
            Queue<NodeL> open = new Queue<NodeL>();
            HashSet<NodeL> closed = new HashSet<NodeL>(); // todo use this?
            open.Enqueue(a.start);
            while (open.Any())
            {
                time++;
                var current = open.Dequeue();
                closed.Add(current);
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
                    return new Path(path);
                }
                else
                {
                    foreach (var item in current.neighbours)
                    {
                        bool found = cs.Any(x => x.timeStep == (current.time + 1) && a.ID == x.a.ID && x.nodeId == item.id);
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
                            return new Path(path);

                        }
                        else
                        {
                            if (current.previous!= null && item.id == current.previous.id)
                                continue;// do not allow immediately going back
                            var newNode1 = new NodeL(item.id, item.x, item.y,current.time+1,item.neighbours,current);
                            //newNode1.neighbours = new List<NodeL>(item.neighbours);
                            //newNode1.previous = current;.
                            //newNode1.time = current.time + 1;
                            if(!closed.Contains(newNode1))
                                open.Enqueue(newNode1);
                        }
                    }
                    if (!cs.Any(x => x.timeStep == (current.time + 1) && a.ID == x.a.ID && x.nodeId == current.id))
                    {
                        // cycle also fine
                        var newNode = new NodeL(current.id, current.x, current.y,current.time+1,current.neighbours,current);
                        //newNode.previous = current;
                        //newNode.neighbours = new List<NodeL>(current.neighbours);
                        //newNode.time = current.time + 1;
                        open.Enqueue(newNode);
                    }
                }
            }
            return null;
        }
    }
}
