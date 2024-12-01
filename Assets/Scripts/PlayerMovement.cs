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

    private Animator _animator;                                // Ссылка на Animator
    private CharacterController _characterController;          // Ссылка на CharacterController
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
        _characterController = GetComponent<CharacterController>();
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
        {
            vertical = 0;
        }
        
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            
            _moveDirection = Vector3.Lerp(_moveDirection, transform.TransformDirection(inputDirection) * currentSpeed, 0.1f);
            _isMoving = true;
        }
        else
        {
            _moveDirection = Vector3.Lerp(_moveDirection, Vector3.zero, 0.1f);
            _isMoving = false;
        }
        
        _characterController.Move(_moveDirection * Time.deltaTime);
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
    }
    
    /// <summary>
    /// Обработка танца
    /// </summary>
    void HandleDance()
    {
        if (Input.GetKeyDown(KeyCode.Z)) // Нажатие клавиши Z
        {
            _isDancing = true; // Начинаем танцевать
        }

        // Прерывание танца, если игрок начинает двигаться
        if (_isDancing && _isMoving)
        {
            _isDancing = false; // Прерываем танец при движении
        }
    }
    
    /// <summary>
    /// Проверка, находится ли персонаж на земле с помощью луча.
    /// </summary>
    bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, _characterController.height / 6 + groundCheckDistance))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Гравитация персонажа
    /// </summary>
    void ApplyGravity()
    {
        _isGrounded = IsGrounded();
        
        Debug.Log("Is Grounded: " + _isGrounded);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _isFalling = false;
            if (!_isJumping && !_isLanding)
            {
                _isLanding = true;
            }
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
            if (!_isJumping && !_isFalling)
            {
                _isFalling = true;
            }
        }
        
        _characterController.Move(_velocity * Time.deltaTime);
    }
    
    /// <summary>
    /// Прыжок персонажа
    /// </summary>
    void HandleJump()
    {
        if (_isGrounded && Input.GetButtonDown("Jump") && Time.time - _lastJumpTime >= jumpCooldown) // Проверка на землю и нажатие клавиши прыжка
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Расчет начальной скорости для достижения высоты прыжка
            _isJumping = true;  // Срабатывание анимации прыжка
            _lastJumpTime = Time.time;
        }
    }

    /// <summary>
    /// Обновление параметров анимации
    /// </summary>
    void UpdateAnimator()
    {
        // Устанавливаем параметры для анимации в зависимости от состояния
        _animator.SetBool("IsJumping", _isJumping);
        _animator.SetBool("IsFalling", _isFalling);
        _animator.SetBool("IsLanding", _isLanding);
        _animator.SetBool("IsWalking", _isMoving);
        _animator.SetBool("IsDancing", _isDancing);
        
        if (_isMoving)
        {
            _animator.SetBool("IsRunning", Input.GetKey(KeyCode.LeftShift));
        }

        // После приземления, сбрасываем флаг landing, чтобы анимация не зацикливалась
        if (_isLanding)
        {
            _isLanding = false; // Сбрасываем флаг приземления
            _isJumping = false; // Сбрасываем флаг прыжка
            _lastLandingTime = Time.time;
        }
    }
}
