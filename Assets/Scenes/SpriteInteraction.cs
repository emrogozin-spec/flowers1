using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Transform> objectsToMove = new List<Transform>(); // Список объектов для перемещения
    [SerializeField] private Transform arrowSprite; // Спрайт стрелки
    
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float animationDuration = 1.5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private int arrowRotations = 2; // Количество полных оборотов
    [SerializeField] private float initialArrowRotation = -90f; // Начальный поворот стрелки

    private bool isAnimating = false;
    private bool isExtended = false;
    
    private Vector3 originalPosition;
    private List<Vector3> originalPositions = new List<Vector3>();
    private Quaternion arrowOriginalRotation;
    private Vector3 arrowOriginalPosition;

    private void Start()
    {
        originalPosition = transform.position;
        
        // Сохраняем оригинальные позиции всех объектов
        originalPositions.Clear();
        foreach (Transform obj in objectsToMove)
        {
            if (obj != null)
                originalPositions.Add(obj.position);
            else
                originalPositions.Add(Vector3.zero);
        }
        
        if (arrowSprite != null)
        {
            arrowOriginalPosition = arrowSprite.position;
            arrowSprite.rotation = Quaternion.Euler(0, 0, initialArrowRotation);
            arrowOriginalRotation = arrowSprite.rotation;
        }
    }

    private void OnMouseDown()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateSprites());
        }
    }

    private IEnumerator AnimateSprites()
    {
        isAnimating = true;
        
        // Определяем направление анимации
        float direction = isExtended ? -1f : 1f;
        
        // Начальные и конечные значения для текущей анимации
        Vector3 startPos = isExtended ? originalPosition + Vector3.right * moveDistance : originalPosition;
        Vector3 endPos = isExtended ? originalPosition : originalPosition + Vector3.right * moveDistance;
        
        // Для вращения стрелки
        float startRotation = isExtended ? initialArrowRotation + 180f : initialArrowRotation;
        float endRotation = isExtended ? initialArrowRotation : initialArrowRotation + 180f;
        float rotationDirection = isExtended ? -1f : 1f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            // Квадратичная easing-функция для более плавного старта и остановки
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            
            // Перемещение основного спрайта
            transform.position = Vector3.Lerp(startPos, endPos, easedT);
            
            // Перемещение всех объектов из списка
            for (int i = 0; i < objectsToMove.Count; i++)
            {
                if (objectsToMove[i] != null)
                {
                    Vector3 objStartPos = isExtended ? originalPositions[i] + Vector3.right * moveDistance : originalPositions[i];
                    Vector3 objEndPos = isExtended ? originalPositions[i] : originalPositions[i] + Vector3.right * moveDistance;
                    objectsToMove[i].position = Vector3.Lerp(objStartPos, objEndPos, easedT);
                }
            }
            
            // Вращение стрелки
            if (arrowSprite != null)
            {
                // Основной поворот + дополнительные обороты с easing
                float rotationProgress = 360f * arrowRotations * (easedT * rotationDirection);
                float currentRotation = Mathf.Lerp(startRotation, endRotation, easedT) + rotationProgress;
                arrowSprite.rotation = Quaternion.Euler(0, 0, currentRotation);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Финализация позиций и поворота
        transform.position = endPos;
        for (int i = 0; i < objectsToMove.Count; i++)
        {
            if (objectsToMove[i] != null)
            {
                objectsToMove[i].position = isExtended ? originalPositions[i] : originalPositions[i] + Vector3.right * moveDistance;
            }
        }
        
        if (arrowSprite != null)
        {
            arrowSprite.rotation = Quaternion.Euler(0, 0, endRotation);
        }
        
        isExtended = !isExtended;
        isAnimating = false;
    }

    public void ResetAll()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        for (int i = 0; i < objectsToMove.Count; i++)
        {
            if (objectsToMove[i] != null)
                objectsToMove[i].position = originalPositions[i];
        }
        if (arrowSprite != null)
        {
            arrowSprite.position = arrowOriginalPosition;
            arrowSprite.rotation = Quaternion.Euler(0, 0, initialArrowRotation);
        }
        isExtended = false;
        isAnimating = false;
    }
}