using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private float moveFactor = 25f;
    [SerializeField] private float zoomFactor = 500f;
    [SerializeField] private float rotationFactor = 150f;
    [SerializeField] private float height = 20f;
    
    private Camera _mainCamera;
    private Vector3 _direction;
    private bool _drag;

    private const float MaxFov = 80f;
    private const float MinFov = 10f;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        
        _direction = new Vector3();
        transform.rotation = Quaternion.Euler(40f, 0f, 0f);
    }

    private void Update()
    {
        UpdateInput();
        Move();
    }

    private void UpdateInput()
    {
        _direction.x = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
        _direction.z = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        _drag = Input.GetMouseButton(1);
    }

    private void Move()
    {
        var form = transform;
        var moveDirection = form.forward * _direction.z + form.right * _direction.x;

        var position = form.position;
        position += moveDirection * (moveFactor * Time.deltaTime);
        
        // height should always stay the same
        position = new Vector3(position.x, height, position.z);

        form.position = position;
        
        UpdateRotation();
        UpdateZoom();
    }

    private void UpdateRotation()
    {
        if (!_drag)
        {
            return;
        }
        
        var xAxis = Input.GetAxis("Mouse X");
        if (xAxis == 0)
        {
            return;
        }

        var form = transform;
        
        // point towards the ground
        var point = form.position + form.forward * (height * 1.25f);
        
        // rotate around that point
        transform.RotateAround(point, new Vector3(0f, 1f,0f), xAxis * (rotationFactor * Time.deltaTime));
    }
    
    private void UpdateZoom()
    {
        var scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta == 0)
        {
            return;
        }

        var fov = _mainCamera.fieldOfView;
        fov -= scrollDelta * (zoomFactor * Time.deltaTime);

        if (fov < MinFov)
        {
            fov = MinFov;
        }
        
        if (fov > MaxFov)
        {
            fov = MaxFov;
        }

        // change FOV to zoom
        _mainCamera.fieldOfView = fov;
    }
}
