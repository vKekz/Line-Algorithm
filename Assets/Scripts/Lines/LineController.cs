using System.Collections.Generic;
using Grid;
using Lines.Handler;
using Lines.Other;
using UnityEditor;
using UnityEngine;

namespace Lines
{
    [ExecuteInEditMode]
    public class LineController : MonoBehaviour
    {

        [SerializeField]
        public bool combine = true;
        
        private ILineHandler _lineHandler;
        private List<CustomLine> _lines;
        
        private GridGenerator _gridGenerator;
        private Vector3 _startPosition, _endPosition;

        private void OnEnable()
        {
            _gridGenerator = GetComponent<GridGenerator>();

            _lineHandler = new LineHandler();
            _lines = new List<CustomLine>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _lineHandler.GetIntersections().Clear();
                _lines.Clear();
            }
            if (Input.GetMouseButtonDown(0))
            {
                _startPosition = _gridGenerator.GetMousePositionOnGrid();
            }

            if (_startPosition == null)
            {
                return;
            }
            if (Input.GetMouseButtonUp(0))
            {
                _endPosition = _gridGenerator.GetMousePositionOnGrid();
                
                // Create a new line based on start and endpoint of mouse
                var newLine = new CustomLine(_startPosition, _endPosition);
                if (newLine.GetLength() < _gridGenerator.gridSize)
                {
                    return;
                }
                
                _lines.Add(newLine);
            
                var splitLines = combine ? _lineHandler.Combine(_lines.ToArray(), newLine, _gridGenerator.gridSize) : 
                    _lineHandler.Split(_lines.ToArray(), newLine, _gridGenerator.gridSize);
            
                // Add new lines to previous list
                _lines.Clear();
                _lines.AddRange(splitLines);
            }
        }

        private void OnGUI()
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                
                Handles.DrawLine(line.StartPoint, line.EndPoint, 3f);
                Handles.Label(line.GetMiddlePoint(), "(" + (i + 1) + ") " + line.GetLength() + "m");
            }
            
            foreach (var intersection in _lineHandler.GetIntersections())
            {
                Handles.DrawDottedLine(intersection, new Vector3(intersection.x, 0.75f, intersection.z), 2);
            }
        }
    }
}