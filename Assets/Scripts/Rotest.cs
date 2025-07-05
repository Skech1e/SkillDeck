using System.Collections;
using UnityEngine;

public class Rotest : MonoBehaviour
{
    public float radius, speed;
    public Mode mode;
    public bool useLerpAngle, useSlerp;
    public float oldAngle, newAngle;
    public Quaternion oldQuat, newQuat;
    Transform child;

    private void Start()
    {
        child = transform.GetChild(0);
    }
    public void Rotate()
    {
        switch (mode)
        {
            case Mode.Mathf: StartCoroutine(RotationMath()); break;
            case Mode.Quaternion: StartCoroutine(RotationQuat()); break;
            case Mode.Vector: break;
        }
    }

    private IEnumerator RotationMath()
    {
        yield return null;
        float startTime = Time.unscaledTime;
        float timer = 0;
        float lerpAngle = 0;
        while (timer < speed)
        {
            timer = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(timer / speed);
            t = Mathf.SmoothStep(0, 1, t);
            if (useLerpAngle)
                lerpAngle = Mathf.LerpAngle(oldAngle, newAngle, t);
            else
                lerpAngle = Mathf.Lerp(oldAngle, newAngle, t);
            UpdateElementTransform(lerpAngle);
            yield return null;
        }
    }

    private IEnumerator RotationQuat()
    {
        yield return null;
        float startTime = Time.unscaledTime;
        float timer = 0;
        float lerpAngle = 0;
        Quaternion quat = Quaternion.identity;
        while (timer < speed)
        {
            timer = Time.unscaledTime - startTime;
            float t = Mathf.Clamp01(timer / speed);
            t = Mathf.SmoothStep(0, 1, t);
            if (useSlerp)
                quat = Quaternion.Slerp(oldQuat, newQuat, t);
            else
                quat = Quaternion.Lerp(oldQuat, newQuat, t);
            UpdateElementTransform(quat);
            yield return null;
        }
    }

    private void UpdateElementTransform(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(radius * Mathf.Sin(radians), radius * Mathf.Cos(radians));
        child.localPosition = pos;
    }

    private void UpdateElementTransform(Quaternion rotation)
    {
        Vector3 dir = rotation * Vector3.up;
        child.localPosition = new Vector2(dir.x, dir.y) * radius;
    }

    public void UpdatePos() => UpdateElementTransform(oldAngle);
    public void GetAngle() => oldAngle = GetAngleFromPosition();
    public bool invert;
    private float GetAngleFromPosition()
    {
        float angle = Mathf.Atan2(child.localPosition.x, child.localPosition.y) * Mathf.Rad2Deg;
        return angle < 0f ? angle+360f : angle;
    }
}
public enum Mode { Mathf, Vector, Quaternion }