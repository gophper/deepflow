using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MobaGame.UI
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Joystick Components")]
        public RectTransform background;
        public RectTransform handle;

        [Header("Settings")]
        public float handleRange = 50f;

        private Vector2 inputDirection = Vector2.zero;
        private bool isDragging = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            inputDirection = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 position = Vector2.zero;
            
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out position))
            {
                // Normalize position
                position.x = (position.x / background.sizeDelta.x);
                position.y = (position.y / background.sizeDelta.y);

                // Calculate input direction
                inputDirection = new Vector2(position.x * 2, position.y * 2);
                inputDirection = (inputDirection.magnitude > 1.0f) ? inputDirection.normalized : inputDirection;

                // Move handle
                handle.anchoredPosition = new Vector2(
                    inputDirection.x * handleRange,
                    inputDirection.y * handleRange);
            }
        }

        public Vector2 GetInputDirection()
        {
            return inputDirection;
        }
    }
}
