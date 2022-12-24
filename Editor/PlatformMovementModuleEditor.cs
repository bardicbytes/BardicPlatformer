using BardicBytes.BardicFramework.EventVars;
using BardicBytes.BardicFramework.Utilities;
using BardicBytes.BardicFrameworkEditor;
using BardicBytes.BardicFrameworkEditor.Utilities;
using BardicBytes.BardicPlatformer;
using UnityEditor;
using UnityEngine;

namespace BardicBytes.BardicPlatformerEditor
{
    [CustomEditor(typeof(PlatformMovementModule))]
    public class PlatformMovementModuleEditor : BardicEditor<PlatformMovementModule>
    {
        //protected SerializedObject configFieldSerializedObject;
        protected SerializedObject evSrcSO;
        protected SerializedObject configSO;
        protected PropertyFieldHelper configPropHelper;

        protected PlatformMovementConfigEventVar sourceEV;
        protected PlatformMovementConfig fallbackValue;

        protected bool moveConfigFoldout = false;

        protected override void OnEnable()
        {
            sourceEV = null;
            fallbackValue = null;
            base.OnEnable();
            RefreshConfigEditor();
        }

        private void RefreshConfigEditor()
        {
            //prop of the module's config field
            var cfp = serializedObject.FindProperty(StringFormatting.GetBackingFieldName("ConfigField"));
            //prop of source event var reference
            var srcEVp = cfp.FindPropertyRelative("srcEV");
            //prop of value field to use if there is no srcEV to pull from
            var fallbackP = cfp.FindPropertyRelative("fallbackValue");

            sourceEV = srcEVp.objectReferenceValue as PlatformMovementConfigEventVar;
            fallbackValue = fallbackP.objectReferenceValue as PlatformMovementConfig;

            if (sourceEV != null)// when we have a source referenced, fallbackValue is ignored
            {
                //a new serialized object of the sourceEV
                evSrcSO = new SerializedObject(sourceEV);
                var instancer = Target.Actor.GetModule<EventVarInstancer>();
                //the serialized index of the platform movement config selected
                int configIndex = -1;
                if(instancer.HasInstance(sourceEV))
                {
                    var ic = instancer.FindInstanceData(sourceEV);
                    configIndex = ic.IntValue;
                }

                //the property of the list of MoveConfigs within the given MoveConfigEventVar
                var configsListProp = evSrcSO.FindProperty("configs");
                if (configIndex > -1 && configsListProp.arraySize > 0)
                {
                    var configP = configsListProp.GetArrayElementAtIndex(configIndex);
                    PlatformMovementConfig mc = configP.objectReferenceValue as PlatformMovementConfig;

                    configSO = new SerializedObject(mc);
                    configPropHelper = new PropertyFieldHelper(configSO);
                }
                else if(fallbackValue != null) //srcEV is ignored, use fallback value
                {
                    PrepFallBackProp();
                }
                else
                {
                    configPropHelper = null;
                }
            }
            else if(fallbackValue != null) //srcEV is ignored, use fallback value
            {
                PrepFallBackProp();
            }
            else
            {
                configPropHelper = null;
            }

            void PrepFallBackProp()
            {
                configSO = new SerializedObject(fallbackValue);
                configPropHelper = new PropertyFieldHelper(configSO);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var cfp = serializedObject.FindProperty(StringFormatting.GetBackingFieldName("ConfigField"));
            var srcEVp = cfp.FindPropertyRelative("srcEV");
            var fallbackP = cfp.FindPropertyRelative("fallbackValue");
            var srcTarg = srcEVp.objectReferenceValue;
            var fbTarg = fallbackP.objectReferenceValue;

            var st = this.sourceEV as EventVar;
            bool hasInstance = st != null && Target.Actor.GetModule<EventVarInstancer>().HasInstance(st);

            GUILayout.Space(10f);
            string headerText = "";
            if(srcEVp.objectReferenceValue == null && fallbackP.objectReferenceValue != null)
            {
                headerText = fallbackP.displayName + " (Fallback)";
            }
            else if(srcEVp.objectReferenceValue != null)
            {
                headerText = configSO.targetObject.name + " " + (hasInstance ? " *Instance" : " *Asset");
            }
            moveConfigFoldout = EditorGUILayout.Foldout(moveConfigFoldout, headerText);
            if (srcTarg == null && fbTarg == null) GUILayout.Label("Select a Config Above");
            if (!moveConfigFoldout) return;
            if (this.sourceEV != srcTarg || this.fallbackValue != fbTarg)
            {
                RefreshConfigEditor();
            }
            EditorGUI.indentLevel++;
            configPropHelper?.DrawPropFields();
            EditorGUI.indentLevel--;
        }
    }
}