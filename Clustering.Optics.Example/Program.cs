using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clustering.Optics.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Clustering.Optics.PointsList points = new Clustering.Optics.PointsList();

            // create random clusters with random points
            const int dimensions = 10;

            Random rand = new Random(5487);

            UInt32 totalPointsIndex = 0;
            for (int clusterIndex = 0; clusterIndex < 5; clusterIndex++)
            {
                var clusterCenter = Enumerable.Range(0, dimensions).Select(x => rand.NextDouble()).ToArray();
                int clusterCount = rand.Next(100, 1000);

                for (int pointIndex = 0; pointIndex < clusterCount; pointIndex++)
                {
                    var pointVector = clusterCenter.Select(v => v + (rand.NextDouble() * 0.05) - 0.025).ToArray();
                    points.AddPoint(totalPointsIndex, pointVector);
                    totalPointsIndex++;
                }
            }

            // add random noise
            const int noisePointCount = 1000;
            for (int pointIndex = 0; pointIndex < noisePointCount; pointIndex++)
            {
                var pointVector = Enumerable.Range(0, dimensions).Select(x => rand.NextDouble()).ToArray();
                points.AddPoint(totalPointsIndex, pointVector);
                totalPointsIndex++;
            }

            double maxRadius = 5;
            int minPoints = 10;
            var optics = new Clustering.Optics.OPTICS(maxRadius, minPoints, points);

            optics.BuildReachability();

            var reachablity = optics.ReachabilityPoints();

            foreach (var item in reachablity)
	        {
                Console.WriteLine(item.PointId + ";" + item.Reachability);
	        }
        }
    }
}
