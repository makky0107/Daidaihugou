using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IconButtonHandler : MonoBehaviour
{
    private bool isPressed = false;
    private float pressTime = 0f;

    public SCardCallFromIcon SCallIcon;

    public void OnPointerDown()
    {
        isPressed = true;
        pressTime = Time.time;
    }

    public void OnPointerUp()
    {
        isPressed = false;
        if (Time.time - pressTime >= 0.3f)
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
        SCallIcon.SkillSelct();
    }

    private void LongPress()
    {
        SCallIcon.SkillActivation();
    }
}
