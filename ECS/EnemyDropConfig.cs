using UnityEngine;

public class EnemyDropConfig : MonoBehaviour
{
    public static EnemyDropConfig Instance { get; private set; }

    [Header("Health Drop")]
    [Tooltip("The health effect data to spawn when enemies drop health")]
    public EffectDataSO healthEffectData;

    [Header("Gold Drop")]
    public EffectDataSO goldEffectData;
    
    [Header("Drop Chances")]
    [Tooltip("Base chance for health drop (before luck modifier)")]
    [Range(0f, 100f)]
    public float baseHealthDropChance = 5f;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}