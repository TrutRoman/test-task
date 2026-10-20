using System;
using UnityEditor;
using Object = UnityEngine.Object;

// Assigns private [SerializeField] fields the same way the Inspector does.
public static class EditorWiring
{
    public static void SetReference(Object target, string fieldName, Object value)
    {
        Apply(target, fieldName, property => property.objectReferenceValue = value);
    }

    public static void SetFloat(Object target, string fieldName, float value)
    {
        Apply(target, fieldName, property => property.floatValue = value);
    }

    static void Apply(Object target, string fieldName, Action<SerializedProperty> assign)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null) throw new ArgumentException($"{target.GetType().Name} has no serialized field '{fieldName}'.");

        assign(property);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
