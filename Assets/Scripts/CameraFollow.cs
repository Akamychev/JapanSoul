using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform user;                                  // Ссылка на игрока
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, -5f); // Смещение камеры относительно игрока
    [SerializeField] private float followSpeed = 10f;                        // Скорость следования камеры
    [SerializeField] private float rotationSpeed = 5f;                       // Скорость вращения камеры

    private float _currentXRotation = 0f;                                    // Текущий угол вращения по оси X
    private float _currentYRotation = 0f;                                    // Текущий угол вращения по оси Y

    [SerializeField] private float minXRotation = -30f;                      // Минимальный угол наклона камеры (вверх-вниз)
    [SerializeField] private float maxXRotation = 60f;                       // Максимальный угол наклона камеры (вверх-вниз)
    
    private Camera cameraComponent;                                          // Ссылка на камеру

    [SerializeField] private float collisionOffset = 0.5f;                   // Смещение камеры, чтобы избежать столкновений с объектами
    [SerializeField] private LayerMask collisionLayerMask;                   // Слой, с которым камера может столкнуться

    private void Start()
    {
        if (user == null)
        {
            Debug.LogError("User transform not assigned in CameraFollow script.");
            return;
        }

        cameraComponent = GetComponent<Camera>();
    }

    /// <summary>
    /// Вращение мыши
    /// </summary>
    private void LateUpdate()
    {
        if (user == null)
            return;

        // Вращение камеры с помощью мыши
        float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed;
        float verticalInput = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Обновление углов вращения
        _currentYRotation += horizontalInput;
        _currentXRotation -= verticalInput;

        // Ограничиваем вертикальный угол вращения
        _currentXRotation = Mathf.Clamp(_currentXRotation, minXRotation, maxXRotation);

        // Поворот камеры вокруг игрока
        Quaternion rotation = Quaternion.Euler(_currentXRotation, _currentYRotation, 0);

        // Преобразуем смещение камеры относительно игрока с учетом вращения
        Vector3 desiredPosition = user.position + rotation * offset;

        // Проверка на столкновение с препятствиями
        RaycastHit hit;
        if (Physics.Raycast(user.position, desiredPosition - user.position, out hit, offset.magnitude, collisionLayerMask))
        {
            // Если есть столкновение, перемещаем камеру немного ближе
            desiredPosition = hit.point + hit.normal * collisionOffset;
        }

        // Плавно двигаем камеру к новой позиции
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Смотрим на игрока
        transform.LookAt(user);
    }
}
