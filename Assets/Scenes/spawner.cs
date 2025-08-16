using UnityEngine;
using System.Collections.Generic;

public class spawner : MonoBehaviour  // Имя класса с маленькой буквы
{
    [System.Serializable]
    public class CollectableItem
    {
        public GameObject prefab;
        public string itemName;
    }

    [Header("Spawn Settings")]
    public List<CollectableItem> itemsToSpawn = new List<CollectableItem>();
    public Transform spawnParent;
    public Vector3 firstSpawnPos = new Vector3(0, 4, 0);
    public Vector3 spawnOffset = new Vector3(0, -1.2f, 0);
    public int maxSpawned = 5;  // Максимум 5 предметов

    [Header("Appearance")]
    public float fadeStep = 0.1f;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Vector3 nextSpawnPos;
    private Dictionary<GameObject, CollectableItem> itemMap = new Dictionary<GameObject, CollectableItem>();

    private void Start()
    {
        nextSpawnPos = firstSpawnPos;
        SetupItemClickHandlers();
    }

    private void SetupItemClickHandlers()
    {
        foreach (CollectableItem item in itemsToSpawn)
        {
            if (item.prefab != null)
            {
                var clicker = item.prefab.AddComponent<ItemClickDetector>();
                clicker.Setup(this, item);
                itemMap.Add(item.prefab, item);
            }
        }
    }

    public void SpawnItem(CollectableItem item)
    {
        if (spawnedObjects.Count >= maxSpawned)
        {
            RemoveFirstItem();
        }

        GameObject newObj = Instantiate(item.prefab, spawnParent);
        newObj.transform.localPosition = nextSpawnPos;

        float alpha = 1f - (spawnedObjects.Count * fadeStep);
        SetObjectAlpha(newObj, Mathf.Clamp(alpha, 0.3f, 1f));

        spawnedObjects.Add(newObj);
        newObj.name = $"{item.itemName}_{spawnedObjects.Count}";

        nextSpawnPos += spawnOffset;
    }

    private void RemoveFirstItem()
    {
        if (spawnedObjects.Count == 0) return;

        GameObject oldest = spawnedObjects[0];
        spawnedObjects.RemoveAt(0);
        Destroy(oldest);

        ShiftItemsUp();
    }

    private void ShiftItemsUp()
    {
        nextSpawnPos -= spawnOffset;
        
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            spawnedObjects[i].transform.localPosition += -spawnOffset;
            float alpha = 1f - i * fadeStep;
            SetObjectAlpha(spawnedObjects[i], Mathf.Clamp(alpha, 0.3f, 1f));
            
            CollectableItem item = itemMap[spawnedObjects[i]];
            spawnedObjects[i].name = $"{item.itemName}_{i + 1}";
        }
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
        
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
        }
    }

    public void ClearAll()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
        nextSpawnPos = firstSpawnPos;
    }
}

public class ItemClickDetector : MonoBehaviour
{
    private spawner itemSpawner;
    private spawner.CollectableItem myItem;

    public void Setup(spawner spawner, spawner.CollectableItem item)
    {
        this.itemSpawner = spawner;
        this.myItem = item;
    }

    private void OnMouseDown()
    {
        if (itemSpawner != null)
        {
            itemSpawner.SpawnItem(myItem);
        }
    }
}