using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataInterop
{
    /// <summary>
    ///     Base class for all IGeometryObject implementing types
    /// </summary>
    public abstract class CRSBase
    {
        /// <summary>
        ///     Gets the properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; internal set; }

        /// <summary>
        ///     Gets the type of the GeometryObject object.
        /// </summary>
        public CRSType Type { get; internal set; }
    }
}
