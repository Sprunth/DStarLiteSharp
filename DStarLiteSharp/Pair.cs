using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DStarLiteSharp
{
    /// <summary>
    ///The class <see cref="Pair"/> models a container for two objects wherein the
    ///object order is of no consequence for equality and hashing.An example of
    ///using Pair would be as the return type for a method that needs to return two
    ///related objects.Another good use is as entries in a Set or keys in a Map
    ///when only the unordered combination of two objects is of interest.
    /// 
    ///The term "object" as being a one of a Pair can be loosely interpreted. A
    ///Pair may have one or two null entries as values.Both values
    ///may also be the same object.
    /// 
    ///Mind that the order of the type parameters T and U is of no importance.A
    ///Pair&lt;T, U&gt; can still return True for method equals
    /// called with a Pair&lt;U, T&gt; argument.
    ///Instances of this class are immutable, but the provided values might not be.
    ///This means the consistency of equality checks and the hash code is only as
    ///strong as that of the value types.
    /// </summary>
    public class Pair<T, U> : ICloneable
    {
        /* Todo checklist
         * implement serializable
         * finish porting xml doc comments
         * */

        private T object1;
        private U object2;

        private bool object1Null;
        private bool object2Null;
        private bool dualNull;

        public Pair(T object1, U object2)
        {
            this.object1 = object1;
            this.object2 = object2;
            object1Null = object1 == null;
            object2Null = object2 == null;
            dualNull = object1Null && object2Null;
        }

        public T first()
        {
            return object1;
        }

        public U second()
        {
            return object2;
        }

        public void setFirst(T object1)
        {
            this.object1 = object1;
            object1Null = object1 == null;
            dualNull = object1Null && object2Null;
        }

        public void setSecond(U object2)
        {
            this.object2 = object2;
            object2Null = object2 == null;
            dualNull = object1Null && object2Null;
        }

        public object Clone()
        {
            return new Pair<T, U>(object1, object2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (this == obj)
                return true;

            if (obj.GetType() != typeof (Pair<object, object>))
                return false;

            var otherPair = (Pair<object, object>) obj;

            if (dualNull)
                return otherPair.dualNull;

            if (otherPair.dualNull)
                return false;

            if (object1Null)
            {
                if (otherPair.object1Null)
                    return object2.Equals(otherPair.object2);
                else if (otherPair.object2Null)
                    return object2.Equals(otherPair.object1);
                else
                    return false;
            }
            else if (object2Null)
            {
                if (otherPair.object2Null)
                    return object1.Equals(otherPair.object1);
                else if (otherPair.object1Null)
                    return object1.Equals(otherPair.object2);
                else
                    return false;
            }
            else
            {
                if (object1.Equals(otherPair.object1))
                    return object2.Equals(otherPair.object2);
                else if (object1.Equals(otherPair.object2))
                    return object2.Equals(otherPair.object1);
                else
                    return false;
            }
        }

        public override int GetHashCode()
        {
            var hashCode = object1Null ? 0 : object1.GetHashCode();
            hashCode += object2Null ? 0 : object2.GetHashCode();
            return hashCode;
        }
    }
}