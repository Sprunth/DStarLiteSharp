using DStarLiteSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStarLiteSharp
{
    public class DStarLite
    {
        // Private Member variables
        private List<State> path = new List<State>();

        private double C1;

        private double k_m;

        private State s_start = new State();

        private State s_goal = new State();

        private State s_last = new State();

        private int maxSteps;

        private C5.IPriorityQueue<State> openList = new C5.IntervalHeap<State>();

        private readonly bool allowDiagonalPathing;

        // Change back to private****
        public Dictionary<State, CellInfo> cellHash = new Dictionary<State, CellInfo>();

        private Dictionary<State, float> openHash = new Dictionary<State, float>();

        // Constants
        private double M_SQRT2 = Math.Sqrt(2);

        // Default constructor
        public DStarLite(int maxSteps = 80000, bool allowDiagonalPathing = true)
        {
            this.maxSteps = maxSteps;
            this.allowDiagonalPathing = allowDiagonalPathing;
            this.C1 = 1;
        }

        // Calculate Keys
        public void CalculateKeys()
        {
        }

        public void init(int sX, int sY, int gX, int gY)
        {
            this.cellHash.Clear();
            this.path.Clear();
            this.openHash.Clear();
            while (!this.openList.IsEmpty)
            {
                this.openList.DeleteMin();
            }

            this.k_m = 0;
            this.s_start.x = sX;
            this.s_start.y = sY;
            this.s_goal.x = gX;
            this.s_goal.y = gY;
            CellInfo tmp = new CellInfo();
            tmp.g = 0;
            tmp.rhs = 0;
            tmp.cost = this.C1;
            this.cellHash.Add(this.s_goal, tmp);
            tmp = new CellInfo();
            tmp.rhs = this.heuristic(this.s_start, this.s_goal);
            tmp.g = this.heuristic(this.s_start, this.s_goal);
            tmp.cost = this.C1;
            this.cellHash.Add(this.s_start, tmp);
            this.s_start = this.calculateKey(this.s_start);
            this.s_last = this.s_start;
        }

        private State calculateKey(State u)
        {
            double val = Math.Min(this.getRHS(u), this.getG(u));
            u.k.setFirst((val
                          + (this.heuristic(u, this.s_start) + this.k_m)));
            u.k.setSecond(val);
            return u;
        }

        private double getRHS(State u)
        {
            if ((u == this.s_goal))
            {
                return 0;
            }

            // if the cellHash doesn't contain the State u
            if (!this.cellHash.ContainsKey(u))
            {
                return this.heuristic(u, this.s_goal);
            }

            return this.cellHash[u].rhs;
        }

        private double getG(State u)
        {
            // if the cellHash doesn't contain the State u
            if (!this.cellHash.ContainsKey(u))
            {
                return this.heuristic(u, this.s_goal);
            }

            return this.cellHash[u].g;
        }

        private double heuristic(State a, State b)
        {
            return (this.eightCondist(a, b) * this.C1);
        }

        private double eightCondist(State a, State b)
        {
            double temp;
            double min = Math.Abs((a.x - b.x));
            double max = Math.Abs((a.y - b.y));
            if ((min > max))
            {
                temp = min;
                min = max;
                max = temp;
            }

            return (((this.M_SQRT2 - 1)
                     * min)
                    + max);
        }

        public bool replan()
        {
            this.path.Clear();
            int res = this.computeShortestPath();
            if ((res < 0))
            {
                Console.WriteLine("No Path to Goal");
                return false;
            }

            LinkedList<State> n = new LinkedList<State>();
            State cur = this.s_start;
            if ((this.getG(this.s_start) == Double.MaxValue))
            {
                Console.WriteLine("No Path to Goal");
                return false;
            }

            while (cur.neq(this.s_goal))
            {
                this.path.Add(cur);
                n = new LinkedList<State>();
                n = this.getSucc(cur);
                if (n.Count == 0)
                {
                    Console.WriteLine("No Path to Goal");
                    return false;
                }

                double cmin = Double.MaxValue;
                double tmin = 0;
                State smin = new State();
                foreach (State i in n)
                {
                    double val = this.cost(cur, i);
                    double val2 = (this.trueDist(i, this.s_goal) + this.trueDist(this.s_start, i));
                    val = (val + this.getG(i));
                    if (this.close(val, cmin))
                    {
                        if ((tmin > val2))
                        {
                            tmin = val2;
                            cmin = val;
                            smin = i;
                        }
                    }
                    else if ((val < cmin))
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

            this.path.Add(this.s_goal);
            return true;
        }

        private int computeShortestPath()
        {
            LinkedList<State> s = new LinkedList<State>();
            if (this.openList.IsEmpty)
            {
                return 1;
            }

            int k = 0;
            // todo: check conversion of this while condition
            while (((!this.openList.IsEmpty
                     && this.openList.FindMin().lt(s_start = calculateKey(s_start))
                     || (this.getRHS(this.s_start) != this.getG(this.s_start)))))
            {
                if (k++ > this.maxSteps)
                {
                    Console.WriteLine("At maxsteps");
                    return -1;
                }
                State u;
                bool test = (this.getRHS(this.s_start) != this.getG(this.s_start));
                // lazy remove
                while (true)
                {
                    if (this.openList.IsEmpty)
                    {
                        return 1;
                    }

                    u = this.openList.DeleteMin();
                    if (!this.isValid(u))
                    {
                        // TODO: Warning!!! continue If
                    }

                    if ((!u.lt(this.s_start)
                         && !test))
                    {
                        return 2;
                    }

                    break;
                }

                this.openHash.Remove(u);
                State k_old = new State(u);
                if (k_old.lt(this.calculateKey(u)))
                {
                    // u is out of date
                    this.insert(u);
                }
                else if ((this.getG(u) > this.getRHS(u)))
                {
                    // needs update (got better)
                    this.setG(u, this.getRHS(u));
                    s = this.getPred(u);
                    foreach (State i in s)
                    {
                        this.updateVertex(i);
                    }
                }
                else
                {
                    //  g <= rhs, state has got worse
                    this.setG(u, Double.MaxValue);
                    s = this.getPred(u);
                    foreach (State i in s)
                    {
                        this.updateVertex(i);
                    }

                    this.updateVertex(u);
                }
            }

            Console.WriteLine($"Path found in {k} steps");

            // while
            return 0;
        }

        private LinkedList<State> getSucc(State u)
        {
            LinkedList<State> s = new LinkedList<State>();
            State tempState;
            if (this.occupied(u))
            {
                return s;
            }

            // Generate the successors, starting at the immediate right,
            // Moving in a clockwise manner
            tempState = new State((u.x + 1), u.y, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x + 1), (u.y + 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            tempState = new State(u.x, (u.y + 1), new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x - 1), (u.y + 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            tempState = new State((u.x - 1), u.y, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x - 1), (u.y - 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            tempState = new State(u.x, (u.y - 1), new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                //tempState = new State((u.x + 1), (u.y - 1), new Pair<double, double>(-1, -1));
                //s.AddFirst(tempState);
            }
            return s;
        }

        private LinkedList<State> getPred(State u)
        {
            LinkedList<State> s = new LinkedList<State>();
            State tempState;
            tempState = new State((u.x + 1), u.y, new Pair<double, double>(-1, -1));
            if (!this.occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State((u.x + 1), (u.y + 1), new Pair<double, double>(-1, -1));
                if (!this.occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.x, (u.y + 1), new Pair<double, double>(-1, -1));
            if (!this.occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State((u.x - 1), (u.y + 1), new Pair<double, double>(-1, -1));
                if (!this.occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State((u.x - 1), u.y, new Pair<double, double>(-1, -1));
            if (!this.occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State((u.x - 1), (u.y - 1), new Pair<double, double>(-1, -1));
                if (!this.occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.x, (u.y - 1), new Pair<double, double>(-1, -1));
            if (!this.occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State((u.x + 1), (u.y - 1), new Pair<double, double>(-1, -1));
                if (!this.occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            return s;
        }

        public void updateStart(int x, int y)
        {
            this.s_start.x = x;
            this.s_start.y = y;
            this.k_m = (this.k_m + this.heuristic(this.s_last, this.s_start));
            this.s_start = this.calculateKey(this.s_start);
            this.s_last = this.s_start;
        }

        public void updateGoal(int x, int y)
        {
            List<Pair<ipoint2, Double>> toAdd = new List<Pair<ipoint2, Double>>();
            Pair<ipoint2, Double> tempPoint;

            foreach (var entry in cellHash)
            {
                if (!this.close(entry.Value.cost, this.C1))
                {
                    tempPoint = new Pair<ipoint2, double>(new ipoint2(entry.Key.x, entry.Key.y),
                        entry.Value.cost);
                    toAdd.Add(tempPoint);
                }
            }

            this.cellHash.Clear();
            this.openHash.Clear();
            while (!this.openList.IsEmpty)
            {
                this.openList.DeleteMin();
            }

            this.k_m = 0;
            this.s_goal.x = x;
            this.s_goal.y = y;
            CellInfo tmp = new CellInfo();
            tmp.rhs = 0;
            tmp.g = 0;
            tmp.cost = this.C1;
            this.cellHash.Add(this.s_goal, tmp);
            tmp = new CellInfo();
            tmp.rhs = this.heuristic(this.s_start, this.s_goal);
            tmp.g = this.heuristic(this.s_start, this.s_goal);
            tmp.cost = this.C1;
            this.cellHash.Add(this.s_start, tmp);
            this.s_start = this.calculateKey(this.s_start);
            this.s_last = this.s_start;
            foreach (var pair in toAdd)
            {
                this.updateCell(pair.first().x, pair.first().y, pair.second());
            }
        }

        private void updateVertex(State u)
        {
            LinkedList<State> s = new LinkedList<State>();
            if (u.neq(this.s_goal))
            {
                s = this.getSucc(u);
                double tmp = Double.MaxValue;
                double tmp2;
                foreach (State i in s)
                {
                    tmp2 = (this.getG(i) + this.cost(u, i));
                    if ((tmp2 < tmp))
                    {
                        tmp = tmp2;
                    }
                }

                if (!this.close(this.getRHS(u), tmp))
                {
                    this.setRHS(u, tmp);
                }
            }

            if (!this.close(this.getG(u), this.getRHS(u)))
            {
                this.insert(u);
            }
        }

        private bool isValid(State u)
        {
            if (!this.openHash.ContainsKey(u))
            {
                return false;
            }

            if (!this.close(this.keyHashCode(u), this.openHash[u]))
            {
                return false;
            }

            return true;
        }

        private void setG(State u, double g)
        {
            this.makeNewCell(u);
            this.cellHash[u].g = g;
        }

        private void setRHS(State u, double rhs)
        {
            this.makeNewCell(u);
            this.cellHash[u].rhs = rhs;
        }

        private void makeNewCell(State u)
        {
            if (this.cellHash.ContainsKey(u))
            {
                return;
            }

            CellInfo tmp = new CellInfo();
            tmp.rhs = this.heuristic(u, this.s_goal);
            tmp.g = this.heuristic(u, this.s_goal);
            tmp.cost = this.C1;
            this.cellHash.Add(u, tmp);
        }

        public void updateCell(int x, int y, double val)
        {
            State u = new State();
            u.x = x;
            u.y = y;
            if ((u.eq(this.s_start) || u.eq(this.s_goal)))
            {
                return;
            }

            this.makeNewCell(u);
            this.cellHash[u].cost = val;
            this.updateVertex(u);
        }

        private void insert(State u)
        {
            // iterator cur
            float csum;
            u = this.calculateKey(u);
            // cur = openHash.find(u);
            csum = this.keyHashCode(u);
            //  return if cell is already in list. TODO: this should be
            //  uncommented except it introduces a bug, I suspect that there is a
            //  bug somewhere else and having duplicates in the openList queue
            //  hides the problem...
            //
            //if ((cur != openHash.end()) && (close(csum, cur->second))) return;
            if (openHash.ContainsKey(u))
                return;
            this.openHash.Add(u, csum);
            this.openList.Add(u);
        }

        private float keyHashCode(State u)
        {
            return ((float)((u.k.first() + (1193 * u.k.second()))));
        }

        private bool occupied(State u)
        {
            // if the cellHash does not contain the State u
            if (!this.cellHash.ContainsKey(u))
            {
                return false;
            }

            return (this.cellHash[u].cost < 0);
        }

        private double trueDist(State a, State b)
        {
            float x = (a.x - b.x);
            float y = (a.y - b.y);
            return Math.Sqrt(((x * x)
                              + (y * y)));
        }

        private double cost(State a, State b)
        {
            int xd = Math.Abs((a.x - b.x));
            int yd = Math.Abs((a.y - b.y));
            double scale = 1;
            if ((xd + yd) > 1)
            {
                scale = this.M_SQRT2;
            }

            if ((this.cellHash.ContainsKey(a) == false))
            {
                return (scale * this.C1);
            }

            return (scale * this.cellHash[a].cost);
        }

        private bool close(double x, double y)
        {
            if (((x == Double.MaxValue)
                 && (y == Double.MaxValue)))
            {
                return true;
            }

            return (Math.Abs((x - y)) < 1E-05);
        }

        public List<State> getPath()
        {
            return this.path;
        }
    }

    public class CellInfo
    {
        public double g = 0;

        public double rhs = 0;

        public double cost = 0;
    }

    public class ipoint2
    {
        public int x = 0;

        public int y = 0;

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