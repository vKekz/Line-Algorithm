using System;
using UnityEngine;

namespace Lines.Other
{
    public class CustomLine : IEquatable<CustomLine>
    {
        public Vector3 StartPoint { get; }
        public Vector3 EndPoint { get; }
        public Vector3 MiddlePoint { get; }
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
            Direction = GetDirection();
            Length = GetLength();
        }

        public CustomLine MakeSkip()
        {
            Skip = true;
            return this;
        }
        
        /// <returns>Returns a Vector where the intersection happens, if there is one. Otherwise it returns default.</returns>
        public Vector3 Intersects(CustomLine customLine)
        {
            var firstLineStart = StartPoint;
            var firstLineEnd = EndPoint;

            var secondLineStart = customLine.StartPoint;
            var secondLineEnd = customLine.EndPoint;
            
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
            
            // Check if the intersection point is on both lines
            if (ContainsPoint(intersectionPoint) && customLine.ContainsPoint(intersectionPoint))
            {
                return new Vector3(x, 0, z);
            }

            return default;
        }

        // TODO: improve check, because diagonal lines are not working as intended
        /// <returns>Returns true if the given point is on the line.</returns>
        public bool ContainsPoint(Vector3 point)
        {
            return ((point.x >= StartPoint.x && point.x <= EndPoint.x) || (point.x <= StartPoint.x && point.x >= EndPoint.x)) && 
                ((point.z >= StartPoint.z && point.z <= EndPoint.z) || (point.z <= StartPoint.z && point.z >= EndPoint.z));
        }
        
        private Vector3 GetMiddlePoint()
        {
            var middleX = (StartPoint.x + EndPoint.x) / 2;
            var middleZ = (StartPoint.z + EndPoint.z) / 2;
            
            return new Vector3(middleX, 0, middleZ);
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
            if (crossProduct == 0f)
            {
                return Direction.Equal;
            }
            
            return crossProduct > 0 ? Direction.Right : Direction.Left;
        }

        private float GetLength()
        {
            return Mathf.Sqrt(Mathf.Pow(EndPoint.x - StartPoint.x, 2) + Mathf.Pow(EndPoint.z - StartPoint.z, 2));
        }
        
        public override string ToString()
        {
            return "Start: " + StartPoint + ", End: " + EndPoint + ", Middle: " + MiddlePoint + ", Direction: " + Direction + ", Length: " + Length + "m";
        }

        public bool Equals(CustomLine otherLine)
        {
            return otherLine != null && StartPoint == otherLine.StartPoint && EndPoint == otherLine.EndPoint;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as CustomLine);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartPoint, EndPoint);
        }
    }
}