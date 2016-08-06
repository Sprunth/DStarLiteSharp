using System;
using System.Collections.Generic;
using C5;

namespace DStarLiteSharp
{
    public class DStarLite
    {
        private readonly bool allowDiagonalPathing;

        private readonly double C1;

        // Constants
        private readonly double M_SQRT2 = Math.Sqrt(2);

        private readonly int maxSteps;

        private readonly Dictionary<State, float> openHash = new Dictionary<State, float>();

        private readonly IPriorityQueue<State> openList = new IntervalHeap<State>();
        // Private Member variables
        private readonly List<State> path = new List<State>();

        private readonly State s_goal = new State();

        // Change back to private****
        public Dictionary<State, CellInfo> cellHash = new Dictionary<State, CellInfo>();

        private double k_m;

        private State s_last = new State();

        private State s_start = new State();

        // Default constructor
        public DStarLite(int maxSteps = 80000, bool allowDiagonalPathing = true)
        {
            this.maxSteps = maxSteps;
            this.allowDiagonalPathing = allowDiagonalPathing;
            C1 = 1;
        }

        // Calculate Keys
        public void CalculateKeys()
        {
        }

        public void init(int sX, int sY, int gX, int gY)
        {
            cellHash.Clear();
            path.Clear();
            openHash.Clear();
            while (!openList.IsEmpty)
            {
                openList.DeleteMin();
            }

            k_m = 0;
            s_start.x = sX;
            s_start.y = sY;
            s_goal.x = gX;
            s_goal.y = gY;
            var tmp = new CellInfo();
            tmp.g = 0;
            tmp.rhs = 0;
            tmp.cost = C1;
            cellHash.Add(s_goal, tmp);
            tmp = new CellInfo();
            tmp.rhs = heuristic(s_start, s_goal);
            tmp.g = heuristic(s_start, s_goal);
            tmp.cost = C1;
            cellHash.Add(s_start, tmp);
            s_start = calculateKey(s_start);
            s_last = s_start;
        }

        private State calculateKey(State u)
        {
            var val = Math.Min(getRHS(u), getG(u));
            u.k.setFirst(val
                         + (heuristic(u, s_start) + k_m));
            u.k.setSecond(val);
            return u;
        }

        private double getRHS(State u)
        {
            if (u == s_goal)
            {
                return 0;
            }

            // if the cellHash doesn't contain the State u
            if (!cellHash.ContainsKey(u))
            {
                return heuristic(u, s_goal);
            }

            return cellHash[u].rhs;
        }

        private double getG(State u)
        {
            // if the cellHash doesn't contain the State u
            if (!cellHash.ContainsKey(u))
            {
                return heuristic(u, s_goal);
            }

            return cellHash[u].g;
        }

        private double heuristic(State a, State b)
        {
            return eightCondist(a, b) * C1;
        }

        private double eightCondist(State a, State b)
        {
            double temp;
            double min = Math.Abs(a.x - b.x);
            double max = Math.Abs(a.y - b.y);
            if (min > max)
            {
                temp = min;
                min = max;
                max = temp;
            }

            return (M_SQRT2 - 1)
                   * min
                   + max;
        }

        public bool replan()
        {
            path.Clear();
            var res = computeShortestPath();
            if (res < 0)
            {
                Console.WriteLine("No Path to Goal");
                return false;
            }

            var n = new System.Collections.Generic.LinkedList<State>();
            var cur = s_start;
            if (getG(s_start) == double.MaxValue)
            {
                Console.WriteLine("No Path to Goal");
                return false;
            }

            while (cur.neq(s_goal))
            {
                path.Add(cur);
                n = new System.Collections.Generic.LinkedList<State>();
                n = getSucc(cur);
                if (n.Count == 0)
                {
                    Console.WriteLine("No Path to Goal");
                    return false;
                }

                var cmin = double.MaxValue;
                double tmin = 0;
                var smin = new State();
                foreach (var i in n)
                {
                    var val = cost(cur, i);
                    var val2 = trueDist(i, s_goal) + trueDist(s_start, i);
                    val = val + getG(i);
                    if (close(val, cmin))
                    {
                        if (tmin > val2)
                        {
                            tmin = val2;
                            cmin = val;
                            smin = i;
                        }
                    }
                    else if (val < cmin)
                    {
                        tmin = val2;
                        cmin = val;
                        smin = i;
                    }
                }

                n.Clear();
                cur = new State(smin);
                // cur = smin;
            }

            path.Add(s_goal);
            return true;
        }

