using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering.Optics
{
    public class PointsList
    {
        internal List<Point> _points;

        public PointsList()
        {
            _points = new List<Point>();
        }

        public void AddPoint(UInt32 id, double[] vector)
        {
            var newPoint = new Point((UInt32)_points.Count, id, vector);
            _points.Add(newPoint);
        }
    }
}
