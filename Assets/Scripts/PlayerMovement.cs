using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;             // Скорость движения
    [SerializeField] private float runSpeed = 6f;              // Скорость бега
    [SerializeField] private float rotationSpeed = 5f;         // Скорость поворота
    [SerializeField] private float gravity = -9.81f;           // Сила гравитации
    [SerializeField] private float jumpHeight = 2f;            // Высота прыжка (если нужно)
    [SerializeField] private float groundCheckDistance = 0.1f; // Расстояние для проверки земли
    [SerializeField] private float jumpCooldown = 0.2f;        // Кулдаун для прыжка (в секундах)
    [SerializeField] private LayerMask groundLayer;

    private Animator _animator;                                // Ссылка на Animator
    private Rigidbody _rigidbody;                              // Ссылка на Rigidbody
    private Vector3 _moveDirection;                            // Движение игрока
    private Vector3 _velocity;                                 // Вертикальная скорость (для гравитации)
    
    private bool _isMoving;                                    // Флаг движения
    private bool _isGrounded;                                  // Флаг земли
    private bool _isJumping;                                   // Флаг прыжка
    private bool _isFalling;                                   // Флаг падения
    private bool _isLanding;                                   // Флаг приземления
    private float _lastJumpTime;                               // Время последнего прыжка
    private float _lastLandingTime;                            // Время последнего приземления
    private bool _isDancing;                                   // Флаг танца

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _animator = GetComponent<Animator>();
        _lastJumpTime = -jumpCooldown;
        _lastLandingTime = -jumpCooldown;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        ApplyGravity();
        HandleJump();
        UpdateAnimator();
        HandleDance();
    }

    /// <summary>
    /// Мувмент персонажа
    /// </summary>
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (vertical < 0)
            vertical = 0;

        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            Vector3 desiredVelocity = transform.TransformDirection(inputDirection) * currentSpeed;

            
            Vector3 velocity = Vector3.Lerp(_rigidbody.velocity, new Vector3(desiredVelocity.x, _rigidbody.velocity.y, desiredVelocity.z), Time.deltaTime * 10f);
            _rigidbody.velocity = velocity;
            _moveDirection = velocity; 
            _isMoving = true;
        }
        else
        {
            Vector3 velocity = Vector3.Lerp(_rigidbody.velocity, new Vector3(0, _rigidbody.velocity.y, 0), Time.deltaTime * 10f);
            _rigidbody.velocity = velocity;
            _moveDirection = velocity;
            _isMoving = false;
        }
    }

    /// <summary>
    /// Поворот персонажа
    /// </summary>
    void HandleRotation()
    {
        if (_isMoving && !_isJumping)
        {
            Quaternion toRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        
        if (_isJumping || _isFalling)
        {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
    
    /// <summary>
    /// Обработка танца
    /// </summary>
    void HandleDance()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _isDancing = true;
        }
        
        if (_isDancing && _isMoving)
        {
            _isDancing = false;
        }
    }
    
    /// <summary>
    /// Проверка, находится ли персонаж на земле с помощью луча.
    /// </summary>
    bool IsGrounded()
    {
        RaycastHit hit;

        Vector3 colliderCenter = transform.position;
        Debug.DrawRay(colliderCenter, Vector3.down * groundCheckDistance, Color.red, 1f);

        if (Physics.Raycast(colliderCenter, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            Debug.Log($"Ray hit: {hit.collider.name}");
            return hit.collider != null;
        }
        Debug.Log("Ray did not hit anything");
        return false;
    }

    /// <summary>
    /// Гравитация персонажа
    /// </summary>
    void ApplyGravity()
    {
        _isGrounded = IsGrounded();
        
        Debug.Log("Is Grounded: " + _isGrounded);

        if (_isGrounded && _rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -0.2f, _rigidbody.velocity.z);
            _isFalling = false;
            if (!_isJumping && !_isLanding)
            {
                _isLanding = true;
            }
        }
        else
        {
            _rigidbody.velocity += Vector3.up * (gravity * Time.deltaTime);
            if (!_isJumping && !_isFalling)
            {
                _isFalling = true;
            }
        }
    }
    
    /// <summary>
    /// Прыжок персонажа
    /// </summary>
    void HandleJump()
    {
        if (_isGrounded && Input.GetButtonDown("Jump") && Time.time - _lastJumpTime >= jumpCooldown)
        {
            if (!_isJumping && !_isFalling)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _velocity.y, _rigidbody.velocity.z);
                _isJumping = true;
                _lastJumpTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Обновление параметров анимации
    /// </summary>
    void UpdateAnimator()
    {
        _animator.SetBool("IsJumping", _isJumping);
        _animator.SetBool("IsFalling", _isFalling);
        _animator.SetBool("IsLanding", _isLanding);
        _animator.SetBool("IsWalking", _isMoving);
        _animator.SetBool("IsDancing", _isDancing);
        
        if (_isMoving)
        {
            _animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
        }
        
        if (_isLanding)
        {
            _isLanding = false;
            _isJumping = false;
            _lastLandingTime = Time.time;
        }
        
        if (_isGrounded && _rigidbody.velocity.y <= 0.1f && !_isMoving)
        {
            _isFalling = false;
            _isJumping = false;
        }
    }
}
