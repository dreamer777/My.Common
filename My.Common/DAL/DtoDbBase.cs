#region usings
using System;

using JetBrains.Annotations;

#endregion



namespace My.Common.DAL
{
    /// <summary>
    ///     Добавляет ICloneable
    /// </summary>
    [Serializable]
    public abstract class DtoDbBase<T> : MarshalByRefObjectNoTimeout
        where T : DtoDbBase<T>
    {
        public virtual T Clone()
        {
            T copy = (T) MemberwiseClone();
            return copy;
        }


        protected abstract bool InteriorEquals([NotNull] T other);


        public bool Equals(DtoDbBase<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (this.GetType() != other.GetType())
                return false;
            return InteriorEquals((T) other);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            return InteriorEquals((T) obj);
        }


        public static bool operator ==(DtoDbBase<T> left, DtoDbBase<T> right)
        {
            return Equals(left, right);
        }


        public static bool operator !=(DtoDbBase<T> left, DtoDbBase<T> right)
        {
            return !Equals(left, right);
        }
    }
}