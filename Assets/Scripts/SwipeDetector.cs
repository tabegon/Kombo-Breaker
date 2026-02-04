using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float minSwipeDistance = 50f;
    private float maxTapDuration = 0.2f;
    private float touchStartTime;
    private bool isTouching = false;

    void Update()
    {
        DetectTouch();
    }

    void DetectTouch()
    {
        // Nouveau système - Tactile
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            
            if (touch.press.wasPressedThisFrame)
            {
                startTouchPosition = touch.position.ReadValue();
                touchStartTime = Time.time;
                isTouching = true;
            }

            if (touch.press.wasReleasedThisFrame && isTouching)
            {
                endTouchPosition = touch.position.ReadValue();
                float touchDuration = Time.time - touchStartTime;
                DetectSwipe(touchDuration);
                isTouching = false;
            }
        }
        
        // Nouveau système - Souris (pour test sur PC)
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                startTouchPosition = Mouse.current.position.ReadValue();
                touchStartTime = Time.time;
                isTouching = true;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && isTouching)
            {
                endTouchPosition = Mouse.current.position.ReadValue();
                float touchDuration = Time.time - touchStartTime;
                DetectSwipe(touchDuration);
                isTouching = false;
            }
        }
    }

    void DetectSwipe(float duration)
    {
        float distance = Vector2.Distance(startTouchPosition, endTouchPosition);

        // Si c'est un tap rapide
        if (distance < minSwipeDistance && duration < maxTapDuration)
        {
            if (GameManager3D.Instance != null)
            {
                GameManager3D.Instance.OnTap();
            }
            return;
        }

        // Si c'est un swipe
        if (distance >= minSwipeDistance)
        {
            Vector2 direction = endTouchPosition - startTouchPosition;
            direction.Normalize();

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Swipe horizontal
                if (direction.x > 0)
                {
                    if (GameManager3D.Instance != null)
                    {
                        GameManager3D.Instance.OnSwipe(SwipeDirection.Right);
                    }
                }
                else
                {
                    if (GameManager3D.Instance != null)
                    {
                        GameManager3D.Instance.OnSwipe(SwipeDirection.Left);
                    }
                }
            }
            else
            {
                // Swipe vertical
                if (direction.y > 0)
                {
                    if (GameManager3D.Instance != null)
                    {
                        GameManager3D.Instance.OnSwipe(SwipeDirection.Up);
                    }
                }
                else
                {
                    if (GameManager3D.Instance != null)
                    {
                        GameManager3D.Instance.OnSwipe(SwipeDirection.Down);
                    }
                }
            }
        }
    }
}
