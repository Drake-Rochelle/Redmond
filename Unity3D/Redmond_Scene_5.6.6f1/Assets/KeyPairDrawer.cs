#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(redmondManager.KeyPair))]
public class KeyPairDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float halfWidth = position.width / 2f;
        Rect inputRect = new Rect(position.x, position.y, halfWidth - 5, position.height);
        Rect outputRect = new Rect(position.x + halfWidth + 5, position.y, halfWidth - 5, position.height);

        SerializedProperty inputProp = property.FindPropertyRelative("input");
        SerializedProperty outputProp = property.FindPropertyRelative("output");

        EditorGUI.PropertyField(inputRect, inputProp, GUIContent.none);
        EditorGUI.PropertyField(outputRect, outputProp, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
#endif