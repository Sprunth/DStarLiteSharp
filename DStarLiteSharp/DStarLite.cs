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
        public readonly Dictionary<State, CellInfo> cellHash = new Dictionary<State, CellInfo>();

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
            s_start.X = sX;
            s_start.Y = sY;
            s_goal.X = gX;
            s_goal.Y = gY;
            var tmp = new CellInfo
            {
                G = 0,
                Rhs = 0,
                Cost = C1
            };
            cellHash.Add(s_goal, tmp);
            tmp = new CellInfo
            {
                Rhs = Heuristic(s_start, s_goal),
                G = Heuristic(s_start, s_goal),
                Cost = C1
            };
            cellHash.Add(s_start, tmp);
            s_start = CalculateKey(s_start);
            s_last = s_start;
        }

        private State CalculateKey(State u)
        {
            var val = Math.Min(GetRhs(u), GetG(u));
            u.k.SetFirst(val
                         + (Heuristic(u, s_start) + k_m));
            u.k.SetSecond(val);
            return u;
        }

        private double GetRhs(State u)
        {
            if (u.Equals(s_goal))
            {
                return 0;
            }

            // if the cellHash doesn't contain the State u
            if (!cellHash.ContainsKey(u))
            {
                return Heuristic(u, s_goal);
            }

            return cellHash[u].Rhs;
        }

        private double GetG(State u)
        {
            // if the cellHash doesn't contain the State u
            if (!cellHash.ContainsKey(u))
            {
                return Heuristic(u, s_goal);
            }

            return cellHash[u].G;
        }

        private double Heuristic(State a, State b)
        {
            return EightCondist(a, b) * C1;
        }

        private double EightCondist(State a, State b)
        {
            double min = Math.Abs(a.X - b.X);
            double max = Math.Abs(a.Y - b.Y);
            if (min > max)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            return (M_SQRT2 - 1)
                   * min
                   + max;
        }

        public bool Replan()
        {
            path.Clear();
            var res = ComputeShortestPath();
            if (res < 0)
            {
                Console.WriteLine("No Path to Goal");
                return false;
            }

            var cur = s_start;
            if (GetG(s_start) == double.MaxValue)
            {
                Console.WriteLine("No Path to Goal");
                return false;
            }

            while (cur.Neq(s_goal))
            {
                path.Add(cur);
                var n = new System.Collections.Generic.LinkedList<State>();
                n = GetSucc(cur);
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
                    var val = Cost(cur, i);
                    var val2 = TrueDist(i, s_goal) + TrueDist(s_start, i);
                    val = val + GetG(i);
                    if (Close(val, cmin))
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

        private int ComputeShortestPath()
        {
            if (openList.IsEmpty)
            {
                return 1;
            }

            var k = 0;
            // todo: check conversion of this while condition
            while (!openList.IsEmpty
                   && openList.FindMin().Lt(s_start = CalculateKey(s_start))
                   || (GetRhs(s_start) != GetG(s_start)))
            {
                if (k++ > maxSteps)
                {
                    Console.WriteLine("At maxsteps");
                    return -1;
                }
                State u;
                var test = GetRhs(s_start) != GetG(s_start);
                // lazy remove
                while (true)
                {
                    if (openList.IsEmpty)
                    {
                        return 1;
                    }

                    u = openList.DeleteMin();
                    if (!IsValid(u))
                    {
                        // TODO: Warning!!! continue If
                    }

                    if (!u.Lt(s_start)
                        && !test)
                    {
                        return 2;
                    }

                    break;
                }

                openHash.Remove(u);
                var k_old = new State(u);
                if (k_old.Lt(CalculateKey(u)))
                {
                    // u is out of date
                    Insert(u);
                }
                else
                {
                    System.Collections.Generic.LinkedList<State> s;
                    if (GetG(u) > GetRhs(u))
                    {
                        // needs update (got better)
                        SetG(u, GetRhs(u));
                        s = GetPred(u);
                        foreach (var i in s)
                        {
                            UpdateVertex(i);
                        }
                    }
                    else
                    {
                        //  g <= rhs, state has got worse
                        SetG(u, double.MaxValue);
                        s = GetPred(u);
                        foreach (var i in s)
                        {
                            UpdateVertex(i);
                        }

                        UpdateVertex(u);
                    }
                }
            }

            Console.WriteLine($"Path found in {k} steps");

            // while
            return 0;
        }

        private System.Collections.Generic.LinkedList<State> GetSucc(State u)
        {
            var s = new System.Collections.Generic.LinkedList<State>();
            State tempState;
            if (Occupied(u))
            {
                return s;
            }

            // Generate the successors, starting at the immediate right,
            // Moving in a clockwise manner
            tempState = new State(u.X + 1, u.Y, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                tempState = new State((u.X + 1), (u.Y + 1), new Pair<double, double>(-1, -1));
                s.AddFirst(tempState);
            }
            tempState = new State(u.X, u.Y + 1, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                tempState = new State((u.X - 1), (u.Y + 1), new Pair<double, double>(-1, -1));
                s.AddFirst(tempState);
            }
            tempState = new State(u.X - 1, u.Y, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                tempState = new State((u.X - 1), (u.Y - 1), new Pair<double, double>(-1, -1));
                s.AddFirst(tempState);
            }
            tempState = new State(u.X, u.Y - 1, new Pair<double, double>(-1, -1));
            s.AddFirst(tempState);
            if (allowDiagonalPathing)
            {
                tempState = new State((u.X + 1), (u.Y - 1), new Pair<double, double>(-1, -1));
                s.AddFirst(tempState);
            }
            return s;
        }

        private System.Collections.Generic.LinkedList<State> GetPred(State u)
        {
            var s = new System.Collections.Generic.LinkedList<State>();
            State tempState;
            tempState = new State(u.X + 1, u.Y, new Pair<double, double>(-1, -1));
            if (!Occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.X + 1, u.Y + 1, new Pair<double, double>(-1, -1));
                if (!Occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.X, u.Y + 1, new Pair<double, double>(-1, -1));
            if (!Occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.X - 1, u.Y + 1, new Pair<double, double>(-1, -1));
                if (!Occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.X - 1, u.Y, new Pair<double, double>(-1, -1));
            if (!Occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.X - 1, u.Y - 1, new Pair<double, double>(-1, -1));
                if (!Occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            tempState = new State(u.X, u.Y - 1, new Pair<double, double>(-1, -1));
            if (!Occupied(tempState))
            {
                s.AddFirst(tempState);
            }

            if (allowDiagonalPathing)
            {
                tempState = new State(u.X + 1, u.Y - 1, new Pair<double, double>(-1, -1));
                if (!Occupied(tempState))
                {
                    s.AddFirst(tempState);
                }
            }

            return s;
        }

        public void UpdateStart(int x, int y)
        {
            s_start.X = x;
            s_start.Y = y;
            k_m = k_m + Heuristic(s_last, s_start);
            s_start = CalculateKey(s_start);
            s_last = s_start;
        }

        public void UpdateGoal(int x, int y)
        {
            var toAdd = new List<Pair<Ipoint2, double>>();

            foreach (var entry in cellHash)
            {
                if (!Close(entry.Value.Cost, C1))
                {
                    var tempPoint = new Pair<Ipoint2, double>(new Ipoint2(entry.Key.X, entry.Key.Y),
                        entry.Value.Cost);
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
            s_goal.X = x;
            s_goal.Y = y;
            var tmp = new CellInfo
            {
                Rhs = 0,
                G = 0,
                Cost = C1
            };
            cellHash.Add(s_goal, tmp);
            tmp = new CellInfo
            {
                Rhs = Heuristic(s_start, s_goal),
                G = Heuristic(s_start, s_goal),
                Cost = C1
            };
            cellHash.Add(s_start, tmp);
            s_start = CalculateKey(s_start);
            s_last = s_start;
            foreach (var pair in toAdd)
            {
                UpdateCell(pair.First().x, pair.First().y, pair.Second());
            }
        }

        private void UpdateVertex(State u)
        {
            if (u.Neq(s_goal))
            {
                var s = GetSucc(u);
                var tmp = double.MaxValue;
                foreach (var i in s)
                {
                    var tmp2 = GetG(i) + Cost(u, i);
                    if (tmp2 < tmp)
                    {
                        tmp = tmp2;
                    }
                }

                if (!Close(GetRhs(u), tmp))
                {
                    SetRhs(u, tmp);
                }
            }

            if (!Close(GetG(u), GetRhs(u)))
            {
                Insert(u);
            }
        }

        private bool IsValid(State u)
        {
            if (!openHash.ContainsKey(u))
            {
                return false;
            }

            if (!Close(KeyHashCode(u), openHash[u]))
            {
                return false;
            }

            return true;
        }

        private void SetG(State u, double g)
        {
            MakeNewCell(u);
            cellHash[u].G = g;
        }

        private void SetRhs(State u, double rhs)
        {
            MakeNewCell(u);
            cellHash[u].Rhs = rhs;
        }

        private void MakeNewCell(State u)
        {
            if (cellHash.ContainsKey(u))
            {
                return;
            }

            var tmp = new CellInfo
            {
                Rhs = Heuristic(u, s_goal),
                G = Heuristic(u, s_goal),
                Cost = C1
            };
            cellHash.Add(u, tmp);
        }

        public void UpdateCell(int x, int y, double val)
        {
            var u = new State
            {
                X = x,
                Y = y
            };
            if (u.Eq(s_start) || u.Eq(s_goal))
            {
                return;
            }

            MakeNewCell(u);
            cellHash[u].Cost = val;
            UpdateVertex(u);
        }

        private void Insert(State u)
        {
            // iterator cur
            u = CalculateKey(u);
            // cur = openHash.find(u);
            var csum = KeyHashCode(u);
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

        private static float KeyHashCode(State u)
        {
            return (float)(u.k.First() + 1193 * u.k.Second());
        }

        private bool Occupied(State u)
        {
            // if the cellHash does not contain the State u
            if (!cellHash.ContainsKey(u))
            {
                return false;
            }

            return cellHash[u].Cost < 0;
        }

        private static double TrueDist(State a, State b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return Math.Sqrt(x * x
                             + y * y);
        }

        private double Cost(State a, State b)
        {
            var xd = Math.Abs(a.X - b.X);
            var yd = Math.Abs(a.Y - b.Y);
            double scale = 1;
            if (xd + yd > 1)
            {
                scale = M_SQRT2;
            }

            if (cellHash.ContainsKey(a) == false)
            {
                return scale * C1;
            }

            return scale * cellHash[a].Cost;
        }

        private static bool Close(double x, double y)
        {
            if ((x == double.MaxValue)
                && (y == double.MaxValue))
            {
                return true;
            }

            return Math.Abs(x - y) < 1E-05;
        }

        public List<State> GetPath()
        {
            return path;
        }
    }

    public class CellInfo
    {
        public double Cost;
        public double G;

        public double Rhs;
    }

    public class Ipoint2
    {
        public int x;

        public int y;

        // default constructor
        public Ipoint2()
        {
        }

        // overloaded constructor
        public Ipoint2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}