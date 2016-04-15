namespace GeoJson
{
    using System.Collections.Generic;

    public class MultiPolygon
    {
        private string type = "MultiPolygon";
        private List<double[]> coordinates = new List<double[]>();

        public void AddCoordinate(double x, double y)
        {
            double[] coord = new double[2];
            coord[0] = x;
            coord[1] = y;
            this.coordinates.Add(coord);
        }
    }
    
}