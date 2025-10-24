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
            _editorActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetDatabase.GUIDToAssetPath("e5891171a06138540a63d3caf1759315"));
            return _editorActions;
        }
    }
    static InputActionAsset _editorActions;
#endif
}
