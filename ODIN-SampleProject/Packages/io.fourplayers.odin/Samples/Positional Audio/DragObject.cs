using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
    public class DragObject : MonoBehaviour
    {
        private Vector3 MoveOffset;
        private float MouseZCoord;

        private void OnMouseDown()
        {
            MouseZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            MoveOffset = gameObject.transform.position - GetMouseWorldPos();
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = MouseZCoord;

            return Camera.main.ScreenToWorldPoint(mousePoint);
        }

        private void OnMouseDrag()
        {
            gameObject.transform.position = MoveOffset + GetMouseWorldPos();
        }
    }
}
