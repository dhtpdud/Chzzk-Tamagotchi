using UnityEngine;

public class DragableObject : MonoBehaviour
{
    [SerializeField]
    private bool isSelected;
    private Vector2 onMouseDownPosition;
    public bool isLockX;
    public bool isLockY;
    public void OnMouseDown()
    {
        onMouseDownPosition = transform.position;
        isSelected = true;
        GameManager.instance.dragingObject = gameObject;
    }
    public void OnMouseDrag()
    {
        if (isSelected)
        {
            var targetPosition = onMouseDownPosition + (GameManager.instance.onMouseDragPosition - GameManager.instance.onMouseDownPosition);
            transform.position = new Vector3(isLockX ? transform.position.x : targetPosition.x, isLockY ? transform.position.y : targetPosition.y, transform.position.z);
        }
    }
    public void OnMouseUp()
    {
        onMouseDownPosition = Vector2.zero;
        isSelected = false;
        GameManager.instance.dragingObject = null;
    }
}
