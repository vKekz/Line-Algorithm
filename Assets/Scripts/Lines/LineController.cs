using System;
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
            // Clear for debugging
            if (Input.GetKeyDown(KeyCode.C))
            {
                _lineHandler.GetIntersections().Clear();
                _lines.Clear();
            }
            // Remove latest line
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_lines.Count > 0)
                {
                    _lines.RemoveAt(_lines.Count - 1);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                _startPosition = _gridGenerator.GetMousePositionOnGrid();
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

                var start = DateTime.Now.Ticks / (decimal) TimeSpan.TicksPerMillisecond;
                
                var splitLines = combine ? 
                    _lineHandler.Combine(_lines.ToArray(), newLine) : _lineHandler.Split(_lines.ToArray(), newLine);
                
                var stop = DateTime.Now.Ticks / (decimal) TimeSpan.TicksPerMillisecond;
                var diff = stop - start;
                
                Debug.Log("Modifying lines took " + diff + "ms");
                
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

                Handles.color = Color.yellow;
                Handles.DrawLine(line.StartPoint, line.EndPoint, 3f);
                Handles.Label(line.MiddlePoint, "(" + (i + 1) + ") " + Math.Round(line.Length, 2) + "m " + (line.Direction == Direction.Right ? ">" : "<"));
            }

            for (var i = 0; i < _lineHandler.GetIntersections().Count; i++)
            {
                var intersection = _lineHandler.GetIntersections()[i];

                Handles.color = Color.red;
                Handles.DrawDottedLine(intersection, intersection + Vector3.up * 1f, 2);
                Handles.Label(intersection + Vector3.up * 1.1f, "(" + (i + 1) + ")");
            }
        }
    }
}