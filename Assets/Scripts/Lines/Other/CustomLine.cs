using UnityEngine;

namespace Lines.Other
{
    public struct CustomLine
    {
        public Vector3 StartPoint { get; }
        public Vector3 EndPoint { get; }
        public bool Remove { get; private set; }

        public CustomLine(Vector3 startPoint, Vector3 endPoint, bool remove = false)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Remove = remove;
        }

        public CustomLine ToRemove()
        {
            Remove = true;
            return this;
        }
        
        public CustomLine ResetRemove()
        {
            Remove = false;
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

            // Check if out of bounds
            if (ContainsPoint(intersectionPoint) && customLine.ContainsPoint(intersectionPoint))
            {
                return new Vector3(x, 0, z);
            }

            return default;
        }

        /// <returns>Returns true if the given point is on the line.</returns>
        private bool ContainsPoint(Vector3 point)
        {
            return ((point.x >= StartPoint.x && point.x <= EndPoint.x) || (point.x <= StartPoint.x && point.x >= EndPoint.x)) && 
                ((point.z >= StartPoint.z && point.z <= EndPoint.z) || (point.z <= StartPoint.z && point.z >= EndPoint.z));
        }

        /// <returns>Returns the middle point of the line.</returns>
        public Vector3 GetMiddlePoint()
        {
            var middleX = (StartPoint.x + EndPoint.x) / 2;
            var middleZ = (StartPoint.z + EndPoint.z) / 2;
            
            return new Vector3(middleX, 0, middleZ);
        }
        
        /// <returns>Returns the direction from start to end of the line.</returns>
        public Direction GetDirection()
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

        /// <returns>Returns the length from start to end of the line.</returns>
        public float GetLength()
        {
            return Mathf.Sqrt(Mathf.Pow(EndPoint.x - StartPoint.x, 2) + Mathf.Pow(EndPoint.z - StartPoint.z, 2));
        }
        
        public override string ToString()
        {
            return "Start: " + StartPoint + ", End: " + EndPoint + ", Middle: " + GetMiddlePoint() + ", Direction: " + GetDirection() + ", Length: " + GetLength() + "m";
        }
    }
}