#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#endregion



namespace My.Common.DAL
{
    internal class DtoDummy : DtoDbBase<DtoDummy>
    {
        protected override bool InteriorEquals(DtoDummy other)
        {
            return true;
        }
    }
}