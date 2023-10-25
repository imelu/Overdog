using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TypeDropDownAttribute : PropertyAttribute
{
    public Type BaseType { get; private set; }

    public TypeDropDownAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}
