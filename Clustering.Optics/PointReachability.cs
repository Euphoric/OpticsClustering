using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clustering.Optics
{
    public struct PointReachability
    {
        public PointReachability(UInt32 pointId, double reachability)
        {
            PointId = pointId;
            Reachability = reachability;
        }

        public readonly UInt32 PointId;
        public readonly double Reachability;
    }
}
