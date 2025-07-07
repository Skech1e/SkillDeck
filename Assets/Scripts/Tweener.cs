using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tweener
{
    public IEnumerator UpdatePositionAndScale(Transform objT, Vector2 startPos, Vector2 targetPos, float animSpeed, float startScale = 0f, float finalScale = 0f)
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
                objT.localScale = Vector3.one * scale;
            }

            yield return null;
        }
    }

    public IEnumerator Fade(Image img, float fromVal, float toVal, float speed, bool value)
    {
        if (value)
            img.gameObject.SetActive(true);
        float start = Time.unscaledTime;
        Color c = img.color;
        float timer = 0f;
        while (timer < speed)
        {
            timer = Time.unscaledTime - start;
            float t = Mathf.Clamp01(timer / speed);
            t = Mathf.SmoothStep(0, 1, t);
            c.a = Mathf.Lerp(fromVal, toVal, t);
            img.color = c;
            yield return null;
        }
        img.gameObject.SetActive(value);
    }

    public IEnumerator LerpElements(List<Element> elements, float animSpeed, float highlightAngle, float radius, Element chosenElement)
    {
        float startTime = Time.unscaledTime;
        float timer = 0;
        while (timer < animSpeed)
        {
            timer = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(timer / animSpeed);
            t = Mathf.SmoothStep(0, 1, t);

            foreach (Element element in elements)
            {
                float lerpAngle = Mathf.Lerp(element.oldAngle, element.newAngle, t);
                float lerpScale = Mathf.Lerp(element.oldScale, element.newScale, t);
                Angular.GetPositionFromAngle(element.transform, lerpAngle, radius, lerpScale);

                if (element == chosenElement && (element.newAngle == highlightAngle || element.newAngle == highlightAngle - 360f))
                {
                    element.img.material.SetFloat("_Saturation", t);
                }
                else if (element.img.material.GetFloat("_Saturation") > 0f)
                    element.img.material.SetFloat("_Saturation", 1 - t);
            }
            yield return null;
        }
    }
}