        private int computeShortestPath()
        {
            var s = new System.Collections.Generic.LinkedList<State>();
            if (openList.IsEmpty)
            {
                return 1;
            }

            var k = 0;
            // todo: check conversion of this while condition
            while (!openList.IsEmpty
                   && openList.FindMin().lt(s_start = calculateKey(s_start))
                   || (getRHS(s_start) != getG(s_start)))
            {
                if (k++ > maxSteps)
                {
                    Console.WriteLine("At maxsteps");
                    return -1;
                }
                State u;
                var test = getRHS(s_start) != getG(s_start);
                // lazy remove
                while (true)
                {
                    if (openList.IsEmpty)
                    {
                        return 1;
                    }

                    u = openList.DeleteMin();
                    if (!isValid(u))
                    {
                        // TODO: Warning!!! continue If
                    }

                    if (!u.lt(s_start)
                        && !test)
                    {
                        return 2;
                    }

                    break;
                }

                openHash.Remove(u);
                var k_old = new State(u);
                if (k_old.lt(calculateKey(u)))
                {
                    // u is out of date
                    insert(u);
                }
                else if (getG(u) > getRHS(u))
                {
                    // needs update (got better)
                    setG(u, getRHS(u));
                    s = getPred(u);
                    foreach (var i in s)
                    {
                        updateVertex(i);
                    }
                }
                else
                {
                    //  g <= rhs, state has got worse
                    setG(u, double.MaxValue);
                    s = getPred(u);
                    foreach (var i in s)
                    {
                        updateVertex(i);
                    }

                    updateVertex(u);
                }
            }

            Console.WriteLine($"Path found in {k} steps");

            // while
            return 0;
        }

