using UnityEngine;

public class WeaponAttachment : MonoBehaviour
{
    [SerializeField] public GameObject katanaPrefab;  // Префаб катаны
    [SerializeField] public GameObject sheath;        // Ножны
    [SerializeField] private Transform rightHand;     // Ссылка на правую руку
    [SerializeField] private Transform spine;         // Ссылка на спину персонажа

    private GameObject katanaInstance;                // Экземпляр катаны
    private bool isKatanaDrawn = false;               // Флаг, показывающий, достали ли катану

    void Start()
    {
        if (rightHand == null)
        {
            Debug.LogError("Right hand bone not found!");
            return;
        }

        if (spine == null)
        {
            Debug.LogError("Spine bone not found!");
            return;
        }

        // Привязываем ножны к спине
        if (sheath != null)
        {
            sheath.transform.SetParent(spine);
            sheath.transform.localPosition = Vector3.zero;  // Позиционируем ножны на спине
            sheath.transform.localRotation = Quaternion.identity;  // Без вращения относительно спины
        }

        // Если катана уже достана, создаем экземпляр и привязываем к правой руке
        if (katanaPrefab != null && isKatanaDrawn)
        {
            katanaInstance = Instantiate(katanaPrefab);
            katanaInstance.transform.SetParent(rightHand);
            katanaInstance.transform.localPosition = Vector3.zero;  // Центрируем катану относительно руки
            katanaInstance.transform.localRotation = Quaternion.identity;  // Без вращения относительно руки
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleKatana();
        }
    }

    /// <summary>
    /// Функция для смены состояния катаны (достать/убрать)
    /// </summary>
    void ToggleKatana()
    {
        isKatanaDrawn = !isKatanaDrawn;

        if (isKatanaDrawn)
        {
            // Достаем катану и прикрепляем её к руке
            if (katanaPrefab != null && rightHand != null)
            {
                if (katanaInstance == null)
                {
                    katanaInstance = Instantiate(katanaPrefab);  // Создаем экземпляр катаны
                }
                katanaInstance.transform.SetParent(rightHand);
                katanaInstance.transform.localPosition = Vector3.zero;  // Центрируем катану относительно руки
                katanaInstance.transform.localRotation = Quaternion.identity;  // Без вращения относительно руки
                katanaInstance.SetActive(true);  // Активируем катану, если она была скрыта
            }
        }
        else
        {
            // Убираем катану (отключаем привязку)
            if (katanaInstance != null)
            {
                katanaInstance.transform.SetParent(null);  // Убираем привязку к руке
                katanaInstance.SetActive(false);  // Скрываем катану, если она не используется
            }
            
        }
    }
}
