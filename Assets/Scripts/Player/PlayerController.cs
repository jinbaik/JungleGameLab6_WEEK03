using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [Header("Movement Parameter")]
    [SerializeField] private float _sensitivity = 0.5f;
    [SerializeField] private float _minPitch = -40f, _maxPitch = 70f;
    [SerializeField] private float _moveSpeed = 30f;
    private Vector2 _rotationValue = Vector2.zero;

    private bool _isStuck = false;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        WallInteraction();

        if (_isStuck) return;

        HandleAcceleration();
        HandleRotation();
    }

    private void OnLook(InputValue value)
    {
        Vector2 inputValue = value.Get<Vector2>();

        _rotationValue.x -= inputValue.y * _sensitivity;
        _rotationValue.y += inputValue.x * _sensitivity;

        _rotationValue.x = Mathf.Clamp(_rotationValue.x, _minPitch, _maxPitch);
    }
    private void HandleRotation()
    {
        transform.rotation = Quaternion.Euler(_rotationValue.x, _rotationValue.y, 0f);
    }

    private void HandleAcceleration()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * _moveSpeed);

        if (transform.position.y < 0.4f)
        {
            transform.position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }
    }

    private void WallInteraction()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f))
        {
            if(hit.transform.CompareTag("Stuckable"))
            {
                _isStuck = true;
            }
        }
    }
}
