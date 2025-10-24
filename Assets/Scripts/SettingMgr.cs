using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingMgr : Singleton<SettingMgr>
{
    [SerializeField]
    GameObject
    _togglePrefab,
    _dropDownPrefab;
    public const string LanguageKey = "GameLanguage", MuteKey = "Mute";
    public string LanguageCode
    {
        get
        {
            return PlayerPrefs.GetString(LanguageKey);
        }
        set { PlayerPrefs.SetString(LanguageKey, value); }
    }

    public bool Mute
    {
        get
        {
            return PlayerPrefs.GetInt(MuteKey) == 1 ? true : false;
        }
        set { PlayerPrefs.SetInt(MuteKey, value ? 1 : 0); }
    }
    List<string> LanguageList;


    [SerializeField]
    RectTransform _scrollContentRoot;
    TransparentOnDisableGroup settingGroup
    {
        get
        {
            if (_transparentOnDisableGroup == null)
                _transparentOnDisableGroup = GetComponent<TransparentOnDisableGroup>();
            return _transparentOnDisableGroup;
        }
    }
    TransparentOnDisableGroup _transparentOnDisableGroup;

    void Start()
    {
        InputMgr.Actions["UI/ESC"].Enable();
        InputMgr.Actions["UI/ESC"].performed += Show;

        if ((!PlayerPrefs.HasKey(LanguageKey)) ||
            (
                LanguageCode != "en"
                && LanguageCode != "cn"
            ))
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    LanguageCode = "cn";
                    break;
                case SystemLanguage.English:
                default:
                    LanguageCode = "en";
                    break;
            }
            GameMgr.Instance.ChangeLanguage(LanguageCode);
        }

        LanguageList = new() { "cn", "en" };

        AddToggleFunction("SkillMode", GameMgr.Instance.SetSkillMode, () => GameMgr.Instance.GetSkillMode());
        AddToggleFunction("Player1First", GameMgr.Instance.SetPlayerFirst, () => GameMgr.Instance.GetPlayer1First());
        AddToggleFunction("AIMode", GameMgr.Instance.SetAIMode, () => GameMgr.Instance.GetAIMode());
        AddToggleFunction("PlayerAsO", GameMgr.Instance.SetPlayerType, () => GameMgr.Instance.GetPlayer1TypeAsO());

        AddToggleFunction("Mute", SetMute, () => Mute);

        var languageEvent = new TMP_Dropdown.DropdownEvent();
        languageEvent.AddListener(ChangeLanguage);
        AddDropdownFunction("Language", new List<string>() { "简体中文", "English" }, languageEvent, LanguageList.IndexOf(LanguageCode));

        settingGroup.HideEndEvent += checkShouldRestart;
    }

    void SetMute(bool mute)
    {
        Mute = mute;
    }

    void checkShouldRestart()
    {
        GameMgr.Instance.CheckSettingChanged();
    }

    public void ChangeLanguage(int optionIndex)
    {
        GameMgr.Instance.ChangeLanguage(
            optionIndex switch
            {
                0 => "cn",
                1 => "en",
                _ => "en"
            }
        );
    }

    public void Show(InputAction.CallbackContext context)
    {
        settingGroup.canvasGroup.interactable = settingGroup.canvasGroup.blocksRaycasts = !settingGroup.canvasGroup.interactable;
    }

    public void Show()
    {
        settingGroup.canvasGroup.interactable = settingGroup.canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        settingGroup.canvasGroup.interactable = settingGroup.canvasGroup.blocksRaycasts = false;
    }

    public GameObject AddToggleFunction(string funcName, UnityAction<bool> toggleAction, Func<bool> DefaultOnFunc = null)
    {
        GameObject NewToggleFunc = Instantiate(_togglePrefab);
        NewToggleFunc.transform.SetParent(_scrollContentRoot, false);

        NewToggleFunc.GetComponentInChildren<TMP_Text>().text = funcName;// todo: Localize

        Toggle newToggle = NewToggleFunc.GetComponentInChildren<Toggle>();
        newToggle.onValueChanged.AddListener(toggleAction);
        if (DefaultOnFunc != null)
            settingGroup.ShowBeginEvent += () => newToggle.SetIsOnWithoutNotify(DefaultOnFunc.Invoke());

        return NewToggleFunc;
    }

    public GameObject AddDropdownFunction(string funcName, List<string> options, TMP_Dropdown.DropdownEvent onChangeAction, int defaultOptionIndex)
    {
        GameObject NewDropdownFunc = Instantiate(_dropDownPrefab);
        NewDropdownFunc.transform.SetParent(_scrollContentRoot, false);

        TMP_Dropdown newDropdown = NewDropdownFunc.GetComponentInChildren<TMP_Dropdown>();

        //Set FuncName

        List<TMP_Dropdown.OptionData> optionDataList = new();
        foreach (var option in options)
        {
            TMP_Dropdown.OptionData optionData = new()
            {
                text = option // todo: Localize
            };
            optionDataList.Add(optionData);
        }

        newDropdown.AddOptions(optionDataList);
        newDropdown.onValueChanged = onChangeAction;

        settingGroup.ShowBeginEvent += () => newDropdown.SetValueWithoutNotify(defaultOptionIndex);

        return NewDropdownFunc;
    }
}
