using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIconModel
{
    public int ID;
    public Sprite Icon;

    public SCIconModel(int iconID)
    {
        Debug.Log($"iconID {iconID}");
        SCardIconEntity iconEntity = Resources.Load<SCardIconEntity>("SCIcon/SCardIcon" + iconID);
        ID = iconEntity.ID;
        Icon = iconEntity.Icon;
        Debug.Log($"model Icon {Icon} iconEntity.Icon {iconEntity.Icon}");
    }
}
