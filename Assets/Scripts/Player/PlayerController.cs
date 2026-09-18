using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private int _currentBounceCount = 0;

    private Rigidbody _body;

    private Vector2 _lookDelta;

    // 현재 추적해야 하는 목표 회전
    private Quaternion _targetRotation;

    private bool _isCollision = false;


    private void Awake()
    {
        _body = GetComponent<Rigidbody>();

        // 최초 회전값으로 초기화
        _targetRotation = _body.rotation;

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

        // 충돌 중에는 VisibleTarget 방향으로
        // 목표 회전을 갱신하지 않는다.
        if (!_isCollision)
        {
            HandleRotation();
        }

        UpdateRotation();
    }


    private void OnLook(InputValue value)
    {
        _lookDelta -= value.Get<Vector2>();
    }


    private void OnAttack(InputValue value)
    {
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

        _visibleTarget.position += movement * _sensitivity;

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

    private void HandleAcceleration()
    {
        Vector3 targetVelocity =
            transform.forward * _moveSpeed;

        Vector3 velocityDelta =
            targetVelocity - _body.linearVelocity;

        if (_moveResponseTime <= 0f)
        {
            _body.linearVelocity = targetVelocity;
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


    // =========================================================
    // Collision
    // =========================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.transform.CompareTag("Stuckable"))
            return;

        if (_currentBounceCount >= _maxBounce)
            return;

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


    private IEnumerator CollisionTimer()
    {
        _isCollision = true;

        yield return new WaitForSeconds(_collisionTime);

        _isCollision = false;
    }
}