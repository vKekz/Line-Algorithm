using System.Collections.Generic;
using System.Linq;
using Lines.Other;
using UnityEngine;

namespace Lines.Handler
{
    public class LineHandler : ILineHandler
    {
        private readonly List<Vector3> _intersections = new();

        /// <returns>Returns the given array of lines but split with new lines if any intersections were found.</returns>
        public IEnumerable<CustomLine> Split(CustomLine[] lines, CustomLine newLine, float gridSize)
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

            foreach (var otherLine in linesToReturn)
            {
                if (otherLine.Equals(newLine))
                {
                    continue;
                }
                
                var intersection = newLine.Intersects(otherLine);
                if (intersection == default)
                {
                    continue;
                }
                
                // Create new lines based on intersection (should always be 4 in total)
                // Checking for length before creating, because they might be too small or on the same spot
                
                var intersects = false;
                var first = new CustomLine(newLine.StartPoint, intersection);
                if (first.GetLength() >= gridSize)
                {
                    Debug.Log("1: " + first);
                    
                    linesCreated.Add(first);
                    intersects = true;
                }

                var second = new CustomLine(intersection, newLine.EndPoint);
                if (second.GetLength() >= gridSize)
                {
                    Debug.Log("2: " + second);

                    linesCreated.Add(second);
                    intersects = true;
                }
                
                var third = new CustomLine(otherLine.StartPoint, intersection);
                if (third.GetLength() >= gridSize)
                {
                    Debug.Log("3: " + third);

                    linesCreated.Add(third);
                    intersects = true;
                }
                
                var fourth = new CustomLine(intersection, otherLine.EndPoint);
                if (fourth.GetLength() >= gridSize)
                {
                    Debug.Log("4: " + fourth);

                    linesCreated.Add(fourth);
                    intersects = true;
                }

                if (intersects)
                {
                    Debug.Log("Intersection at " + intersection);
                    
                    // Lines that were split should be removed later
                    linesToRemove.Add(otherLine);
                    linesToRemove.Add(newLine);
                
                    // Just for intersection debugging
                    GetIntersections().Add(intersection);
                }
            }

            // Remove other lines that were split
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove);
            }
            
            // Add new lines to previous list
            linesToReturn.AddRange(linesCreated);
            
            return linesToReturn;
        }

        /// <returns>Returns the given array of lines, but lines that have the same direction and were split are now merged together.</returns>
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
            
            foreach (var otherLine in linesToReturn)
            {
                if (otherLine.Equals(newLine) || otherLine.Remove || newLine.Remove)
                {
                    continue;
                }

                var otherStartPoint = otherLine.StartPoint;
                var otherEndPoint = otherLine.EndPoint;

                var newStartPoint = newLine.StartPoint;
                var newEndpoint = newLine.EndPoint;

                var remove = false;
                
                // First case
                if (newStartPoint == otherEndPoint && (newEndpoint.x == otherStartPoint.x || newEndpoint.z == otherStartPoint.z))
                {
                    var first = new CustomLine(otherStartPoint, newEndpoint);
                    linesCreated.Add(first);
                    
                    Debug.Log("Merge at 1: " + first);
                
                    remove = true;
                }
                
                // Second case
                if (newEndpoint == otherEndPoint && (newStartPoint.x == otherStartPoint.x || newStartPoint.z == otherStartPoint.z))
                {
                    var second = new CustomLine(otherStartPoint, newStartPoint);
                    linesCreated.Add(second);
                    
                    Debug.Log("Merge at 2: " + second);
                    
                    remove = true;
                }
                
                // Third case
                if (newEndpoint == otherStartPoint && (newStartPoint.x == otherEndPoint.x || newStartPoint.z == otherEndPoint.z))
                {
                    var third = new CustomLine(newStartPoint, otherEndPoint);
                    linesCreated.Add(third);
                    
                    Debug.Log("Merge at 3: " + third);
                    
                    remove = true;
                }
                
                // Fourth case
                if (newStartPoint == otherStartPoint && (newEndpoint.x == otherEndPoint.x || newEndpoint.z == otherEndPoint.z))
                {
                    var fourth = new CustomLine(newEndpoint, otherEndPoint);
                    linesCreated.Add(fourth);
                    
                    Debug.Log("Merge at 4: " + fourth);
                    
                    remove = true;
                }

                if (remove)
                {
                    Debug.Log(newLine);
                    Debug.Log(otherLine);
                    
                    linesToRemove.Add(otherLine.ToRemove());
                    linesToRemove.Add(newLine.ToRemove());
                }
            }
            
            // Remove other lines that were split
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove.ResetRemove());
            }
            
            // Add new lines to previous list
            linesToReturn.AddRange(linesCreated);
            
            return linesToReturn;
        }

        /// <returns>Returns the given array but combined with the methods "Split" and "Merge".</returns>
        public IEnumerable<CustomLine> Combine(CustomLine[] lines, CustomLine newLine, float gridSize)
        {
            return Merge(Split(lines, newLine, gridSize).ToArray(), newLine);
        }

        public List<Vector3> GetIntersections()
        {
            return _intersections;
        }
    }
}