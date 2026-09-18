using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [Header("Movement Parameter")]
    [SerializeField] private float _sensitivity = 0.5f;
    [SerializeField] private float _minPitch = -40f, _maxPitch = 70f;
    [SerializeField] private float _moveSpeed = 30f;
    [SerializeField] private Transform _visibleTarget;
    private Vector2 _rotationValue = Vector2.zero;
    //private Vector2 _target = Vector2.zero;
    private Vector2 _lookDelta;

    private float _positionOffset = 5f;
    private float _rotationOffset = 0f;
    private float resetDuration = 3f;

    private bool _isStuck = false;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleVisibleTarget();
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
        /*
        Vector2 inputValue = value.Get<Vector2>();

        Debug.Log(inputValue);

        _rotationValue.x -= inputValue.y * _sensitivity;
        _rotationValue.y += inputValue.x * _sensitivity;

        _rotationValue.x = Mathf.Clamp(_rotationValue.x, _minPitch, _maxPitch);

        _visibleTarget.transform.position -= new Vector3(inputValue.x, 0, inputValue.y) * _sensitivity ;
        */


        _lookDelta -= value.Get<Vector2>();
    }

    private void OnAttack(InputValue value)
    {
        if (!_isStuck) return;

        if (value.isPressed)
        {
            StartCoroutine(ResetTransform());
        }
    }
    
    private void HandleVisibleTarget()
    {
        Vector3 movement = new Vector3(
            _lookDelta.x,
            0f,
            _lookDelta.y
        );

        _visibleTarget.position +=
            movement * _sensitivity;

        _lookDelta = Vector2.zero;
    }

    private void HandleRotation()
    {
        //transform.rotation = Quaternion.Euler(_rotationValue.x, _rotationValue.y, 0f);
        transform.LookAt(_visibleTarget);
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

    private IEnumerator ResetTransform()
    {
        float elapsed = 0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * _positionOffset;

        Quaternion startRotation = transform.rotation;

        Vector3 startEuler = transform.eulerAngles;

        Quaternion targetRotation = Quaternion.Euler(
            0f,
            startEuler.y,
            0f
        );

        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / resetDuration);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            transform.rotation = Quaternion.Lerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        _rotationValue.x = 0f;
        _rotationValue.y = transform.eulerAngles.y;

        _isStuck = false;
    }
}
