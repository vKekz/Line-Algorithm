using System.Collections.Generic;
using System.Linq;
using Lines.Other;
using UnityEngine;

namespace Lines.Handler
{
    public class LineHandler : ILineHandler
    {
        private readonly List<Vector3> _intersections = new();

        public IEnumerable<CustomLine> Split(CustomLine[] lines, CustomLine newLine)
        {
            // Return the same list if there is less than 2 lines
            var count = lines.Length;
            if (count <= 1)
            {
                return lines;
            }

            var currentIntersections = new List<Vector3>();
            var linesToRemove = new List<CustomLine>();
            var linesCreated = new List<CustomLine>();
            
            var linesToReturn = new List<CustomLine>();
            linesToReturn.AddRange(lines);
            
            foreach (var otherLine in linesToReturn)
            {
                // Check for intersections between the new and other line
                var intersection = newLine.Intersects(otherLine);
                if (intersection == default)
                {
                    continue;
                }
                
                // Get split result
                linesCreated.AddRange(newLine.Split(otherLine, intersection));
                
                // If no lines were created continue with iteration
                if (linesCreated.Count <= 0)
                {
                    continue;
                }
                
                // Lines that were split should be removed later
                linesToRemove.AddRange(new[] { otherLine, newLine });
                
                SaveIntersection(intersection);

                if (currentIntersections.Contains(intersection))
                {
                    continue;
                }
                currentIntersections.Add(intersection);
            }
            
            // Remove other lines that were split
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove);
            }

            var intersections = currentIntersections.Count;
            if (intersections > 1)
            {
                // Sort intersections by position
                currentIntersections = currentIntersections.OrderBy(i => i.x).ThenBy(i => i.z).ToList();
            }
            
            // Check if there are at least 2 intersections
            while (intersections > 1)
            {
                var lastIntersection = currentIntersections[intersections - 1];
                var beforeLastIntersection = currentIntersections[intersections - 2];
                
                intersections--;
                
                // Check if the new line goes through both intersections
                if (!newLine.ContainsPoints(lastIntersection, beforeLastIntersection))
                {
                    continue;
                }
            
                // Remove lines that go through both intersections (unnecessary duplicates)
                linesCreated.RemoveAll(line => line.ContainsPoints(lastIntersection, beforeLastIntersection));
                
                // Create line that connects both intersections
                linesToReturn.Add(new CustomLine(lastIntersection, beforeLastIntersection));
            }

            // Add new lines to previous list if they don't already exist (fixes duplicates)
            linesToReturn.AddRange(linesCreated.Where(createdLine => !linesToReturn.Contains(createdLine)));
            
            return linesToReturn;
        }

        public IEnumerable<CustomLine> Merge(CustomLine[] lines, CustomLine newLine)
        {
            // Return the same list if there is less than 2 lines
            var count = lines.Length;
            if (count <= 1)
            {
                return lines;
            }
            
            var linesToRemove = new List<CustomLine>();
            var linesCreated = new List<CustomLine>();
            
            var linesToReturn = new List<CustomLine>();
            linesToReturn.AddRange(lines);
            
            var newStartPoint = newLine.StartPoint;
            var newEndPoint = newLine.EndPoint;
            
            foreach (var otherLine in linesToReturn)
            {
                if (otherLine.Skip || newLine.Skip)
                {
                    continue;
                }

                // Check if there is any intersection between the new line and other line (if so, don't merge)
                var intersected = GetIntersections().Any(intersection => newStartPoint == intersection || newEndPoint == intersection);
                if (intersected)
                {
                    continue;
                }
                
                if (!newLine.ParallelTo(otherLine))
                {
                    continue;
                }
                
                // Merged line
                var mergeResult = newLine.Merge(otherLine);
                if (mergeResult == default)
                {
                    continue;
                }
                
                // Remove lines that were merged
                linesToRemove.AddRange(new[] { otherLine.MakeSkip(), newLine.MakeSkip() });
                
                // Add merge result
                linesCreated.Add(mergeResult);
            }
            
            // Remove lines that were merged
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove);
            }
            
            // Add line that was merged
            linesToReturn.AddRange(linesCreated);
            
            return linesToReturn;
        }

        /// <returns>Returns the given array but combined with the methods "Split" and "Merge".</returns>
        public IEnumerable<CustomLine> Combine(CustomLine[] lines, CustomLine newLine)
        {
            return Split(Merge(lines, newLine).ToArray(), newLine);
        }

        private void SaveIntersection(Vector3 position)
        {
            if (GetIntersections().Contains(position))
            {
                return;
            }
            
            GetIntersections().Add(position);
        }

        public List<Vector3> GetIntersections()
        {
            return _intersections;
        }
    }
}