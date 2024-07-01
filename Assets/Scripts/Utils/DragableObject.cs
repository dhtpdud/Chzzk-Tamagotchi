using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragableObject : MonoBehaviour
{
    private bool isSelected;
    private Vector2 onMouseDownPosition;
    public bool isLockX;
    public bool isLockY;
    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            onMouseDownPosition = transform.position;
            isSelected = true;
            GameManager.instance.dragingObject = gameObject;
        }
    }
    private void OnMouseDrag()
    {
        if (isSelected && !EventSystem.current.IsPointerOverGameObject())
        {
            var targetPosition = onMouseDownPosition + (GameManager.instance.onMouseDragPosition - GameManager.instance.onMouseDownPosition);
            transform.position = new Vector3(isLockX ? transform.position.x : targetPosition.x, isLockY ? transform.position.y : targetPosition.y, transform.position.z);
        }
    }
    private void OnMouseUp()
    {
        onMouseDownPosition = Vector2.zero;
        isSelected = false;
        GameManager.instance.dragingObject = null;
    }
}
