using System.Collections.Generic;
using Lines.Other;
using UnityEngine;

namespace Lines.Handler
{
    public interface ILineHandler
    {
        IEnumerable<CustomLine> Split(CustomLine[] lines, CustomLine newLine, float gridSize);

        IEnumerable<CustomLine> Merge(CustomLine[] lines, CustomLine newLine);
        
        IEnumerable<CustomLine> Combine(CustomLine[] lines, CustomLine newLine, float gridSize);
        
        List<Vector3> GetIntersections();
    }
}