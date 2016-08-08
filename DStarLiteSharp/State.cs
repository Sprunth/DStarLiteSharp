using System;

namespace DStarLiteSharp
{
    public class State : ICloneable, IComparable
    {
        public readonly Pair<double, double> k = new Pair<double, double>(0.0, 0.0);
        public int X;
        public int Y;

        public State()
        {
        }

        public State(int x, int y, Pair<double, double> k)
        {
            X = x;
            Y = y;
            this.k = k;
        }

        public State(State other)
        {
            X = other.X;
            Y = other.Y;
            k = other.k;
        }

        public object Clone()
        {
            return new State(X, Y, new Pair<double, double>(k.First(), k.Second()));
        }

        public int CompareTo(object obj)
        {
            var other = (State)obj;
            if (k.First() - 0.00001 > other.k.First()) return 1;
            if (k.First() < other.k.First() - 0.00001) return -1;
            if (k.Second() > other.k.Second()) return 1;
            if (k.Second() < other.k.Second()) return -1;
            return 0;
        }

        //Equals
        public static bool operator ==(State s1, State s2)
        {
            return (s1.X == s2.X) && (s1.Y == s2.Y);
        }

        //Not Equals
        public static bool operator !=(State s1, State s2)
        {
            return (s1.X != s2.X) || (s1.Y != s2.Y);
        }

        //Greater than or equal to
        public static bool operator >=(State s1, State s2)
        {
            if (s1.k.First() < s2.k.First()) return false;
            if (s1.k.First() > s2.k.First()) return true;
            return s1.k.Second() > s2.k.Second() + 0.00001;
        }

        //Greater than
        public static bool operator >(State s1, State s2)
        {
            if (s1.k.First() - 0.00001 > s2.k.First()) return true;
            if (s1.k.First() < s2.k.First() - 0.00001) return false;
            return s1.k.Second() > s2.k.Second();
        }

        //Less than or equal to
        public static bool operator <=(State s1, State s2)
        {
            if (s1.k.First() < s2.k.First()) return true;
            if (s1.k.First() > s2.k.First()) return false;
            return s1.k.Second() < s2.k.Second() + 0.00001;
        }

        //Less than
        public static bool operator <(State s1, State s2)
        {
            if (s1.k.First() + 0.000001 < s2.k.First()) return true;
            if (s1.k.First() - 0.000001 > s2.k.First()) return false;
            return s1.k.Second() < s2.k.Second();
        }

        public override int GetHashCode()
        {
            return X + 34245 * Y;
        }

        public override bool Equals(object obj)
        {
            //check for self-comparison
            if (this == (State)obj) return true;

            //use instanceof instead of getClass here for two reasons
            //1. if need be, it can match any supertype, and not just one class;
            //2. it renders an explict check for "that == null" redundant, since
            //it does the check for null already - "null instanceof [type]" always
            //returns false. (See Effective Java by Joshua Bloch.)
            if (obj.GetType() != typeof(State)) return false;
            //Alternative to the above line :
            //if ( aThat == null || aThat.getClass() != this.getClass() ) return false;

            //cast to native object is now safe
            var that = (State)obj;

            //now a proper field-by-field evaluation can be made
            return X == that.X && Y == that.Y;
        }
    }
}