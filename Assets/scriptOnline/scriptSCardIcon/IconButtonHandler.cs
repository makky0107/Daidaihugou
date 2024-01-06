using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IconButtonHandler : MonoBehaviour
{
    private bool isPressed = false;
    private float pressTime = 0f;

    public void OnPointerDown()
    {
        isPressed = true;
        pressTime = Time.time;
    }

    public void OnPointerUp()
    {
        isPressed = false;
        if (Time.time - pressTime >= 1f)
        {
            LongPress();
        }
        else
        {
            ShortPress();
        }
    }

    private void ShortPress()
    {
        // ƒ^ƒbƒv‚Ìˆ—
    }

    private void LongPress()
    {
        // ’·‰Ÿ‚µ‚Ìˆ—
    }
}
