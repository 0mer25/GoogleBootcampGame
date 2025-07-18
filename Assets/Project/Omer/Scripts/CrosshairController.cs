using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRect;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        crosshairRect.position = mousePos;
    }
}
