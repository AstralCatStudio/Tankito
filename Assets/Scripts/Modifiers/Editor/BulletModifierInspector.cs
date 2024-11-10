using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BulletModifier))]
public class BulletModifierInspector : Editor
{
    private Type[] _implementations;
    private int _implementationTypeIndex;

    public override void OnInspectorGUI()
    {
        BulletModifier testBehaviour = target as BulletModifier;
        //specify type
        if (testBehaviour == null)
        {
            return;
        }

        if (_implementations == null || GUILayout.Button("Refresh implementations"))
        {
            //this is probably the most imporant part:
            //find all implementations of INode using System.Reflection.Module
            _implementations = GetImplementations<ABulletEvent>().ToArray();
        }

        EditorGUILayout.LabelField($"Found {_implementations.Count()} implementations");

        //select implementation from editor popup
        _implementationTypeIndex = EditorGUILayout.Popup(new GUIContent("Implementation"),
            _implementationTypeIndex, _implementations.Select(impl => impl.FullName).ToArray());

        if (GUILayout.Button("Create instance"))
        {
            //set new value
            testBehaviour.onDetonateEvents.Add( (ABulletEvent)Activator.CreateInstance(_implementations[_implementationTypeIndex]));
        }

        base.OnInspectorGUI();
    }

    private static Type[] GetImplementations<T>()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

        var interfaceType = typeof(T);
        return types.Where(p => interfaceType.IsAssignableFrom(p) && !p.IsAbstract).ToArray();
    }
}
