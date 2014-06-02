#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

#endregion



namespace My.Common
{
    [DataContract]
    [Serializable]
    public abstract class MarshalByRefObjectNoTimeout : MarshalByRefObject
    {
        /// <summary>
        /// Отключить контроль времени существования LifetimeService
        /// </summary>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}