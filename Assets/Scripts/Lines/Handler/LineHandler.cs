using System;
using System.Collections.Generic;
using System.Linq;
using Lines.Other;
using UnityEngine;

namespace Lines.Handler
{
    public class LineHandler : ILineHandler
    {
        private readonly List<Vector3> _intersections = new();
        private const float FloatingTolerance = 0.0001f;

        /// <returns>Returns the given array of lines but split with new lines if any intersections were found.</returns>
        public IEnumerable<CustomLine> Split(CustomLine[] lines, CustomLine newLine, float minLength)
        {
            // Return the same list if there is less than 2 lines
            var count = lines.Length;
            if (count <= 1)
            {
                return lines;
            }
            
            var linesToReturn = new List<CustomLine>();
            linesToReturn.AddRange(lines);
            
            var linesToRemove = new List<CustomLine>();
            var linesCreated = new List<CustomLine>();
            
            foreach (var otherLine in linesToReturn)
            {
                if (otherLine.Equals(newLine))
                {
                    continue;
                }
                
                // Check if the added line is intersecting with any other line
                var intersection = newLine.Intersects(otherLine);
                if (intersection == default)
                {
                    continue;
                }
                
                // Lines can also be half as long as the initial grid size (diagonal)
                minLength /= 2f;
                
                // Create new lines based on intersection (can be 4 in total)
                // Checking for length before creating, because they might be too small
                
                var first = new CustomLine(newLine.StartPoint, intersection);
                if (first.Length >= minLength)
                {
                    linesCreated.Add(first);
                }

                var second = new CustomLine(intersection, newLine.EndPoint);
                if (second.Length >= minLength)
                {
                    linesCreated.Add(second);
                }
                
                var third = new CustomLine(otherLine.StartPoint, intersection);
                if (third.Length >= minLength)
                {
                    linesCreated.Add(third);
                }
                
                var fourth = new CustomLine(intersection, otherLine.EndPoint);
                if (fourth.Length >= minLength)
                {
                    linesCreated.Add(fourth);
                }
                
                // If no lines were created continue with iteration
                if (linesCreated.Count <= 0)
                {
                    continue;
                }
                    
                // Lines that were split should be removed later
                linesToRemove.Add(otherLine);
                linesToRemove.Add(newLine);
                
                // TODO: Update intersections when re-/moving a line
                if (GetIntersections().Contains(intersection))
                {
                    continue;
                }
                
                // Save intersection
                GetIntersections().Add(intersection);
            }

            // Remove other lines that were split
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove);
            }
            
            // Add new lines to previous list if they don't already exist (fixes duplicates)
            foreach (var createdLine in linesCreated.Where(createdLine => !linesToReturn.Contains(createdLine)))
            {
                linesToReturn.Add(createdLine);
            }

            return linesToReturn;
        }

        /// <returns>Returns the given array of lines, but lines that can be seen as one, are now merged together.</returns>
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
                if (otherLine.Skip || newLine.Skip)
                {
                    continue;
                }

                var newStartPoint = newLine.StartPoint;
                var newEndPoint = newLine.EndPoint;

                // Check if the compared lines are able to merge (intersections)
                var intersected = GetIntersections().Any(intersection => newStartPoint == intersection || newEndPoint == intersection);
                if (intersected)
                {
                    continue;
                }
                
                var otherStartPoint = otherLine.StartPoint;
                var otherEndPoint = otherLine.EndPoint;

                // create placeholder
                var mergedLine = new CustomLine(Vector3.zero, Vector3.zero);
                
                // TODO: allow diagonal merges
                
                // Special case 1 (Other line between the new line)
                if (newLine.ContainsPoint(otherStartPoint) && newLine.ContainsPoint(otherEndPoint) && newLine.Length > otherLine.Length && 
                    
                    // Anchor points
                    (Math.Abs(newStartPoint.x - otherStartPoint.x) < FloatingTolerance || 
                     Math.Abs(newStartPoint.z - otherStartPoint.z) < FloatingTolerance) && 
                    (Math.Abs(newEndPoint.x - otherStartPoint.x) < FloatingTolerance || 
                     Math.Abs(newEndPoint.z - otherStartPoint.z) < FloatingTolerance))
                {
                    // can later be changed to match previous direction (just swap newStartPoint with newEndPoint), if needed
                    mergedLine = new CustomLine(newStartPoint, newEndPoint);
                }
                
                // First case (New: Start == Other: End)
                if (newStartPoint == otherEndPoint && 
                    (Math.Abs(newEndPoint.x - otherStartPoint.x) < FloatingTolerance || 
                     Math.Abs(newEndPoint.z - otherStartPoint.z) < FloatingTolerance) &&
                    
                    // Anchor point
                    (Math.Abs(newEndPoint.x - otherEndPoint.x) < FloatingTolerance || 
                     Math.Abs(newEndPoint.z - otherEndPoint.z) < FloatingTolerance))
                {
                    mergedLine = newLine.Direction == otherLine.Direction ? 
                        new CustomLine(otherStartPoint, newEndPoint) : new CustomLine(newStartPoint, newEndPoint);
                }
                
                // Second case (New: Start == Other: Start)
                if (newStartPoint == otherStartPoint && 
                    (Math.Abs(newEndPoint.x - otherEndPoint.x) < FloatingTolerance || 
                     Math.Abs(newEndPoint.z - otherEndPoint.z) < FloatingTolerance))
                {
                    mergedLine = newLine.Direction == otherLine.Direction ? 
                        new CustomLine(otherStartPoint, newEndPoint) : new CustomLine(newEndPoint, otherEndPoint);
                }
                
                // Third case (New: End == Other: End)
                if (newEndPoint == otherEndPoint && 
                    (Math.Abs(newStartPoint.x - otherStartPoint.x) < FloatingTolerance || 
                     Math.Abs(newStartPoint.z - otherStartPoint.z) < FloatingTolerance))
                {
                    mergedLine = newLine.Direction == otherLine.Direction ? 
                        new CustomLine(newStartPoint, newEndPoint) : new CustomLine(otherStartPoint, newStartPoint);
                }
                
                // Fourth case (New: End == Other: Start)
                if (newEndPoint == otherStartPoint && 
                    (Math.Abs(newStartPoint.x - otherEndPoint.x) < FloatingTolerance || 
                     Math.Abs(newStartPoint.z - otherEndPoint.z) < FloatingTolerance))
                {
                    mergedLine = newLine.Direction == otherLine.Direction ? 
                        new CustomLine(newStartPoint, otherEndPoint) : new CustomLine(newStartPoint, otherStartPoint);
                }

                // Continue iteration if no lines were merged
                if (mergedLine.Length == 0f)
                {
                    continue;
                }
                
                linesToRemove.Add(otherLine.MakeSkip());
                linesToRemove.Add(newLine.MakeSkip());
                
                linesCreated.Add(mergedLine);
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
        public IEnumerable<CustomLine> Combine(CustomLine[] lines, CustomLine newLine, float minLength)
        {
            return Split(Merge(lines, newLine).ToArray(), newLine, minLength);
        }

        public List<Vector3> GetIntersections()
        {
            return _intersections;
        }
    }
}