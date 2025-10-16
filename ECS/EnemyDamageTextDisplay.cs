using UnityEngine;
using TMPro;
using System.Collections;

public class EnemyDamageTextDisplay : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab; 
    [SerializeField] private Transform textHolder; 
    [SerializeField] private Vector3 textOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 textRandomOffset = new Vector3(0.5f, 0.5f, 0.5f);
    
    private Pool damageTextPool;
    private Camera mainCamera;

    public static EnemyDamageTextDisplay enemyDamageTextInstance;
    
    void Awake()
    {
        mainCamera = Camera.main;
        enemyDamageTextInstance = this;
        if (textHolder == null)
        {
            textHolder = new GameObject("DamageTextHolder").transform;
        }
        
        damageTextPool = new Pool(damageTextPrefab, 20, textHolder);
    }
    
    public void ShowDamage(float damage, Vector3 position, bool isHeadshot)
    {
        //Check if player wants to see damage numbers
        if (MenuManager.showEnemyDamageTaken == false) return;
        
        GameObject textObj = damageTextPool.Get();
        
        //Random offset for visual variety
        Vector3 randomOffset = new Vector3(
            Random.Range(-textRandomOffset.x, textRandomOffset.x),
            Random.Range(-textRandomOffset.y, textRandomOffset.y),
            Random.Range(-textRandomOffset.z, textRandomOffset.z)
        );
        
        textObj.transform.position = position + textOffset + randomOffset;
        textObj.transform.SetParent(textHolder);
        
        if (mainCamera != null)
        {
            textObj.transform.rotation = Quaternion.LookRotation(textObj.transform.position - mainCamera.transform.position);
        }
        
        TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damage).ToString();
            
            if (isHeadshot)
            {
                textMesh.color = Color.red;
                textMesh.text = damage.ToString() + "!"; 
            }
            else
            {
                textMesh.color = Color.red;
            }
        }
        
        StartCoroutine(ReturnTextToPool(textObj, 1.5f));
    }
    
    private IEnumerator ReturnTextToPool(GameObject textObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        textObj.SetActive(false);
    }
}