        private System.Collections.Generic.LinkedList<State> getSucc(State u)
        {
            var s = new System.Collections.Generic.LinkedList<State>();
            State tempState;
            if (occupied(u))
            {
                return s;
            }

            // Generate the successors, starting at the immediate right,
            // Moving in a clockwise manner
            tempState = new State(u.x + 1, u.y, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x + 1), (u.y + 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            tempState = new State(u.x, u.y + 1, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x - 1), (u.y + 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            tempState = new State(u.x - 1, u.y, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x - 1), (u.y - 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            tempState = new State(u.x, u.y - 1, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x + 1), (u.y - 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            return s;
        }

        private System.Collections.Generic.LinkedList<State> getPred(State u)
        {
            var s = new System.Collections.Generic.LinkedList<State>();
            State tempState;
            tempState = new State(u.x + 1, u.y, new Pair<double, double>(-1, -1));
            if (!occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.x + 1, u.y + 1, new Pair<double, double>(-1, -1));
                if (!occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.x, u.y + 1, new Pair<double, double>(-1, -1));
            if (!occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.x - 1, u.y + 1, new Pair<double, double>(-1, -1));
                if (!occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.x - 1, u.y, new Pair<double, double>(-1, -1));
            if (!occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.x - 1, u.y - 1, new Pair<double, double>(-1, -1));
                if (!occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.x, u.y - 1, new Pair<double, double>(-1, -1));
            if (!occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.x + 1, u.y - 1, new Pair<double, double>(-1, -1));
                if (!occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            return s;
        }

        public void updateStart(int x, int y)
        {
            s_start.x = x;
            s_start.y = y;
            k_m = k_m + heuristic(s_last, s_start);
            s_start = calculateKey(s_start);
            s_last = s_start;
        }

        public void updateGoal(int x, int y)
        {
            var toAdd = new List<Pair<ipoint2, double>>();
            Pair<ipoint2, double> tempPoint;

            foreach (var entry in cellHash)
            {
                if (!close(entry.Value.cost, C1))
                {
                    tempPoint = new Pair<ipoint2, double>(new ipoint2(entry.Key.x, entry.Key.y),
                        entry.Value.cost);
                    toAdd.Add(tempPoint);
                }
            }

            cellHash.Clear();
            openHash.Clear();
            while (!openList.IsEmpty)
            {
                openList.DeleteMin();
            }

            k_m = 0;
            s_goal.x = x;
            s_goal.y = y;
            var tmp = new CellInfo();
            tmp.rhs = 0;
            tmp.g = 0;
            tmp.cost = C1;
            cellHash.Add(s_goal, tmp);
            tmp = new CellInfo();
            tmp.rhs = heuristic(s_start, s_goal);
            tmp.g = heuristic(s_start, s_goal);
            tmp.cost = C1;
            cellHash.Add(s_start, tmp);
            s_start = calculateKey(s_start);
            s_last = s_start;
            foreach (var pair in toAdd)
            {
                updateCell(pair.first().x, pair.first().y, pair.second());
            }
        }

        private void updateVertex(State u)
        {
            var s = new System.Collections.Generic.LinkedList<State>();
            if (u.neq(s_goal))
            {
                s = getSucc(u);
                var tmp = double.MaxValue;
                double tmp2;
                foreach (var i in s)
                {
                    tmp2 = getG(i) + cost(u, i);
                    if (tmp2 < tmp)
                    {
                        tmp = tmp2;
                    }
                }

                if (!close(getRHS(u), tmp))
                {
                    setRHS(u, tmp);
                }
            }

            if (!close(getG(u), getRHS(u)))
            {
                insert(u);
            }
        }

        private bool isValid(State u)
        {
            if (!openHash.ContainsKey(u))
            {
                return false;
            }

            if (!close(keyHashCode(u), openHash[u]))
            {
                return false;
            }

            return true;
        }

        private void setG(State u, double g)
        {
            makeNewCell(u);
            cellHash[u].g = g;
        }

        private void setRHS(State u, double rhs)
        {
            makeNewCell(u);
            cellHash[u].rhs = rhs;
        }

        private void makeNewCell(State u)
        {
            if (cellHash.ContainsKey(u))
            {
                return;
            }

            var tmp = new CellInfo();
            tmp.rhs = heuristic(u, s_goal);
            tmp.g = heuristic(u, s_goal);
            tmp.cost = C1;
            cellHash.Add(u, tmp);
        }

        public void updateCell(int x, int y, double val)
        {
            var u = new State();
            u.x = x;
            u.y = y;
            if (u.eq(s_start) || u.eq(s_goal))
            {
                return;
            }

            makeNewCell(u);
            cellHash[u].cost = val;
            updateVertex(u);
        }

        private void insert(State u)
        {
            // iterator cur
            float csum;
            u = calculateKey(u);
            // cur = openHash.find(u);
            csum = keyHashCode(u);
            //  return if cell is already in list. TODO: this should be
            //  uncommented except it introduces a bug, I suspect that there is a
            //  bug somewhere else and having duplicates in the openList queue
            //  hides the problem...
            //
            //if ((cur != openHash.end()) && (close(csum, cur->second))) return;
            if (openHash.ContainsKey(u))
                return;
            openHash.Add(u, csum);
            openList.Add(u);
        }

        private float keyHashCode(State u)
        {
            return (float)(u.k.first() + 1193 * u.k.second());
        }

        private bool occupied(State u)
        {
            // if the cellHash does not contain the State u
            if (!cellHash.ContainsKey(u))
            {
                return false;
            }

            return cellHash[u].cost < 0;
        }

        private double trueDist(State a, State b)
        {
            float x = a.x - b.x;
            float y = a.y - b.y;
            return Math.Sqrt(x * x
                             + y * y);
        }

        private double cost(State a, State b)
        {
            var xd = Math.Abs(a.x - b.x);
            var yd = Math.Abs(a.y - b.y);
            double scale = 1;
            if (xd + yd > 1)
            {
                scale = M_SQRT2;
            }

            if (cellHash.ContainsKey(a) == false)
            {
                return scale * C1;
            }

            return scale * cellHash[a].cost;
        }

        private bool close(double x, double y)
        {
            if ((x == double.MaxValue)
                && (y == double.MaxValue))
            {
                return true;
            }

            return Math.Abs(x - y) < 1E-05;
        }

        public List<State> getPath()
        {
            return path;
        }
    }

    public class CellInfo
    {
        public double cost;
        public double g;

        public double rhs;
    }

    public class ipoint2
    {
        public int x;

        public int y;

        // default constructor
        public ipoint2()
        {
        }

        // overloaded constructor
        public ipoint2(int x, int y)
        {
            this.x = this.x;
            this.y = this.y;
        }
    }
}