#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;


#endregion



namespace My.Common.DAL
{
    [Serializable]
    public abstract class UpdateBase
    {
        [NotNull] protected readonly Dictionary<string, object> Changed = new Dictionary<string, object>();


        [NotNull]
        public IEnumerable<KeyValuePair<string, object>> ChangedProps { get { return Changed; } }

        public string ChangedAsString { get { return string.Join(", ", ChangedProps.Select(kvp => string.Format("{0}=[{1}]", kvp.Key, kvp.Value))); } }

        public int CountChangedProps { get { return Changed.Count; } }
        public bool IsEmpty { get { return CountChangedProps == 0; } }
    }
}