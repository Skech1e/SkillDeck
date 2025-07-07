using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIAnimator : MonoBehaviour
{
    [Header("Elements and Params")]
    public float radius;
    public List<Element> elements;
    public Element chosenElement;
    public int chosenIndex;
    public float targetScale;
    public float highlightAngle = 270f;
    public float elementAnimSpeed;

    [Header("Skill Menu")]
    public bool isMenuOpen;
    private Button _skillBtn;
    public Transform SkillMenuTransform;
    public Vector2 defaultPos, targetPosition;
    public Vector2 skillMenuDefaultPos, skillMenuTargetPos = Vector2.zero;
    public float skillBtnAnimSpeed, skillMenuAnimSpeed;
    public TextMeshProUGUI skillMenuTitle;
    public Image skillMenuBG;
    public Transform contextMenu;

    private Tweener _tweener;
    private void Awake()
    {
        _tweener = new();
        _skillBtn = GetComponent<Button>();
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
        isMenuOpen = !isMenuOpen;
        StartCoroutine(AnimateButton());
        StartCoroutine(InitElementsAnimate());
    }

    private IEnumerator AnimateButton()
    {
        if (isMenuOpen == false)
            yield return StartCoroutine(AnimateSkillMenu(isMenuOpen));

        foreach (Element e in elements)
            e.button.interactable = isMenuOpen;

        Vector2 startPos = transform.localPosition;
        Vector2 targetPos = isMenuOpen ? targetPosition : defaultPos;
        float startScale = transform.localScale.x;
        float finalScale = isMenuOpen ? targetScale : 1f;
        bool open = isMenuOpen;

        StartCoroutine(_tweener.Fade(skillMenuBG, open ? 0f : 0.69f, open ? 0.69f : 0f, skillMenuAnimSpeed, open));
        yield return StartCoroutine(_tweener.UpdatePositionAndScale(transform, startPos, targetPos, skillBtnAnimSpeed, startScale, finalScale));

        if (isMenuOpen)
            StartCoroutine(AnimateSkillMenu(true));
    }

    private IEnumerator AnimateSkillMenu(bool enabled)
    {
        Vector2 startPos = SkillMenuTransform.localPosition;
        Vector2 targetPos = enabled ? skillMenuTargetPos : skillMenuDefaultPos;
        yield return StartCoroutine(_tweener.UpdatePositionAndScale(SkillMenuTransform, startPos, targetPos, skillMenuAnimSpeed));
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

    private IEnumerator InitElementsAnimate()
    {
        float angleDelta = isMenuOpen ? (180f / (elements.Count - 1)) : (360f / elements.Count);
        float sectionAngle = isMenuOpen ? highlightAngle - (angleDelta * chosenIndex) : 0f;
        foreach (var element in elements)
        {
            float oldAngle = Angular.GetAngleFromPosition(element.transform.localPosition);
            float newAngle = sectionAngle;
            sectionAngle += angleDelta;
            if (isMenuOpen)
                newAngle = Angular.AngleOffsetting(oldAngle, newAngle, angleDelta, chosenElement, true);
            else
                oldAngle = Angular.AngleOffsetting(oldAngle, newAngle, angleDelta, chosenElement, false);
            element.oldAngle = oldAngle;
            element.newAngle = newAngle;
            element.newScale = ElementScaler(element.newAngle);
            //print($"{element.Name} {element.oldAngle} {element.newAngle}");
        }
        float offset = highlightAngle - chosenElement.newAngle;
        yield return StartCoroutine(_tweener.LerpElements(elements, elementAnimSpeed, highlightAngle, radius, chosenElement));
        UpdateSkillMenu(chosenElement);
    }


    private void UpdateSkillMenu(Element element)
    {
        skillMenuTitle.text = element.Name;
        for (int i = 0; i < contextMenu.childCount; i++)
        {
            var t = contextMenu.GetChild(i);
            int index = Array.IndexOf(element.contextOptions, t);
            t.SetSiblingIndex(index);
        }
    }

    public void ScrollElement(Element element) => StartCoroutine(ElementScroll(element));
    private IEnumerator ElementScroll(Element selectedElement)
    {
        chosenElement = selectedElement;
        chosenIndex = elements.IndexOf(chosenElement);
        float angle = Angular.GetAngleFromPosition(selectedElement.transform.localPosition);
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
        _skillBtn.interactable = false;
        float angleDelta = 180f / (elements.Count - 1);
        foreach (var element in elements)
        {
            element.button.interactable = false;
            float currentAngle = Angular.GetAngleFromPosition(element.transform.localPosition);
            element.oldAngle = currentAngle < 180f ? 360f : currentAngle;
            var target = element.oldAngle + (angleDelta * direction * step);
            //print(element.name + " " + target);

            target = Angular.AngleCorrection(target, angleDelta);

            element.newAngle = target;
            element.newScale = ElementScaler(target);
            //print(element.name + " correct: " + target);
        }

        yield return StartCoroutine(_tweener.LerpElements(elements, elementAnimSpeed, highlightAngle, radius, chosenElement));
        UpdateSkillMenu(chosenElement);

        _skillBtn.interactable = true;
        yield return StartCoroutine(AnimateSkillMenu(true));
        foreach (Element e in elements)
            e.button.interactable = true;
    }
}