using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridGenerator : MonoBehaviour
    {
        public Transform spherePrefab;
        
        public int length = 100;
        public int gridSize = 1;

        private Camera _mainCamera;
        private Plane _plane;

        private List<Transform> _spheres;
        
        private void Start()
        {
            Setup();
        }

        private void Setup()
        {
            _spheres = new List<Transform>();
            _mainCamera = Camera.main;
            
            _plane = new Plane(new Vector3(0, 0, 0), new Vector3(0, 0, CalculateMaximumLength()),
                new Vector3(CalculateMaximumLength(), 0, 0));
            
            Generate();
        }

        private void Generate()
        {
            for (var x = 0; x < length; x++)
            {
                if (x % gridSize != 0)
                {
                    continue;
                }
                
                for (var z = 0; z < length; z++)
                {
                    if (z % gridSize != 0)
                    {
                        continue;
                    }
                    _spheres.Add(Instantiate(spherePrefab, new Vector3(x, 0f, z), Quaternion.Euler(0f, 0f, 0f)));
                }
            }
        }

        private Vector3 GetNearestGridPointFromPosition(Vector3 position)
        {
            var max = CalculateMaximumLength();
            var x = Mathf.RoundToInt(position.x / gridSize);
            var z = Mathf.RoundToInt(position.z / gridSize);

            var result = new Vector3(x, 0f, z) * gridSize;
            if (result.x < 0)
            {
                result.x = 0;
            }
            if (result.x > max)
            {
                result.x = max;
            }
            
            if (result.z < 0)
            {
                result.z = 0;
            }
            if (result.z > max)
            {
                result.z = max;
            }

            return result;
        }

        public Vector3 GetMousePositionOnGrid()
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!_plane.Raycast(ray, out var enter))
            {
                return default;
            }
            
            var worldPosition = ray.GetPoint(enter);
            var max = CalculateMaximumLength();
            
            if (worldPosition.x < 0f)
            {
                worldPosition.x = 0f;
            }
            if (worldPosition.x > max)
            {
                worldPosition.x = max;
            }
            
            if (worldPosition.z < 0f)
            {
                worldPosition.z = 0f;
            }
            if (worldPosition.z > max)
            {
                worldPosition.z = max;
            }

            return GetNearestGridPointFromPosition(worldPosition);
        }

        private float CalculateMaximumLength()
        {
            return (float) (Math.Sqrt(_spheres.Count) - 1) * gridSize;
        }
    }
}