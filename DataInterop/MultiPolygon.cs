using System;
using System.Collections.Generic;
using System.Linq;

namespace DataInterop
{
    /// <summary>
    ///     Defines the <see cref="http://geojson.org/geojson-spec.html#multipolygon">MultiPolygon</see> type.
    /// </summary>
    public class MultiPolygon : GeoJSONObject, IGeometryObject
    {
        public MultiPolygon()
            : this(new List<Polygon>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultiPolygon" /> class.
        /// </summary>
        /// <param name="polygons">The polygons contained in this MultiPolygon.</param>
        public MultiPolygon(List<Polygon> polygons)
        {
            if (polygons == null)
            {
                throw new ArgumentNullException("polygons");
            }

            this.Coordinates = polygons;
            this.Type = GeoJSONObjectType.MultiPolygon;
        }

        /// <summary>
        ///     Gets the list of Polygons enclosed in this MultiPolygon.
        /// </summary>
        public List<Polygon> Coordinates { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((MultiPolygon)obj);
        }

        public override int GetHashCode()
        {
            return this.Coordinates.GetHashCode();
        }

        public static bool operator ==(MultiPolygon left, MultiPolygon right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MultiPolygon left, MultiPolygon right)
        {
            return !Equals(left, right);
        }

        protected bool Equals(MultiPolygon other)
        {
            return base.Equals(other) && this.Coordinates.SequenceEqual(other.Coordinates);
        }
    }
}