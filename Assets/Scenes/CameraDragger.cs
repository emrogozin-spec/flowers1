using UnityEngine;
using System.Collections;

public class CameraDragger : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 2f;
    [SerializeField] private float inertiaDuration = 0.5f;
    [SerializeField] private float softEdgeWidth = 2f;
    [SerializeField] private float edgeReturnSpeed = 3f;

    [Header("References")]
    [SerializeField] private Transform background;

    private Camera mainCamera;
    private Vector3 dragOrigin;
    private Vector3 velocity;
    private float inertiaTime;
    private Bounds backgroundBounds;
    private Bounds cameraBounds;
    private Bounds hardBounds;
    private Bounds softBounds;

    private bool isDragging;
    private bool isInSoftEdge;

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateBounds();
    }

    private void Update()
    {
        HandleInput();
        ApplyInertia();
        
        if (!isDragging && !isInSoftEdge)
        {
            ClampCameraToSoftBounds();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            inertiaTime = 0;
            velocity = Vector3.zero;
            isDragging = true;
            isInSoftEdge = false;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 currentPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // Инвертируем направление разницы
            Vector3 difference = currentPos - dragOrigin;
            
            velocity = difference * dragSpeed;
            background.position += velocity * Time.deltaTime;
            
            dragOrigin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            inertiaTime = inertiaDuration;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
            CheckSoftBounds();
        }
    }

    private void ApplyInertia()
    {
        if (inertiaTime > 0 && !Input.GetMouseButton(1))
        {
            background.position += velocity * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime);
            inertiaTime -= Time.deltaTime;
            
            if (inertiaTime <= 0)
            {
                CheckSoftBounds();
            }
        }
    }

    private void CheckSoftBounds()
    {
        Vector3 bgPos = background.position;
        isInSoftEdge = !softBounds.Contains(new Vector3(
            transform.position.x,
            transform.position.y,
            bgPos.z
        ));

        if (isInSoftEdge)
        {
            StartCoroutine(ReturnFromSoftEdge());
        }
    }

    private IEnumerator ReturnFromSoftEdge()
    {
        Vector3 targetPos = background.position;
        
        if (transform.position.x > softBounds.max.x)
            targetPos.x -= transform.position.x - softBounds.max.x;
        else if (transform.position.x < softBounds.min.x)
            targetPos.x += softBounds.min.x - transform.position.x;
            
        if (transform.position.y > softBounds.max.y)
            targetPos.y -= transform.position.y - softBounds.max.y;
        else if (transform.position.y < softBounds.min.y)
            targetPos.y += softBounds.min.y - transform.position.y;

        while (Vector3.Distance(background.position, targetPos) > 0.01f && !isDragging)
        {
            background.position = Vector3.Lerp(
                background.position, 
                targetPos, 
                edgeReturnSpeed * Time.deltaTime
            );
            yield return null;
        }

        isInSoftEdge = false;
    }

    private void ClampCameraToSoftBounds()
    {
        Vector3 bgPos = background.position;
        Vector3 newPos = bgPos;
        
        float cameraExtentX = cameraBounds.extents.x;
        float cameraExtentY = cameraBounds.extents.y;
        
        float overflowX = 0;
        if (transform.position.x > softBounds.max.x)
            overflowX = transform.position.x - softBounds.max.x;
        else if (transform.position.x < softBounds.min.x)
            overflowX = transform.position.x - softBounds.min.x;
            
        float overflowY = 0;
        if (transform.position.y > softBounds.max.y)
            overflowY = transform.position.y - softBounds.max.y;
        else if (transform.position.y < softBounds.min.y)
            overflowY = transform.position.y - softBounds.min.y;

        if (Mathf.Abs(overflowX) > 0)
        {
            newPos.x -= overflowX * edgeReturnSpeed * Time.deltaTime;
        }
        
        if (Mathf.Abs(overflowY) > 0)
        {
            newPos.y -= overflowY * edgeReturnSpeed * Time.deltaTime;
        }

        background.position = newPos;
    }

    private void CalculateBounds()
    {
        if (background != null)
        {
            SpriteRenderer bgRenderer = background.GetComponent<SpriteRenderer>();
            if (bgRenderer != null)
            {
                backgroundBounds = bgRenderer.bounds;
            }
            else
            {
                Collider2D col = background.GetComponent<Collider2D>();
                if (col != null) backgroundBounds = col.bounds;
            }
        }

        if (mainCamera != null)
        {
            float cameraHeight = mainCamera.orthographicSize * 2;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            cameraBounds = new Bounds(transform.position, new Vector3(cameraWidth, cameraHeight, 0));
        }

        hardBounds = new Bounds(
            backgroundBounds.center,
            backgroundBounds.size
        );

        softBounds = new Bounds(
            backgroundBounds.center,
            new Vector3(
                backgroundBounds.size.x - softEdgeWidth * 2,
                backgroundBounds.size.y - softEdgeWidth * 2,
                backgroundBounds.size.z
            )
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || mainCamera == null || background == null) return;
        
        CalculateBounds();
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hardBounds.center, hardBounds.size);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(softBounds.center, softBounds.size);
    }
}