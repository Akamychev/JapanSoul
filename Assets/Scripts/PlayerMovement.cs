using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;        // Скорость движения
    [SerializeField] private float rotationSpeed = 5f;    // Скорость поворота
    [SerializeField] private float gravity = -9.81f;      // Сила гравитации
    [SerializeField] private float jumpHeight = 2f;       // Высота прыжка (если нужно)

    private Animator _animator;                           // Ссылка на Animator
    private CharacterController _characterController;     // Ссылка на CharacterController
    private Vector3 _moveDirection;                       // Движение игрока
    private Vector3 _velocity;                            // Вертикальная скорость (для гравитации)
    
    private bool _isMoving;                               // Флаг движения
    private bool _isGrounded;                             // Флаг земли

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        ApplyGravity();
    }

    /// <summary>
    /// Мувмент персонажа
    /// </summary>
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            _moveDirection = transform.TransformDirection(inputDirection) * moveSpeed;
            _isMoving = true;
            _animator.SetTrigger("MoveTrigger");
        }
        else
        {
            _moveDirection = Vector3.zero;
            _isMoving = false;
            _animator.SetTrigger("IdleTrigger");
        }
        
        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Поворот персонажа
    /// </summary>
    void HandleRotation()
    {
        if (_isMoving)
        {
            Quaternion toRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Гравитация персонажа
    /// </summary>
    void ApplyGravity()
    {
        _isGrounded = _characterController.isGrounded;

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }
        
        _characterController.Move(_velocity * Time.deltaTime);
    }

    // void UpdateAnimator()
    // {
    //     // Получаем текущую скорость
    //     float speed = _characterController.velocity.magnitude;
    //
    //     // Проверяем, достаточно ли скорость велика для движения
    //     if (speed > idleThreshold)
    //     {
    //         // Двигаемся, анимация ходьбы
    //         _animator.SetFloat("Speed", speed);
    //     }
    //     else
    //     {
    //         // Не двигаемся, анимация idle
    //         _animator.SetFloat("Speed", 0f);
    //     }
    // }
}
