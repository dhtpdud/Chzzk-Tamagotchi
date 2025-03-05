using UnityEngine;

public class ScreenColliderManager : MonoBehaviour
{
    public GameObject top;
    public GameObject bottom;
    public GameObject left;
    public GameObject right;

    void Start()
    {
        CreateScreenColliders();
    }


    void CreateScreenColliders()
    {
        float scaleFactor = GameManager.instance.rootCanvas.transform.localScale.x;
        //Vector3 bottomLeftScreenPoint   = - new Vector3(Screen.width, Screen.height, 0f) / 2 * scaleFactor;
        Vector3 topRightScreenPoint = new Vector3(Screen.width, Screen.height, 0f) / 2 * scaleFactor;

        //// Create top collider
        BoxCollider collider = top.GetComponent<BoxCollider>();
        collider.size = new Vector3(Screen.width * scaleFactor + 1, 1f, 1f);

        top.transform.position = new Vector3(0, topRightScreenPoint.y + (collider.size.y / 2), 0f);

        // Create bottom collider
        collider = bottom.GetComponent<BoxCollider>();
        collider.size = new Vector3(Screen.width * scaleFactor + 1, 1f, 1f);

        //** Bottom needs to account for collider size
        bottom.transform.position = new Vector3(0, -topRightScreenPoint.y, 0f);


        // Create left collider
        collider = left.GetComponent<BoxCollider>();
        collider.size = new Vector3(1f, Screen.height * scaleFactor + 1, 1f);

        //** Left needs to account for collider size
        left.transform.position = new Vector3(-topRightScreenPoint.x - (collider.size.x / 2), 0, 0f);


        // Create right collider
        collider = right.GetComponent<BoxCollider>();
        collider.size = new Vector3(1f, Screen.height * scaleFactor + 1, 1f);

        right.transform.position = new Vector3(topRightScreenPoint.x + (collider.size.x / 2), 0, 0f);
    }
}