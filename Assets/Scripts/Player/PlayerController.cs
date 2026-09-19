using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameter")]
    [SerializeField] private float _sensitivity = 0.5f;
    [SerializeField] private float _moveSpeed = 30f;
    [Header("Target")]
    [SerializeField] private Transform _visibleTarget;

    [Header("Response Time")]
    [SerializeField] private float _moveResponseTime = 0.2f;
    [SerializeField] private float _rotateResponseTime = 0.2f;

    [Header("Collision")]
    [SerializeField] private int _maxBounce = 3;
    [SerializeField] private float _collisionTime = 1f;
    [SerializeField] private float _bounceForce = 10f;
    [SerializeField] private float _resetDuration = 1f;

    [Header("Flight Gauge")]
    [SerializeField] private float _maxGauge = 20f;
    [SerializeField] private Slider _flightGaugeUI;
    [SerializeField] private GameObject _FlightGaugeFillArea;


    private int _currentBounceCount = 0;
    private Rigidbody _body;
    private Vector2 _lookDelta;
    private Vector3 _targetVelocity;
    private Quaternion _targetRotation;
    private bool _isCollision = false;

    private bool _isTryFlight = false;
    private float _currentGauge = 0f;

    private bool _isStuck = false;


    private void Awake()
    {
        _body = GetComponent<Rigidbody>();

        _targetRotation = _body.rotation;

        _currentGauge = _maxGauge;

        _flightGaugeUI.maxValue = _maxGauge;
        _flightGaugeUI.value = _currentGauge;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        HandleVisibleTarget();
    }


    private void FixedUpdate()
    {
        HandleAcceleration();
        UpdateRotation();

        if (!_isTryFlight) return;
        if (_currentGauge <= 0) return;
        // 누르고 있을 때 넘어갈 수 있고
        // 누르고 있는거랑 무관하게 Accel은 되는데, target이 0으로 잡히도록

        // 충돌 중에는 VisibleTarget 방향으로
        // 목표 회전을 갱신하지 않는다.
        if (!_isCollision)
        {
            HandleRotation();
        }
    }


    private void OnLook(InputValue value)
    {
        _lookDelta -= value.Get<Vector2>();
    }


    private void OnAttack(InputValue value)
    {
        _isTryFlight = value.isPressed;
        if(_isTryFlight && _currentGauge > 0)
        {
            StartCoroutine(UseGauge(0.1f, 0.1f));
            StopCoroutine(RegainGauge(1f, 1f));
        }
        else
        {
            StartCoroutine(RegainGauge(1f, 1f));
        }
    }


    // =========================================================
    // Visible Target
    // =========================================================

    private void HandleVisibleTarget()
    {
        Vector3 movement = new Vector3(
            _lookDelta.x,
            0f,
            _lookDelta.y
        );

        _visibleTarget.position -= movement * _sensitivity;

        _lookDelta = Vector2.zero;
    }


    // =========================================================
    // Rotation
    // =========================================================

    /// <summary>
    /// VisibleTarget 방향을 새로운 목표 회전으로 설정한다.
    /// 실제 회전은 UpdateRotation에서 이루어진다.
    /// </summary>
    private void HandleRotation()
    {
        Vector3 direction =
            _visibleTarget.position - transform.position;

        // Top View이므로 Y축 방향 제거
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        SetTargetRotation(targetRotation);
    }


    /// <summary>
    /// 회전의 목표값만 설정한다.
    /// 즉시 회전시키지 않는다.
    /// </summary>
    private void SetTargetRotation(Quaternion targetRotation)
    {
        _targetRotation = targetRotation;
    }


    /// <summary>
    /// 현재 회전에서 목표 회전으로 점진적으로 접근한다.
    /// </summary>
    private void UpdateRotation()
    {
        if (_rotateResponseTime <= 0f)
        {
            _body.MoveRotation(_targetRotation);
            return;
        }

        float rotationDelta =
            Quaternion.Angle(
                _body.rotation,
                _targetRotation
            );

        float responseAngularSpeed =
            rotationDelta / _rotateResponseTime;

        float rotationChange =
            responseAngularSpeed * Time.fixedDeltaTime * 10;

        rotationChange = Mathf.Min(
            rotationChange,
            rotationDelta
        );

        Quaternion nextRotation =
            Quaternion.RotateTowards(
                _body.rotation,
                _targetRotation,
                rotationChange
            );

        _body.MoveRotation(nextRotation);
    }


    // =========================================================
    // Movement
    // =========================================================

    //HandleAcceleration이랑 UpdateAcceleration이랑 나누기
    private void HandleAcceleration()
    {
        if (_isTryFlight && _currentGauge > 0)
            SetTargetVelocity(transform.forward * _moveSpeed);
        else
            SetTargetVelocity(Vector3.zero);


        Vector3 velocityDelta =
            _targetVelocity - _body.linearVelocity;

        if (_moveResponseTime <= 0f)
        {
            _body.linearVelocity = _targetVelocity;
            return;
        }

        Vector3 responseAcceleration =
            velocityDelta / _moveResponseTime;

        Vector3 velocityChange =
            responseAcceleration * Time.fixedDeltaTime;

        velocityChange = Vector3.ClampMagnitude(
            velocityChange,
            velocityDelta.magnitude
        );

        Vector3 acceleration =
            velocityChange / Time.fixedDeltaTime;

        _body.AddForce(
            acceleration,
            ForceMode.Acceleration
        );
    }
    private void SetTargetVelocity(Vector3 targetVelocity)
    {
        _targetVelocity = targetVelocity;
    }


    // =========================================================
    // Collision
    // =========================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Bounceable"))
        {
            if (_currentBounceCount >= _maxBounce)
            {
                return;
            }
                

            _currentBounceCount++;


            // 충돌 지점의 법선 벡터
            Vector3 normal =
                collision.GetContact(0).normal;


            // 현재 진행 방향을 충돌면 기준으로 반사
            Vector3 reflectDirection = Vector3.Reflect(
                transform.forward,
                normal
            );

            // Top View이므로 Y축 제거
            reflectDirection.y = 0f;


            if (reflectDirection.sqrMagnitude <= 0.001f)
                return;


            reflectDirection.Normalize();


            // 반사 방향을 목표 회전으로 설정
            Quaternion bounceRotation =
                Quaternion.LookRotation(reflectDirection);

            SetTargetRotation(bounceRotation);


            // 반사 방향으로 물리적인 힘도 가함
            _body.AddForce(
                reflectDirection * _bounceForce,
                ForceMode.Impulse
            );


            StartCoroutine(CollisionTimer());
        }
    }


    private IEnumerator CollisionTimer()
    {
        _isCollision = true;

        yield return new WaitForSeconds(_collisionTime);

        _isCollision = false;
    }

    private void StuckInteraction()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f))
        {
            if (hit.transform.CompareTag("Bounceable")) 
            {
                if(_currentBounceCount >= _maxBounce)
                {
                    _isStuck = true;
                }
                return;
            }
            if (hit.transform.CompareTag("Stuckable"))
            {
                _isStuck = true;
            }
        }
    }

    // =========================================================
    // Flight Gauge
    // =========================================================

    private IEnumerator UseGauge(float consumeGauge, float consumeInterval)
    {
        while(_isTryFlight)
        {

            _currentGauge -= consumeGauge;


            _currentGauge = Mathf.Clamp(_currentGauge, 0, _maxGauge);

            _flightGaugeUI.value = _currentGauge;

            if (_currentGauge <= 0)
            {
                SetTargetVelocity(Vector3.zero);
                _FlightGaugeFillArea.SetActive(false);
            }

            yield return new WaitForSeconds(consumeInterval);
        }
    }

    private IEnumerator RegainGauge(float regainGauge, float regainInterval)
    {
        yield return new WaitForSeconds(regainInterval);
        while (!_isTryFlight)
        {
            _currentGauge += regainGauge;

            _currentGauge = Mathf.Clamp(_currentGauge, 0, _maxGauge);

            _flightGaugeUI.value = _currentGauge;
            
            if (!_FlightGaugeFillArea.activeInHierarchy)
            {
                _FlightGaugeFillArea.SetActive(true);
            }

            yield return new WaitForSeconds(regainInterval);
        }
    }

    private IEnumerator ResetTransform()
    {
        float elapsed = 0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition;

        Quaternion startRotation = transform.rotation;

        Vector3 startEuler = transform.eulerAngles;

        Quaternion targetRotation = Quaternion.Euler(
            0f,
            startEuler.y,
            0f
        );

        while (elapsed < _resetDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _resetDuration);

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

        _isStuck = false;
    }
}