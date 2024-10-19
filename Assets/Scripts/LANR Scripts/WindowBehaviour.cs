using UnityEngine;

public class WindowBehaviour : MonoBehaviour
{
    float offsetX;
    float offsetY;
    private Camera mainCamera;

    public void Awake()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    public void BeginDrag()
    {
        var pos = mainCamera.WorldToScreenPoint(transform.position);
        offsetX = pos.x - Input.mousePosition.x;
        offsetY = pos.y - Input.mousePosition.y;
    }

    public void OnDrag()
    {
        var mousePos = new Vector3(offsetX + Input.mousePosition.x, offsetY + Input.mousePosition.y, 8);
        transform.position = mainCamera.ScreenToWorldPoint(mousePos);
    }

    public void CloseWindow()
    {
        Destroy(this.gameObject);
    }
}
