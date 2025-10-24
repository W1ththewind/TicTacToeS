using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIMgr : Singleton<GUIMgr>
{
    List<CanvasGroup> _stackedCanvasGroup;

    protected override void Awake()
    {
        base.Awake();
        if (!Destroyed)
        {
            _stackedCanvasGroup = new();

        }
    }


}
