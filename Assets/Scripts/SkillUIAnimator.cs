using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIAnimator : MonoBehaviour
{
    public float radius;
    public bool SkillMenuActive;
    public List<Element> elements;
    public Vector2 defaultPos, targetPosition;
    public float targetScale;
    public float positiveBuffer = 180f, negativeBuffer = -360f;

    [Header("Skill Menu")]
    public Transform SkillMenuTransform;
    public Vector2 skillMenuDefaultPos, skillMenuTargetPos = Vector2.zero;
    public float skillBtnAnimSpeed, elementAnimSpeed, skillMenuAnimSpeed;
    public TextMeshProUGUI skillMenuTitle;
    public Image skillMenuBG;
    private Button skillBtn;
    public Transform GLG;

    private void Awake()
    {
        skillBtn = GetComponent<Button>();
        Application.targetFrameRate = 100;
    }

    private void Start()
    {
        defaultPos = transform.localPosition;
        skillMenuDefaultPos = SkillMenuTransform.localPosition;
    }

    private void OnValidate()
    {
        elements = new();
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
            elements.Add(transform.GetChild(i).GetComponent<Element>());
    }

    public void SkillMenu()
    {
        SkillMenuActive = !SkillMenuActive;
        StartCoroutine(AnimateButton());
        StartCoroutine(AnimateElements());
    }

    private IEnumerator AnimateButton()
    {
        if (SkillMenuActive == false)
            yield return StartCoroutine(AnimateSkillMenu(SkillMenuActive));

        foreach (Element e in elements)
            e.button.interactable = SkillMenuActive;

        Vector2 startPos = transform.localPosition;
        Vector2 targetPos = SkillMenuActive ? targetPosition : defaultPos;
        float startScale = transform.localScale.x;
        float finalScale = SkillMenuActive ? targetScale : 1f;
        StartCoroutine(SkillMenuBGFade(SkillMenuActive));
        yield return StartCoroutine(UpdatePositionAndScale(transform, startPos, targetPos, skillBtnAnimSpeed, startScale, finalScale));

        if (SkillMenuActive)
            StartCoroutine(AnimateSkillMenu(SkillMenuActive));
    }

    private IEnumerator UpdatePositionAndScale(Transform objT, Vector2 startPos, Vector2 targetPos, float animSpeed, float startScale = 0f, float finalScale = 0f)
    {
        float timer = 0;
        float startTime = Time.unscaledTime;
        while (timer < animSpeed)
        {
            timer = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(timer / animSpeed);
            t = Mathf.SmoothStep(0, 1, t);
            float x = Mathf.Lerp(startPos.x, targetPos.x, t);
            float y = Mathf.Lerp(startPos.y, targetPos.y, t);
            objT.localPosition = new Vector2(x, y);

            if (startScale != finalScale)
            {
                float scale = Mathf.Lerp(startScale, finalScale, t);
                transform.localScale = new Vector3(scale, scale, scale);
            }

            yield return null;
        }
    }

    private IEnumerator SkillMenuBGFade(bool enabled)
    {
        if (enabled)
            skillMenuBG.gameObject.SetActive(true);
        Color currentColor = skillMenuBG.color;
        float startVal = currentColor.a;
        float targetVal = enabled ? 0.69f : 0f;

        float timer = 0;
        float startTime = Time.unscaledTime;
        while (timer < skillMenuAnimSpeed)
        {
            timer = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(timer / skillMenuAnimSpeed);
            t = Mathf.SmoothStep(0, 1, t);
            float val = Mathf.Lerp(startVal, targetVal, t);
            currentColor.a = val;
            skillMenuBG.color = currentColor;
            yield return null;
        }
        skillMenuBG.gameObject.SetActive(enabled);
    }

    private IEnumerator AnimateSkillMenu(bool enabled)
    {
        Vector2 startPos = SkillMenuTransform.localPosition;
        Vector2 targetPos = enabled ? skillMenuTargetPos : skillMenuDefaultPos;
        yield return StartCoroutine(UpdatePositionAndScale(SkillMenuTransform, startPos, targetPos, skillMenuAnimSpeed));
    }

    private float GetAngleFromPosition(Vector2 pos)
    {
        float angle = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
        return Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
    }

    private float ElementScaler(float angle)
    {
        float difference = Mathf.Abs(angle + 90f);
        float scale = difference switch
        {
            0f => 1.25f,
            45f => 1.0f,
            _ => 0.85f
        };
        return scale;
    }


    private IEnumerator AnimateElements()
    {
        float angleDelta = SkillMenuActive ? (180f / (elements.Count - 1)) : (360f / elements.Count);
        float sectionAngle = 0;
        foreach (var element in elements)
        {
            element.oldAngle = GetAngleFromPosition(element.transform.localPosition);
            element.newAngle = sectionAngle;
            sectionAngle += angleDelta;
            element.newScale = ElementScaler(element.newAngle);
        }

        yield return StartCoroutine(LerpElements(elementAnimSpeed));
    }

    private IEnumerator LerpElements(float animSpeed)
    {
        yield return null;
        float startTime = Time.unscaledTime;
        float timer = 0;
        while (timer < animSpeed)
        {
            timer = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(timer / animSpeed);
            t = Mathf.SmoothStep(0, 1, t);
            foreach (var element in elements)
            {
                float lerpAngle = Mathf.Lerp(element.oldAngle, element.newAngle, t);
                float lerpScale = Mathf.Lerp(element.oldScale, element.newScale, t);
                UpdateElementTransform(element.transform, lerpAngle, lerpScale);

                if (element.newAngle == -90f)
                {
                    element.img.material.SetFloat("_Saturation", t);
                    UpdateSkillMenu(element);
                }
                else if (element.oldAngle == -90f)
                    element.img.material.SetFloat("_Saturation", 1 - t);

                /*if (lerpAngle == element.newAngle)
                    element.newAngle = GetAngleFromPosition(element.transform.localPosition);*/
            }
            yield return null;
        }
    }


    private void UpdateElementTransform(Transform element, float angle, float scale)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(radius * Mathf.Sin(radians), radius * Mathf.Cos(radians));
        element.localPosition = pos;
        element.localScale = new Vector2(scale, scale);
    }

    private void UpdateSkillMenu(Element element)
    {
        skillMenuTitle.text = element.Name;
        for (int i = 0; i < GLG.childCount; i++)
        {
            var t = GLG.GetChild(i);
            int index = Array.IndexOf(element.contextOptions, t);
            t.SetSiblingIndex(index);
        }
    }

    public void ScrollElement(Element element) => StartCoroutine(ElementScroll(element));
    private IEnumerator ElementScroll(Element selectedElement)
    {
        float angle = GetAngleFromPosition(selectedElement.transform.localPosition);
        angle *= angle > 0 ? -1 : 1;
        float step = Mathf.Abs((angle + 90f) / 45f);
        print(angle + " " + step + " " + selectedElement.name);
        float direction = 0;
        if (angle > -90f)
            direction = 1f;
        else if (angle < -90f)
            direction = -1f;
        else
            yield break;

        StartCoroutine(AnimateSkillMenu(false));
        skillBtn.interactable = false;
        float angleDelta = -180f / (elements.Count - 1);
        Element nc0 = null, nc45 = null;                   //NC - needs correction
        for (int s = 0; s < 1; s++)
        {
            foreach (var element in elements)
            {
                element.button.interactable = false;
                element.newAngle = GetAngleFromPosition(element.transform.localPosition);
                element.oldAngle = element.newAngle * (element.newAngle > 0 ? -1 : 1);
                var target = element.oldAngle + (angleDelta * direction * step);
                //print(element.name + " " + target);
                if (target < -180f)
                {
                    if (target < -180f + angleDelta)
                    {
                        nc45 = element;
                        target = negativeBuffer + angleDelta;
                    }
                    else
                    {
                        nc0 = element;
                        target = negativeBuffer;
                    }
                }
                else if (target > 0f)
                {
                    if (target > -angleDelta)
                    {
                        target = positiveBuffer + -angleDelta;
                    }
                    else
                        target = positiveBuffer;
                }

                element.newAngle = target;
                element.newScale = ElementScaler(target);
                //print(element.name + " correct: " + target);
            }

            yield return StartCoroutine(LerpElements(elementAnimSpeed));

            if (nc0 != null)
            {
                float dirtyAngle = GetAngleFromPosition(nc0.transform.localPosition);
                if (dirtyAngle != 0f)
                {
                    float radians = 0f;
                    UpdateElementTransform(nc0.transform, radians, nc0.newScale);
                    nc0.newAngle = radians;
                    print(dirtyAngle + " " + nc0.name + " " + nc0.newAngle);
                }
                nc0 = null;
            }
            /*if (nc45 != null)
            {
                float dirtyAngle = GetAngleFromPosition(nc45.transform.localPosition);
                print(dirtyAngle + " " + nc45.name + " " + nc45.newAngle);
                if (dirtyAngle != angleDelta)
                {
                    float radians = angleDelta;
                    UpdateElementTransform(nc45.transform, radians, nc45.newScale);
                    nc45.newAngle = radians;
                }
                nc45 = null;
            }*/
        }
        skillBtn.interactable = true;
        yield return StartCoroutine(AnimateSkillMenu(true));
        foreach (Element e in elements)
            e.button.interactable = true;
    }
}