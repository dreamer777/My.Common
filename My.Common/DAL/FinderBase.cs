#region usings
using System;

using JetBrains.Annotations;


#endregion



namespace My.Common.DAL
{
    public abstract class FinderBase<T> where T : FinderBase<T>
    {
        [CanBeNull]
        public string SearchString { get; set; }


        public virtual T Clone()
        {
            return (T) this.MemberwiseClone();
        }


        protected abstract bool InteriorEquals(T other);


        public bool Equals(FinderBase<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (this.GetType() != other.GetType()) return false;
            return Equals(other.SearchString, SearchString) && InteriorEquals((T) other);
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (this.GetType() != obj.GetType()) return false;
            return Equals((FinderBase<T>) obj);
        }


        public static bool operator ==(FinderBase<T> left, FinderBase<T> right)
        {
            return Equals(left, right);
        }


        public static bool operator !=(FinderBase<T> left, FinderBase<T> right)
        {
            return !Equals(left, right);
        }
    }
}