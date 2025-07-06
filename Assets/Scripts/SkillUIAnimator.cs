using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class SkillUIAnimator : MonoBehaviour
{
    public float radius;
    public bool SkillMenuActive;
    public List<Element> elements;
    public Element chosenElement;
    public Vector2 defaultPos, targetPosition;
    public float targetScale;
    public float highlightAngle = 270f;

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
        chosenElement = elements[chosenIndex];
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
        StartCoroutine(InitElementsAnimate());
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
        return angle < 0 ? angle + 360f : angle;
    }

    private float AngleCorrection(float angle, float angleDelta)
    {
        if (angle > 360f)
            angle += 90 + angleDelta;
        else if (angle < 180f)
            angle -= 90 + angleDelta;
        return angle;
    }


    private float ElementScaler(float angle)
    {
        float difference = Mathf.Abs(Mathf.DeltaAngle(highlightAngle, angle));
        float scale = difference switch
        {
            0f => 1.25f,
            45f => 1.0f,
            _ => 0.85f
        };
        return scale;
    }

    public int chosenIndex;
    private IEnumerator InitElementsAnimate()
    {
        float angleDelta = SkillMenuActive ? (180f / (elements.Count - 1)) : (360f / elements.Count);
        float sectionAngle = SkillMenuActive ? 180f : 0f;

        for (int i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            element.oldAngle = GetAngleFromPosition(element.transform.localPosition);
            if (i < 1 && SkillMenuActive)
            {
                elements[chosenIndex].newAngle = highlightAngle;
                var gap = chosenIndex - i;
                sectionAngle = highlightAngle - (gap * angleDelta);
            }
            element.newAngle = sectionAngle;
            sectionAngle += angleDelta;
            //print($"{element.Name} {element.oldAngle} {element.newAngle}");
        }
        float offset = highlightAngle - chosenElement.newAngle;

        SetElementGroupAngles(offset, angleDelta);        
        yield return StartCoroutine(LerpElements(elementAnimSpeed));
    }

    private void SetElementGroupAngles(float offset, float angleDelta)
    {
        foreach (var element in elements)
        {
            float gap;
            if (SkillMenuActive)
            {
                element.newAngle = AngleCorrection(element.newAngle + offset, angleDelta);

                gap = Mathf.Abs(element.newAngle - element.oldAngle);
                if (gap > 180f)
                {
                    if (element.newAngle < element.oldAngle)
                        element.newAngle += 360f;
                    else
                        element.newAngle -= 360f;
                }

                if (chosenElement?.oldAngle == 72f)
                {
                    if (element.oldAngle < element.newAngle)
                        element.newAngle -= 360f;
                }

                //print($"{element.Name} {element.oldAngle} {element.newAngle}");
            }
            else
            {
                gap = Mathf.Abs(element.oldAngle - element.newAngle);
                if (gap > 180f)
                {
                    if (element.oldAngle < element.newAngle)
                        element.oldAngle += 360f;
                    else
                        element.oldAngle -= 360f;
                }

                if (chosenElement?.newAngle == 72f)
                {
                    if (element.oldAngle > element.newAngle)
                        element.oldAngle -= 360f;
                }

                //print($"{element.Name} {element.oldAngle} {element.newAngle}");
            }
            element.newScale = ElementScaler(element.newAngle);
        }
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

                if (element == chosenElement && (element.newAngle == highlightAngle || element.newAngle == highlightAngle - 360f))
                {
                    element.img.material.SetFloat("_Saturation", t);
                    UpdateSkillMenu(element);
                }
                else if (element.img.material.GetFloat("_Saturation") > 0f)
                    element.img.material.SetFloat("_Saturation", 1 - t);
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
        chosenElement = selectedElement;
        float angle = GetAngleFromPosition(selectedElement.transform.localPosition);
        angle = angle < 180f ? 360f : angle;
        angle = Mathf.Round(angle);
        selectedElement.newAngle = angle;

        float diff = (angle - highlightAngle) / 45f;
        int step = Mathf.Abs((int)diff);

        float direction = 0;
        if (angle > highlightAngle)
            direction = -1f;
        else if (angle < highlightAngle)
            direction = 1f;
        else
            yield break;


        StartCoroutine(AnimateSkillMenu(false));
        skillBtn.interactable = false;
        float angleDelta = 180f / (elements.Count - 1);
        foreach (var element in elements)
        {
            element.button.interactable = false;
            float currentAngle = GetAngleFromPosition(element.transform.localPosition);
            element.oldAngle = currentAngle < 180f ? 360f : currentAngle;
            var target = element.oldAngle + (angleDelta * direction * step);
            //print(element.name + " " + target);

            target = AngleCorrection(target, angleDelta);

            element.newAngle = target;
            element.newScale = ElementScaler(target);
            //print(element.name + " correct: " + target);
        }

        yield return StartCoroutine(LerpElements(elementAnimSpeed));

        skillBtn.interactable = true;
        yield return StartCoroutine(AnimateSkillMenu(true));
        foreach (Element e in elements)
            e.button.interactable = true;
    }
}