using System;

namespace DStarLiteSharp
{
    public class State : ICloneable, IComparable
    {
        public Pair<double, double> k = new Pair<double, double>(0.0, 0.0);
        public int x;
        public int y;

        public State()
        {
        }

        public State(int x, int y, Pair<double, double> k)
        {
            this.x = x;
            this.y = y;
            this.k = k;
        }

        public State(State other)
        {
            x = other.x;
            y = other.y;
            k = other.k;
        }

        public object Clone()
        {
            return new State(x, y, new Pair<double, double>(k.first(), k.second()));
        }

        public int CompareTo(object obj)
        {
            var other = (State)obj;
            if (k.first() - 0.00001 > other.k.first()) return 1;
            if (k.first() < other.k.first() - 0.00001) return -1;
            if (k.second() > other.k.second()) return 1;
            if (k.second() < other.k.second()) return -1;
            return 0;
        }

        //Equals
        public bool eq(State s2)
        {
            return (x == s2.x) && (y == s2.y);
        }

        //Not Equals
        public bool neq(State s2)
        {
            return (x != s2.x) || (y != s2.y);
        }

        //Greater than
        public bool gt(State s2)
        {
            if (k.first() - 0.00001 > s2.k.first()) return true;
            if (k.first() < s2.k.first() - 0.00001) return false;
            return k.second() > s2.k.second();
        }

        //Less than or equal to
        public bool lte(State s2)
        {
            if (k.first() < s2.k.first()) return true;
            if (k.first() > s2.k.first()) return false;
            return k.second() < s2.k.second() + 0.00001;
        }

        //Less than
        public bool lt(State s2)
        {
            if (k.first() + 0.000001 < s2.k.first()) return true;
            if (k.first() - 0.000001 > s2.k.first()) return false;
            return k.second() < s2.k.second();
        }

        public override int GetHashCode()
        {
            return x + 34245 * y;
        }

        public override bool Equals(object obj)
        {
            //check for self-comparison
            if (this == obj) return true;

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
            if (x == that.x && y == that.y) return true;
            return false;
        }
    }
}