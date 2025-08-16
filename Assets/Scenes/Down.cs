using UnityEngine;
using System.Collections.Generic; // Добавляем для работы с List<>

public class Down : MonoBehaviour
{
    [Header("Button Settings")]
    public GameObject normalButton;
    public GameObject pressedButton;

    [Header("Platform Settings")]
    public float dropDistance = 10f;
    public List<Transform> platformsToMove = new List<Transform>();

    private List<Vector3> platformStartPositions = new List<Vector3>();
    private bool isLowered = false;

    private void Start()
    {
        // Сохраняем начальные позиции
        SaveStartPositions();
        
        // Инициализация кнопок
        normalButton.SetActive(true);
        pressedButton.SetActive(false);
    }

    private void SaveStartPositions()
    {
        platformStartPositions.Clear();
        foreach (Transform platform in platformsToMove)
        {
            if (platform != null)
            {
                platformStartPositions.Add(platform.position);
            }
        }
    }

    private void OnMouseDown()
    {
        TogglePlatforms();
    }

    private void TogglePlatforms()
    {
        // Переключение кнопок
        normalButton.SetActive(isLowered);
        pressedButton.SetActive(!isLowered);
        
        // Перемещение платформ
        for (int i = 0; i < platformsToMove.Count; i++)
        {
            if (platformsToMove[i] != null)
            {
                platformsToMove[i].position = isLowered ? 
                    platformStartPositions[i] : 
                    platformStartPositions[i] + Vector3.down * dropDistance;
            }
        }
        
        isLowered = !isLowered;
    }

    public void AddPlatform(Transform newPlatform)
    {
        if (!platformsToMove.Contains(newPlatform))
        {
            platformsToMove.Add(newPlatform);
            platformStartPositions.Add(newPlatform.position);
        }
    }

    public void RemovePlatform(Transform platformToRemove)
    {
        int index = platformsToMove.IndexOf(platformToRemove);
        if (index >= 0)
        {
            platformsToMove.RemoveAt(index);
            platformStartPositions.RemoveAt(index);
        }
    }
}