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
            if (Input.GetKeyDown(KeyCode.C))
            {
                _lineHandler.GetIntersections().Clear();
                _lines.Clear();
            }
            if (Input.GetMouseButtonDown(0))
            {
                _startPosition = _gridGenerator.GetMousePositionOnGrid();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_lines.Count > 1)
                {
                    _lines.RemoveAt(_lines.Count - 1);
                }
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
                if (newLine.Length < _gridGenerator.gridSize)
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
                Handles.Label(line.MiddlePoint, "(" + (i + 1) + ") " + line.Length + "m " + (line.Direction == Direction.Right ? ">" : "<"));
            }

            for (var i = 0; i < _lineHandler.GetIntersections().Count; i++)
            {
                var intersection = _lineHandler.GetIntersections()[i];
                
                Handles.DrawDottedLine(intersection, new Vector3(intersection.x, 0.75f, intersection.z), 2);
                
                intersection.y += 1f;
                Handles.Label(intersection, "(" + (i + 1) + ")");
            }
        }
    }
}