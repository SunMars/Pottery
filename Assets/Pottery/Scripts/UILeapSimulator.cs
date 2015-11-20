using UnityEngine;
using System.Collections;

public class UILeapSimulator : MonoBehaviour
{
    GameObject gameObjectToMove;

    void Update()
    {

    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, transform.position);
            Debug.DrawLine(Vector2.zero, pos, Color.cyan);
            if (hit.collider != null)
            {
                gameObjectToMove = hit.collider.gameObject;
            }
        }

        if (Input.GetMouseButtonDown(0) && gameObjectToMove != null)
        {
            //Vector3 mousePositions = new Vector3(gameObjectToMove.transform.position.x + Input.mousePosition.x / sensitivity, gameObjectToMove.transform.position.y + Input.mousePosition.y / sensitivity, gameObjectToMove.transform.position.z);
            //gameObjectToMove.transform.position = Vector3.Lerp(gameObjectToMove.transform.position, pos, Time.smoothDeltaTime / sensitivity);
        }
    }
}
