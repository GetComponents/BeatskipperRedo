using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Healthbarscript that I wrote that is supposed to be integratable into other projects
/// </summary>
[System.Serializable, RequireComponent(typeof(HealthCanvas))]
public class Healthbar : MonoBehaviour
{
    [SerializeField]
    GameObject Unit;
    public IHealth UnitScript;
    private float maxHealth => UnitScript.MaxHealth;
    private float currentHealth => UnitScript.CurrentHealth;
    [SerializeField]
    Slider healthSlider;
    private HealthCanvas healthCanvas;

    [SerializeField]
    bool zLock;

    private void Awake()
    {
        Unit.TryGetComponent<IHealth>(out IHealth tmp);
        UnitScript = tmp;
        healthCanvas = GetComponent<HealthCanvas>();
    }

    private void Start()
    {
        if (UnitScript != null)
        {
            UnitScript.OnHealthChange.AddListener(ChangeHealthSlider);
            ChangeHealthSlider();
        }
        else
        {
            Debug.LogWarning($"{Unit.name} (which has the Healthbar.cs) does not have IDamagable");
        }
    }

    private void Update()
    {
        if (healthCanvas.LookTowardsCamera && healthCanvas.healthbarType == EHealthbarType.OnEnemy)
        {
            healthCanvas.healthCanvas.transform.LookAt(Camera.main.transform.position);
            if (zLock)
                healthCanvas.healthCanvas.transform.eulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, 0);
        }
    }

    public void ChangeHealthSlider()
    {
        healthSlider.value = currentHealth / maxHealth;
    }
}

/// <summary>
/// Interface for Units that need healthbar
/// </summary>
public interface IHealth
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    UnityEvent OnHealthChange { get; set; }
}

