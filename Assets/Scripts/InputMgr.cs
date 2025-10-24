using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMgr : Singleton<InputMgr>
{
    public PlayerInput PlayerInputInstance;
    public static InputActionAsset Actions
    {
        get
        {

#if UNITY_EDITOR
            if (Application.isPlaying)
                return editorActions;
#endif
            return Instance.PlayerInputInstance.actions;
        }
    }

#if UNITY_EDITOR
    static InputActionAsset editorActions
    {
        get
        {
            _editorActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetDatabase.GUIDToAssetPath("ca9f5fa95ffab41fb9a615ab714db018"));
            return _editorActions;
        }
    }
    static InputActionAsset _editorActions;
#endif
}
