namespace AnswerFactory
{
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// The AOI drawn.
    /// </summary>
    /// <param name="poly">
    /// The poly.
    /// </param>
    /// <param name="elm">
    /// The elm.
    /// </param>
    public delegate void AoiDrawn(IPolygon poly, IElement elm);

    public class AnswerFactoryRelay
    {
        public event AoiDrawn AoiHasBeenDrawn;

        private static AnswerFactoryRelay instance;

        private IPolygon localPolygon;

        private IElement localElement;

        private AnswerFactoryRelay()
        {
        }

        public static AnswerFactoryRelay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AnswerFactoryRelay();
                }
                return instance;
            }
        }

        public IPolygon Polygon
        {
            get
            {
                return this.localPolygon;
            }
            set
            {
                this.localPolygon = value;
            }
        }

        public IElement Element
        {
            get
            {
                return this.localElement;
            }
            set
            {
                this.localElement = value;
            }
        }

        public void SetPolygonAndElement(IPolygon polygon, IElement elm)
        {
            this.Polygon = polygon;
            this.Element = elm;

            if (this.AoiHasBeenDrawn != null)
            {
                this.AoiHasBeenDrawn(this.Polygon, this.Element);
            }

        }
    }
}