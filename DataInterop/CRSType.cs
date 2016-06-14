namespace DataInterop
{
    public enum CRSType
    {
        /// <summary>
        ///     Defines a CRS type where the CRS cannot be assumed
        /// </summary>
        Unspecified,

        /// <summary>
        ///     Defines the <see cref="http://geojson.org/geojson-spec.html#named-crs">Named</see> CRS type.
        /// </summary>
        Name,

        /// <summary>
        ///     Defines the <see cref="http://geojson.org/geojson-spec.html#linked-crs">Linked</see> CRS type.
        /// </summary>
        Link
    }
}