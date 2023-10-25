#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

[CustomPropertyDrawer(typeof(TypeDropDownAttribute))]
public class TypeDropDownPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        TypeDropDownAttribute typeDropDownAttribute = attribute as TypeDropDownAttribute;
        Type baseType = typeDropDownAttribute.BaseType;

        string[] types = GetDerivedTypeNames(baseType);

        string value = property.stringValue;
        int index = Array.IndexOf(types, value);

        if (index < 0)
            index = 0;

        EditorGUI.BeginProperty(position, label, property);

        Rect valueEditPosition = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
        index = EditorGUI.Popup(valueEditPosition, index, types);

        property.stringValue = types[index];

        EditorGUI.EndProperty();
    }
    private static List<Type> GetDerivedTypes(Type baseType)
    {
        List<Type> derivedTypes = new List<Type>();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            if (!assembly.FullName.StartsWith("Assembly-CSharp"))
                continue;

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(baseType))
                {
                    derivedTypes.Add(type);
                }
            }
        }

        return derivedTypes;
    }

    private static string[] GetDerivedTypeNames(Type baseType)
    {
        List<Type> types = GetDerivedTypes(baseType);
        string[] typeNames = new string[types.Count];

        for (int i = 0; i < types.Count; ++i)
        {
            typeNames[i] = types[i].FullName;
        }

        return typeNames;
    }
}
#endif