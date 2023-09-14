using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lines.Other
{
    public class CustomLine : IEquatable<CustomLine>
    {
        public Vector3 StartPoint { get; }
        public Vector3 EndPoint { get; }
        public Vector3 MiddlePoint { get; }
        
        private Vector3 DirectionVector { get; }
        public Direction Direction { get; }
        public float Length { get; }
        
        public bool Skip { get; private set; }
        
        public CustomLine(Vector3 startPoint, Vector3 endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Skip = false;

            // pre compute values based on start- and endpoint
            MiddlePoint = GetMiddlePoint();
            DirectionVector = GetDirectionVector();
            Direction = GetDirection();
            Length = GetLength();
        }
        
        public CustomLine MakeSkip()
        {
            Skip = true;
            return this;
        }
        
        /// <returns>Returns a Vector where the intersection happens, if there is one. Otherwise it returns default.</returns>
        public Vector3 Intersects(CustomLine line)
        {
            var firstLineStart = StartPoint;
            var firstLineEnd = EndPoint;

            var secondLineStart = line.StartPoint;
            var secondLineEnd = line.EndPoint;
            
            // Line equations
            var firstDeltaX = firstLineStart.x - firstLineEnd.x;
            var firstDeltaZ = firstLineEnd.z - firstLineStart.z;
            var firstEquation = firstDeltaZ * firstLineStart.x + firstDeltaX * firstLineStart.z;

            var secondDeltaX = secondLineStart.x - secondLineEnd.x;
            var secondDeltaZ = secondLineEnd.z - secondLineStart.z;
            var secondEquation= secondDeltaZ * secondLineStart.x + secondDeltaX * secondLineStart.z;
            
            // Lines will never meet (parallel or too small)
            var determinant = firstDeltaZ * secondDeltaX - secondDeltaZ * firstDeltaX;
            if (determinant == 0)
            {
                return default;
            }

            var x = (secondDeltaX * firstEquation - firstDeltaX * secondEquation) / determinant;
            var z = (firstDeltaZ * secondEquation - secondDeltaZ * firstEquation) / determinant;
            var intersectionPoint = new Vector3(x, 0, z);
            
            // Check if the intersection point lies on both lines
            if (ContainsPoint(intersectionPoint) && line.ContainsPoint(intersectionPoint))
            {
                return intersectionPoint;
            }

            return default;
        }

        /// <returns>Returns true if line is parallel to given one.</returns>
        public bool ParallelTo(CustomLine line)
        {
            var secondLineStart = line.StartPoint;
            var secondLineEnd = line.EndPoint;
            
            var firstDeltaX = StartPoint.x - EndPoint.x;
            var firstDeltaZ = EndPoint.z - StartPoint.z;

            var secondDeltaX = secondLineStart.x - secondLineEnd.x;
            var secondDeltaZ = secondLineEnd.z - secondLineStart.z;
            
            var determinant = firstDeltaZ * secondDeltaX - secondDeltaZ * firstDeltaX;
            return determinant == 0f;
        }

        /// <returns>Returns the shortest distance to the given point.</returns>
        private float DistanceToPoint(Vector3 point)
        {
            return Mathf.Abs((point.x - StartPoint.x) * DirectionVector.z - (point.z - StartPoint.z) * DirectionVector.x) / Length;
        }

        /// <returns>Returns true if the given point is on the line.</returns>
        private bool ContainsPoint(Vector3 point)
        {
            return IsPointInBounds(point) && DistanceToPoint(point) <= Constants.FloatingTolerance;
        }

        /// <returns>Returns true if all given points are on the line.</returns>
        public bool ContainsPoints(params Vector3[] points)
        {
            return points.All(ContainsPoint);
        }

        private bool IsPointInBounds(Vector3 point)
        {
            return ((point.x >= StartPoint.x && point.x <= EndPoint.x) ||
                    (point.x <= StartPoint.x && point.x >= EndPoint.x)) &&
                   ((point.z >= StartPoint.z && point.z <= EndPoint.z) ||
                    (point.z <= StartPoint.z && point.z >= EndPoint.z));
        }
        
        private Vector3 GetMiddlePoint()
        {
            var middleX = (StartPoint.x + EndPoint.x) / 2;
            var middleZ = (StartPoint.z + EndPoint.z) / 2;
            
            return new Vector3(middleX, 0, middleZ);
        }

        private Vector3 GetDirectionVector()
        {
            return EndPoint - StartPoint;
        }
        
        private Direction GetDirection()
        {
            var startPoint = StartPoint;
            var endPoint = EndPoint;
            var point = EndPoint;

            endPoint.x -= startPoint.x;
            endPoint.z -= startPoint.z;
            
            point.x -= startPoint.x;
            point.z -= startPoint.z;

            var crossProduct = startPoint.x * point.x - startPoint.z * point.z;
            return crossProduct > 0 ? Direction.Right : Direction.Left;
        }

        private float GetLength()
        {
            return Vector3.Distance(StartPoint, EndPoint);
        }
        
        /// <returns>Returns a list of lines that were split with the given line at the given intersection.</returns>
        public IEnumerable<CustomLine> Split(CustomLine line, Vector3 intersection)
        {
            // Create new lines based on intersection (can be 4 in total)
            // Checking for length before creating, because they might be too small
            var lines = new List<CustomLine>(4);
            
            var first = new CustomLine(StartPoint, intersection);
            lines.Add(first);

            var second = new CustomLine(intersection, EndPoint);
            lines.Add(second);
                
            var third = new CustomLine(line.StartPoint, intersection);
            lines.Add(third);
                
            var fourth = new CustomLine(intersection, line.EndPoint);
            lines.Add(fourth);

            // Remove lines if they are too short (fixes 0 length splits)
            lines.RemoveAll(createdLine => createdLine.Length <= Constants.FloatingTolerance);

            return lines;
        }
        
        /// <returns>If the current line can merge with the given one, this returns a new merged line, otherwise default.</returns>
        public CustomLine Merge(CustomLine line)
        {
            var mergeResult = new CustomLine(Vector3.zero, Vector3.zero);
            
            var otherStartPoint = line.StartPoint;
            var otherEndPoint = line.EndPoint;
            
            // First special case (Other line is between the new line)
            if (ContainsPoints(otherStartPoint, otherEndPoint) && Length > line.Length)
            {
                // can later be changed to match previous direction (just swap newStartPoint with newEndPoint), if needed
                mergeResult = new CustomLine(StartPoint, EndPoint);
            }

            // Second special case (New: Start or Endpoint between other line)
            if (line.ContainsPoint(StartPoint) || line.ContainsPoint(EndPoint))
            {
                mergeResult = Direction == line.Direction ? 
                    new CustomLine(otherStartPoint, EndPoint) : new CustomLine(otherEndPoint, EndPoint);
            }
            
            // First case (New: Start == Other: End)
            if (StartPoint == otherEndPoint)
            {
                mergeResult = Direction == line.Direction ? 
                    new CustomLine(otherStartPoint, EndPoint) : new CustomLine(StartPoint, EndPoint);
            }
            
            // Second case (New: Start == Other: Start)
            if (StartPoint == otherStartPoint)
            {
                mergeResult = Direction == line.Direction ? 
                    new CustomLine(otherStartPoint, EndPoint) : new CustomLine(EndPoint, otherEndPoint);
            }
            
            // Third case (New: End == Other: End)
            if (EndPoint == otherEndPoint)
            {
                mergeResult = Direction == line.Direction ? 
                    new CustomLine(StartPoint, EndPoint) : new CustomLine(otherStartPoint, StartPoint);
            }
            
            // Fourth case (New: End == Other: Start)
            if (EndPoint == otherStartPoint)
            {
                mergeResult = Direction == line.Direction ? 
                    new CustomLine(StartPoint, otherEndPoint) : new CustomLine(StartPoint, otherStartPoint);
            }
            
            return mergeResult.Length != 0f ? mergeResult : default;
        }

        public bool Equals(CustomLine otherLine)
        {
            return otherLine != null && (Direction == otherLine.Direction ? 
                                            StartPoint == otherLine.StartPoint && 
                                            EndPoint == otherLine.EndPoint : 
                                            
                                            StartPoint == otherLine.EndPoint && 
                                            EndPoint == otherLine.StartPoint);
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as CustomLine);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartPoint, EndPoint);
        }
        
        public override string ToString()
        {
            return "Start: " + StartPoint + ", End: " + EndPoint + ", Middle: " + MiddlePoint + ", Direction: " + Direction + ", Length: " + Length + "m";
        }
    }
}