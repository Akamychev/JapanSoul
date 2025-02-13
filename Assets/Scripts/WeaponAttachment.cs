using UnityEngine;

public class WeaponAttachment : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject katanaPrefab;        // Префаб катаны
    [SerializeField] private GameObject katanaSheathed;      // Ссылка на катану в ножнах
    
    [Header("Audio Parameters")]
    [SerializeField] private AudioSource audioSource;        // Источник звука
    [SerializeField] private AudioClip katanaDrawSound;      // Звук катаны при достании
    [SerializeField] private AudioClip katanaSheathSound;    // Звук катаны при убирании

    private Animator _animator;                              // Экземлпяр аниматора
    private bool _isKatanaDrawn = false;                     // Флаг, показывающий, достали ли катану

    private void Start()
    {
        _animator = GetComponent<Animator>();
        
        if (katanaSheathed != null)
            katanaSheathed.SetActive(true);

        if (katanaPrefab != null)
            katanaPrefab.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            ToggleKatana();
    }

    /// <summary>
    /// Функция для смены состояния катаны (достать/убрать)
    /// </summary>
    private void ToggleKatana()
    {
        _isKatanaDrawn = !_isKatanaDrawn;

        _animator.SetTrigger(_isKatanaDrawn ? "DrawWeapon" : "SheatheWeapon");
    }
    
    public void OnWeaponDrawnComplete()
    {
        if (katanaPrefab != null)
            katanaPrefab.SetActive(true); 

        if (katanaSheathed != null)
            katanaSheathed.SetActive(false);
        
        audioSource.PlayOneShot(katanaDrawSound);
    }
    
    public void OnWeaponSheathedComplete()
    {
        if (katanaPrefab != null)
            katanaPrefab.SetActive(false);

        if (katanaSheathed != null)
            katanaSheathed.SetActive(true);
        
        audioSource.PlayOneShot(katanaSheathSound);
    }
}
