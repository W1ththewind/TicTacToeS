using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransparentOnDisableGroup : UIBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.2f, endAlpha = 1;
    public IEnumerator fadeCo;
    public LayoutElement element;
    public UnityAction ShowBeginEvent, ShowEndEvent, HideBeginEvent, HideEndEvent;
    public bool MessageChildrenOnCanvasGroupChanged;
    protected bool canvasGroupInteractable;

    public bool interactable
    {
        get => canvasGroup.interactable;
        set
        {
            canvasGroup.interactable = value;
        }
    }

    public bool blocksRaycasts
    {
        get => canvasGroup.blocksRaycasts;
        set => canvasGroup.blocksRaycasts = value;
    }

    public bool InteractableInHierarchy => canvasGroup.ignoreParentGroups ? canvasGroup.interactable : !this.GetComponentsInParent<CanvasGroup>(true).Any(c => !c.interactable);
    public bool ActiveAndInteractableInHierarchy => canvasGroup.ignoreParentGroups ? canvasGroup.interactable : !this.GetComponentsInParent<CanvasGroup>(true).Any(c => !c.isActiveAndEnabled || !c.interactable);

    protected override void OnEnable()
    {
        base.OnEnable();

        if (!this.gameObject.activeInHierarchy)
            return;

        switch (InteractableInHierarchy)
        {
            case true:
                if (fadeCo != null)
                    StopCoroutine(fadeCo);
                fadeCo = AppearCoroutine();
                StartCoroutine(fadeCo);
                break;
        }
    }

    protected override void OnCanvasGroupChanged()
    {
        bool thisInteractable = ActiveAndInteractableInHierarchy;
        if (canvasGroupInteractable != thisInteractable)
        {
            if (this.gameObject.activeInHierarchy)
            {
                switch (thisInteractable)
                {
                    case true:
                        if (fadeCo != null)
                            StopCoroutine(fadeCo);
                        fadeCo = AppearCoroutine();
                        StartCoroutine(fadeCo);
                        break;

                    case false:
                        if (fadeCo != null)
                            StopCoroutine(fadeCo);
                        fadeCo = FadeCoroutine();
                        StartCoroutine(fadeCo);
                        break;
                }
            }
            canvasGroupInteractable = thisInteractable;
            if (MessageChildrenOnCanvasGroupChanged)
            {
                BroadcastMessage("OnCanvasGroupChanged", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void Show(UnityAction endAction = null)
    {
        if (fadeCo != null)
            StopCoroutine(fadeCo);
        if (this.isActiveAndEnabled)
        {
            fadeCo = AppearCoroutine(endAction);
            StartCoroutine(fadeCo);
        }
        else
        {
            canvasGroup.alpha = endAlpha;
            if (element != null)
                element.ignoreLayout = false;
        }
    }

    public void Hide(UnityAction endAction = null)
    {
        if (fadeCo != null)
            StopCoroutine(fadeCo);
        if (this.isActiveAndEnabled)
        {
            fadeCo = FadeCoroutine(endAction);
            StartCoroutine(fadeCo);
        }
        else
        {
            canvasGroup.alpha = 0;
            if (element != null)
                element.ignoreLayout = true;
        }
    }

    IEnumerator AppearCoroutine(UnityAction endAction = null)
    {
        //if (this.name == "IdeaCanvas")
        //    Debug.Log($"{this.name} AppearCoroutine ShowBeginEvent: {(ShowBeginEvent != null ? "Active" : "NULL")}", this);
        ShowBeginEvent?.Invoke();
        if (element != null)
            element.ignoreLayout = false;
        if (canvasGroup.alpha < 0.5f)
        {
            for (float i = 0; i < fadeDuration; i += Time.deltaTime)
            {
                canvasGroup.alpha = endAlpha * i / fadeDuration;
                yield return null;
            }
        }
        canvasGroup.alpha = endAlpha;
        endAction?.Invoke();
        ShowEndEvent?.Invoke();
        //Debug.Log($"{this.name} ShowEndEvent", this);
    }

    IEnumerator FadeCoroutine(UnityAction endAction = null)
    {
        //if (this.name == "IdeaCanvas")
        //    Debug.Log($"{this.name} FadeCoroutine HideEndEvent: {(HideEndEvent != null ? "Active" : "NULL")}", this);
        HideBeginEvent?.Invoke();
        if (element != null)
            element.ignoreLayout = true;
        if (canvasGroup.alpha > 0.5f)
        {
            for (float i = 0; i < fadeDuration; i += Time.deltaTime)
            {
                canvasGroup.alpha = endAlpha * (1 - i / fadeDuration);
                yield return null;
            }
        }
        canvasGroup.alpha = 0;
        endAction?.Invoke();
        HideEndEvent?.Invoke();
        //if (this.name == "Blur")
        //    Debug.Log($"{this.name} HideEndEvent: {(HideEndEvent != null ? "Active" : "NULL")}", this);
    }

    #region Only for UnityEvent
    /// <summary>
    /// Only for UnityEvent.
    /// </summary>
    /// <param name="interactBlcok"></param>
    public void SetCanvasGroup(bool interactBlcok)
    {
        canvasGroup.interactable = canvasGroup.blocksRaycasts = interactBlcok;
    }
    #endregion

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        canvasGroup = GetComponent<CanvasGroup>();
        element = GetComponent<LayoutElement>();
    }
#endif
}
