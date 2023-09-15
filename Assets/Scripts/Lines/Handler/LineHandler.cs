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
            
            var lastIntersections = new List<Vector3>();
            var linesToRemove = new List<CustomLine>();
            var linesCreated = new List<CustomLine>();
            
            var linesToReturn = new List<CustomLine>();
            linesToReturn.AddRange(lines); 
            
            foreach (var otherLine in linesToReturn)
            {
                // Check for intersections between the new and other line
                var intersection = newLine.Intersection(otherLine);
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
                
                // Save intersection
                SaveIntersection(intersection);

                if (lastIntersections.Contains(intersection))
                {
                    continue;
                }
                lastIntersections.Add(intersection);
            }
            
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove);
            }

            var intersections = lastIntersections.Count;
            if (intersections > 1)
            {
                // Sort intersections by position
                lastIntersections = lastIntersections.OrderBy(i => i.x).ThenBy(i => i.z).ToList();
            }
            
            // Check if there are at least 2 intersections
            while (intersections > 1)
            {
                var lastIntersection = lastIntersections[intersections - 1];
                var beforeLastIntersection = lastIntersections[intersections - 2];
                
                intersections--;
                
                // Check if the new line goes through both intersections
                if (!newLine.ContainsPoints(lastIntersection, beforeLastIntersection))
                {
                    continue;
                }
            
                // Remove lines that go through both intersections (unnecessary duplicates)
                linesCreated.RemoveAll(line => line.ContainsPoints(lastIntersection, beforeLastIntersection));
                
                // Create line that connects both intersections
                linesCreated.Add(new CustomLine(beforeLastIntersection, lastIntersection));
            }
            
            // This removes unnecessary lines that were created during merges
            linesCreated.RemoveAll(createdLine => linesToReturn.Where(line => line.RecentlyMerged)
                                                                     .Any(mergedLine => mergedLine.ResetMerge().ContainsLine(createdLine)));
            
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
            
            var mergeResult = new CustomLine(Vector3.zero, Vector3.zero);
            
            var linesToRemove = new List<CustomLine>();
            var linesToReturn = new List<CustomLine>();
            linesToReturn.AddRange(lines);
            
            foreach (var otherLine in linesToReturn)
            {
                if (otherLine.Skip || !newLine.CanMergeWith(otherLine, GetIntersections()))
                {
                    continue;
                }
             
                mergeResult = newLine.Merge(otherLine);
                if (mergeResult == default)
                {
                    continue;
                }

                // This is just some testing to improve and fix CanMergeWith Method
                // if (GetIntersections().Any(lastIntersection => mergeResult.ContainsPoints(lastIntersection) &&  
                //                                                lastIntersection != mergeResult.StartPoint && 
                //                                                lastIntersection != mergeResult.EndPoint))
                // {
                //     continue;
                // }
                
                linesToRemove.AddRange(new[] { otherLine.MakeSkip(), newLine.MakeSkip() });
            }
            
            // Remove lines that were merged
            foreach (var toRemove in linesToRemove)
            {
                linesToReturn.Remove(toRemove);
            }
            
            // Add line that was merged
            if (mergeResult != default)
            {
                linesToReturn.Add(mergeResult);   
            }
         
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