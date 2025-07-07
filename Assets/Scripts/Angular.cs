using UnityEngine;

public static class Angular
{
    public static void GetPositionFromAngle(Transform element, float angle, float radius, float scale)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(radius * Mathf.Sin(radians), radius * Mathf.Cos(radians));
        element.localPosition = pos;
        element.localScale = new Vector2(scale, scale);
    }

    public static float GetAngleFromPosition(Vector2 pos)
    {
        float angle = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
        return angle < 0 ? angle + 360f : angle;
    }

    public static float AngleCorrection(float angle, float angleDelta)
    {
        if (angle > 360f)
            angle += 90 + angleDelta;
        else if (angle < 180f)
            angle -= 90 + angleDelta;
        return angle;
    }

    public static float AngleOffsetting(float oldAngle, float newAngle, float angleDelta, Element chosenElement, bool value = false)
    {
        float gap;
        if (value)
        {
            newAngle = AngleCorrection(newAngle, angleDelta);
            gap = Mathf.Abs(newAngle - oldAngle);
            if (gap > 180f)
            {
                if (newAngle < oldAngle)
                    newAngle += 360f;
                else
                    newAngle -= 360f;
            }

            if (chosenElement?.oldAngle == 72f)
            {
                if (oldAngle < newAngle)
                    newAngle -= 360f;
            }
            return newAngle;
        }
        else
        {
            gap = Mathf.Abs(oldAngle - newAngle);
            if (gap > 180f)
            {
                if (oldAngle < newAngle)
                    oldAngle += 360f;
                else
                    oldAngle -= 360f;
            }

            if (chosenElement?.newAngle == 72f)
            {
                if (oldAngle > newAngle)
                    oldAngle -= 360f;
            }
            return oldAngle;
        }
    }
}
