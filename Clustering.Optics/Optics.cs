using Priority_Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clustering.Optics
{
    public class OPTICS
    {
        private struct PointRelation
        {
            public readonly UInt32 To;
            public readonly double Distance;

            public PointRelation(uint to, double distance)
            {
                this.To = to;
                this.Distance = distance;
            }
        }

        readonly Point[] _points;
        readonly double _eps;
        readonly int _minPts;
        readonly List<UInt32> _outputIndexes;
        readonly HeapPriorityQueue<Point> _seeds;

        private void AddOutputIndex(UInt32 index)
        {
            _outputIndexes.Add(index);
            if (_outputIndexes.Count % 250 == 0)
            {
                // TODO : add progress reporting interface
                Console.WriteLine("Progress {0}/{1}", _outputIndexes.Count, _outputIndexes.Capacity);
            }
        }

        public OPTICS(double eps, int minPts, PointsList points)
        {
            _points = points._points.ToArray();
            _eps = eps;
            _minPts = minPts;

            _outputIndexes = new List<UInt32>(_points.Length);
            _seeds = new Priority_Queue.HeapPriorityQueue<Point>(_points.Length);

        }

        public double EuclideanDistance(UInt32 p1Index, UInt32 p2Index)
        {
            double dist = 0;
            var vec1 = _points[p1Index].Vector;
            var vec2 = _points[p2Index].Vector;

            for (int i = 0; i < vec1.Length; i++)
            {
                var diff = (vec1[i] - vec2[i]);
                dist += diff * diff;
            }

            return Math.Sqrt(dist);
        }

        // TODO add way to select which distance to use
        public double ManhatanDistance(UInt32 p1Index, UInt32 p2Index)
        {
            double dist = 0;
            var vec1 = _points[p1Index].Vector;
            var vec2 = _points[p2Index].Vector;

            for (int i = 0; i < vec1.Length; i++)
            {
                var diff = Math.Abs(vec1[i] - vec2[i]);
                dist += diff;
            }

            return dist;
        }

        private void GetNeighborhood(UInt32 p1Index, List<PointRelation> neighborhoodOut)
        {
            neighborhoodOut.Clear();

            for (UInt32 p2Index = 0; p2Index < _points.Length; p2Index++)
            {
                var distance = EuclideanDistance(p1Index, p2Index);

                if (distance <= _eps)
                {
                    neighborhoodOut.Add(new PointRelation(p2Index, distance));
                }
            }
        }

        private double CoreDistance(List<PointRelation> neighbors)
        {
            if (neighbors.Count < _minPts)
                return double.NaN;

            neighbors.Sort(pointComparison);
            return neighbors[_minPts-1].Distance;
        }

        private static PointRelationComparison pointComparison = new PointRelationComparison();

        private class PointRelationComparison : IComparer<PointRelation>
        {
            public int Compare(PointRelation x, PointRelation y)
            {
                if (x.Distance == y.Distance)
                {
                    return 0;
                }
                return x.Distance > y.Distance ? 1 : -1;
            }
        }

        public void BuildReachability()
        {
            for (UInt32 pIndex = 0; pIndex < _points.Length; pIndex++)
            {
                if (_points[pIndex].WasProcessed)
                    continue;

                List<PointRelation> neighborOfPoint = new List<PointRelation>();
                GetNeighborhood(pIndex, neighborOfPoint);

                _points[pIndex].WasProcessed = true;

                AddOutputIndex(pIndex);

                double coreDistance = CoreDistance(neighborOfPoint);

                if (!double.IsNaN(coreDistance))
                {
                    _seeds.Clear();
                    Update(pIndex, neighborOfPoint, coreDistance);

                    List<PointRelation> neighborInner = new List<PointRelation>();
                    while (_seeds.Count > 0)
                    {
                        UInt32 pInnerIndex = _seeds.Dequeue().Index;

                        GetNeighborhood(pInnerIndex, neighborInner);

                        _points[pInnerIndex].WasProcessed = true;

                        AddOutputIndex(pInnerIndex);

                        double coreDistanceInner = CoreDistance(neighborInner);

                        if (!double.IsNaN(coreDistanceInner))
                        {
                            Update(pInnerIndex, neighborInner, coreDistanceInner);
                        }
                    }
                }
            }
        }

        private void Update(UInt32 pIndex, List<PointRelation> neighbors, double coreDistance)
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                UInt32 p2Index = neighbors[i].To;

                if (_points[p2Index].WasProcessed)
                    continue;

                double newReachabilityDistance = Math.Max(coreDistance, neighbors[i].Distance);

                if (double.IsNaN(_points[p2Index].ReachabilityDistance))
                {
                    _points[p2Index].ReachabilityDistance = newReachabilityDistance;
                    _seeds.Enqueue(_points[p2Index], newReachabilityDistance);
                }
                else if (newReachabilityDistance < _points[p2Index].ReachabilityDistance)
                {
                    _points[p2Index].ReachabilityDistance = newReachabilityDistance;
                    _seeds.UpdatePriority(_points[p2Index], newReachabilityDistance);
                }
            }
        }

        public IEnumerable<PointReachability> ReachabilityPoints()
        {
            foreach (var item in _outputIndexes)
            {
                yield return new PointReachability(_points[item].Id, _points[item].ReachabilityDistance);
            }
        }
    }
}
