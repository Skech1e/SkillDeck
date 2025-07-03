using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillUIAnimator))]
public class SkillUIAnimInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SkillUIAnimator skillUIAnimator = (SkillUIAnimator)target;
        if(GUILayout.Button("Record Button Position", GUILayout.Height(30)))
            skillUIAnimator.targetPosition = skillUIAnimator.transform.localPosition;
    }
}
