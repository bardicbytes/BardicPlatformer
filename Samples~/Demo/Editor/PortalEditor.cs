using BardicBytes.BardicFramework.Utilities;
using BardicBytes.BardicFrameworkEditor;
using BardicBytes.BardicPlatformerSamples;
using UnityEditor;
using UnityEngine;

namespace BardicBytes.BardicPlatformerSamplesEditor
{
    [CustomEditor(typeof(Portal))]
    public class PortalEditor : BardicEditor<Portal>
    {
        private const string taName = "Target";

        protected override void OnInspectorGUIBeforeOtherFields()
        {
            //if((Target.Target == null) && GUILayout.Button("Create Default Portal Targets"))
            //{
            //    CreateTargets();
            //}
        }

        private void CreateTargets()
        {
            if (Target.Target == null)
            {
                Apply(taName);
            }

            serializedObject.ApplyModifiedProperties();

            void Apply(string name)
            {
                var go = new GameObject(name);
                var c = go.AddComponent<SphereCollider>();
                c.isTrigger = true;
                go.transform.parent = Target.transform;
                go.transform.localPosition = Vector3.zero;
                var property = serializedObject.FindProperty(StringFormatting.GetBackingFieldName(name));
                property.objectReferenceValue = go;
            }
        }
    }
}
