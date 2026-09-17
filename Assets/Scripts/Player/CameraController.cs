using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Vector3 _currentVelocity;
    private Camera _cam;
    
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1, -3);

    private float smoothTime = 0.05f;


    private void Start()
    {
        _cam = GetComponent<Camera>();
        ApplyViewImmediate();
    }

    private void ApplyViewImmediate()
    {
        transform.position = _target.TransformPoint(_offset);
        transform.LookAt(_target.transform.position);
        _currentVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        Vector3 targetWorldPos = _target.TransformPoint(_offset);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetWorldPos,
            ref _currentVelocity,
            smoothTime
        );

        Vector3 lookTarget = _target.transform.position;
        transform.LookAt(lookTarget);
    }

}
