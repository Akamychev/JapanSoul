using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 3f;             // Скорость движения
    [SerializeField] private float runSpeed = 6f;              // Скорость бега
    [SerializeField] private float rotationSpeed = 5f;         // Скорость поворота
    [SerializeField] private float gravity = -9.81f;           // Сила гравитации
    [SerializeField] private float jumpHeight = 2f;            // Высота прыжка (если нужно)
    // [SerializeField] private float groundCheckDistance = 0.1f; // Расстояние для проверки земли
    [SerializeField] private float jumpCooldown = 0.2f;        // Кулдаун для прыжка (в секундах)
    // [SerializeField] private LayerMask groundLayer;

    [Header("Audio Parameters")]
    [SerializeField] private AudioSource audioSource;          // Источник звука
    [SerializeField] private AudioSource walkAudioSource;      // Источник для звуков шагов
    [SerializeField] private AudioSource runAudioSource;       // Источник для звуков бега
    [SerializeField] private AudioClip walkSound;              // Звук шагов
    [SerializeField] private AudioClip runSound;               // Звук бега
    [SerializeField] private AudioClip jumpSound;              // Звук прыжка
    [SerializeField] private AudioClip landSound;              // Звук приземления
    [SerializeField] private AudioClip danceSound;             // Звук танца
    [SerializeField] private AudioClip[] walkingSurfaceSounds; // Звуки для разных поверхностей
    [SerializeField] private AudioClip[] runningSurfaceSounds; // Звуки для разных поверхностей
    [SerializeField] private AudioClip[] landingSurfaceSounds; // Звуки для разных поверхностей
    
    private Animator _animator;                                // Ссылка на Animator
    private Rigidbody _rigidbody;                              // Ссылка на Rigidbody
    private Vector3 _moveDirection;                            // Движение игрока
    private Vector3 _velocity;                                 // Вертикальная скорость (для гравитации)
    
    private bool _isMoving;                                    // Флаг движения
    private bool _isRunning;                                   // Флаг бега
    private bool _isGrounded;                                  // Флаг земли
    private bool _isJumping;                                   // Флаг прыжка
    private bool _isFalling;                                   // Флаг падения
    private bool _isLanding;                                   // Флаг приземления
    private float _lastJumpTime;                               // Время последнего прыжка
    private float _lastLandingTime;                            // Время последнего приземления
    private bool _isDancing;                                   // Флаг танца
    private int _currentSurface;                               // Текущая поверхность под ногами

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
        PlaySurfaceSound();
    }

    /// <summary>
    /// Мувмент персонажа
    /// </summary>
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        _isRunning = Input.GetKey(KeyCode.LeftShift);

        if (vertical < 0)
            vertical = 0;

        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            float currentSpeed = _isRunning ? runSpeed : moveSpeed;
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
            if (audioSource != null && danceSound != null)
            {
                audioSource.PlayOneShot(danceSound);
            }
        }
        
        if (_isDancing && _isMoving)
        {
            _isDancing = false;
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Обработка столкновений для определения нахождения на земле.
    /// </summary>
    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) ) != 0)
        {
            _isGrounded = true;

            _currentSurface = collision.gameObject.layer;
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) ) != 0)
        {
            _isGrounded = false;
        }
    }
    
    // /// <summary>
    // /// Проверка, находится ли персонаж на земле с помощью луча.
    // /// </summary>
    // bool IsGrounded()
    // {
    //     RaycastHit hit;
    //     
    //     Vector3 colliderCenter = transform.position;
    //     Debug.DrawRay(colliderCenter, Vector3.down * groundCheckDistance, Color.red, 1f);
    //     
    //     if (Physics.Raycast(colliderCenter, Vector3.down, out hit, groundCheckDistance, groundLayer))
    //     {
    //         return hit.collider != null;
    //     }
    //     return false;
    // }

    /// <summary>
    /// Гравитация персонажа
    /// </summary>
    void ApplyGravity()
    {
        // _isGrounded = IsGrounded();
        
        if (_isGrounded && _rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -0.01f, _rigidbody.velocity.z);
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

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
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
            _animator.SetBool("IsRunning", _isRunning);
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

    void PlaySurfaceSound()
    {
        if (_isJumping || _isDancing)
        {
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }
            if (runAudioSource.isPlaying)
            {
                runAudioSource.Stop();
            }
            return; // Прерываем выполнение метода, если игрок прыгает или танцует
        }
        
        // Если игрок не двигается, остановить оба звука
        if (!_isMoving)
        {
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop(); // Останавливаем звук шагов
            }
            if (runAudioSource.isPlaying)
            {
                runAudioSource.Stop(); // Останавливаем звук бега
            }
            return; // Выход из метода, не продолжаем проигрывать звук
        }

        // Если игрок бегает
        if (_isRunning)
        {
            if (!runAudioSource.isPlaying)  // Если звук бега не играет
            {
                if (walkAudioSource.isPlaying)
                {
                    walkAudioSource.Stop(); // Останавливаем звук шагов
                }

                AudioClip clipToPlay = runSound;

                // Выбираем звук для разных поверхностей
                switch (_currentSurface)
                {
                    case 7: // Layer "Wood"
                        walkAudioSource.Stop();
                        clipToPlay = runningSurfaceSounds[0];
                        break;
                    default:
                        walkAudioSource.Stop();
                        clipToPlay = runSound; // Стандартный звук для других поверхностей
                        break;
                }

                if (clipToPlay != null)
                {
                    runAudioSource.PlayOneShot(clipToPlay); // Воспроизводим звук бега
                }
            }
        }
        else // Если игрок ходит
        {
            if (!walkAudioSource.isPlaying)  // Если звук шагов не играет
            {
                if (runAudioSource.isPlaying)
                {
                    runAudioSource.Stop(); // Останавливаем звук бега
                }

                AudioClip clipToPlay = walkSound;

                // Выбираем звук для разных поверхностей
                switch (_currentSurface)
                {
                    case 7: // Layer "Wood"
                        walkAudioSource.Stop();
                        clipToPlay = walkingSurfaceSounds[0];
                        break;
                    default:
                        walkAudioSource.Stop();
                        clipToPlay = walkSound; // Стандартный звук для других поверхностей
                        break;
                }

                if (clipToPlay != null)
                {
                    walkAudioSource.PlayOneShot(clipToPlay); // Воспроизводим звук шагов
                }
            }
        }
    }
    
}
