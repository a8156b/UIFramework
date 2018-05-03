using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VBM;
using VBM.Reflection;

namespace VBMEditor {
    [CustomEditor(typeof(ViewModelBinding)), CanEditMultipleObjects]
    public class ViewModelBindingEditor : Editor {
        protected List<System.Type> modelTypeList;

        private const string expandStatusText = " - Click Expand";
        private const string collapseStatusText = " - Click Collapse";
        private bool switchModelSelected;

        public static ViewModelBindingEditor Instance { get; protected set; }

        protected virtual void OnEnable() {
            Instance = this;
            modelTypeList = ReflectionUtility.GetModelTypeList();
        }

        public virtual int IndexOfModelProperty(string name) {
            return modelTypeList.FindIndex((element) => { return element.FullName == name; });
        }

        public virtual List<PropertyInfo> GetModelPropertyList(int selected) {
            List<PropertyInfo> propertyList = new List<PropertyInfo>();
            ReflectionUtility.ForeachGetClassProperty(modelTypeList[selected], (propertyInfo) => {
                if (propertyInfo.CanRead)
                    propertyList.Add(propertyInfo);
            });
            return propertyList;
        }

        public virtual void AddPropertyInMenu(int index, GenericMenu menu, System.Type propertyType, System.Action<string> MenuFunction) {
            List<PropertyInfo> propertyList = GetModelPropertyList(index);
            foreach (PropertyInfo propertyInfo in propertyList) {
                if (propertyType == null || propertyType.IsAssignableFrom(propertyInfo.PropertyType)) {
                    menu.AddItem(new GUIContent(propertyInfo.Name), false, () => {
                        MenuFunction(propertyInfo.Name);
                    });
                } else {
                    menu.AddDisabledItem(new GUIContent(propertyInfo.Name));
                }
            }
        }

        protected void DrawSelectedModel(SerializedProperty modelUniqueId) {
            if (GUILayout.Button(modelUniqueId.stringValue)) {
                ShowAddMemberMenu(modelUniqueId);
            }
        }

        protected void AddModelTypeMenus(SerializedProperty property, GenericMenu menu) {
            foreach (System.Type type in modelTypeList) {
                GUIContent content = new GUIContent(type.FullName);
                menu.AddItem(content, false, () => {
                    property.stringValue = type.FullName;
                    serializedObject.ApplyModifiedProperties();
                });
            }
        }

        protected virtual void ShowAddMemberMenu(SerializedProperty property) {
            GenericMenu menu = new GenericMenu();
            AddModelTypeMenus(property, menu);
            menu.ShowAsContext();
        }

        public override void OnInspectorGUI() {
            ViewModelBinding behavior = target as ViewModelBinding;
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            SerializedProperty parentBinding = serializedObject.FindProperty("parentBinding");
            EditorGUILayout.PropertyField(parentBinding);
            if (parentBinding.objectReferenceValue != null) {
                ViewModelBinding parent = parentBinding.objectReferenceValue as ViewModelBinding;
                if (!behavior.transform.IsChildOf(parent.transform))
                    EditorGUILayout.HelpBox("The object is not parent transform.", MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            SerializedProperty modelUniqueId = serializedObject.FindProperty("modelUniqueId");
            EditorGUILayout.PrefixLabel("Model");
            if (switchModelSelected) {
                EditorGUILayout.PropertyField(modelUniqueId, GUIContent.none, null);
            } else {
                if (GUILayout.Button(modelUniqueId.stringValue))
                    ShowAddMemberMenu(modelUniqueId);
            }

            switchModelSelected = EditorGUILayout.Toggle(switchModelSelected, EditorStyles.radioButton, GUILayout.Width(15f));
            EditorGUILayout.EndHorizontal();

            SerializedProperty propertiesBinding = serializedObject.FindProperty("propertiesBinding");
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(modelUniqueId.stringValue));
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUI.indentLevel++;
            DrawPropertiesBinding(propertiesBinding);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawPropertyBindingList(SerializedProperty bindingList) {
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.BeginHorizontal("box");
            GUI.backgroundColor = backgroundColor;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            string displayName = bindingList.displayName + (bindingList.isExpanded ? collapseStatusText : expandStatusText);
            bindingList.isExpanded = EditorGUILayout.Foldout(bindingList.isExpanded, displayName, true);
            if (GUILayout.Button(new GUIContent("Add", "Add Property Binding"), EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
                bindingList.InsertArrayElementAtIndex(bindingList.arraySize);
            }
            if (GUILayout.Button(new GUIContent("Clear", "Remove all Property Binding"), EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
                bindingList.ClearArray();
            }
            EditorGUILayout.EndHorizontal();

            if (bindingList.isExpanded) {
                for (int i = 0; i < bindingList.arraySize; i++) {
                    SerializedProperty elementProperty = bindingList.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    Color contentColor = GUI.contentColor;
                    GUI.contentColor = Color.cyan;
                    EditorGUILayout.LabelField("Property Binding " + i, EditorStyles.boldLabel);
                    if (GUILayout.Button(new GUIContent("X", "Remove Property Binding"), EditorStyles.miniButton, GUILayout.Width(25))) {
                        bindingList.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUI.contentColor = contentColor;
                    EditorGUILayout.EndHorizontal();
                    ChildPropertyField(elementProperty);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void ChildPropertyField(SerializedProperty property) {
            int depth = property.depth + 1;
            foreach (SerializedProperty childProperty in property) {
                if (childProperty.depth > depth) continue;
                EditorGUILayout.PropertyField(childProperty, true);
            }
        }

        private void AddMenuItem(GenericMenu menu, SerializedProperty bindingList) {
            if (bindingList.arraySize == 0) {
                menu.AddItem(new GUIContent(bindingList.displayName), false, (propertyPath) => {
                    SerializedProperty property = serializedObject.FindProperty(propertyPath.ToString());
                    property.InsertArrayElementAtIndex(property.arraySize);
                    serializedObject.ApplyModifiedProperties();
                }, bindingList.propertyPath);
            } else {
                menu.AddDisabledItem(new GUIContent(bindingList.displayName));
            }
        }

        private void DrawPropertiesBinding(SerializedProperty propertiesBinding) {
            //
            int depth = propertiesBinding.depth + 1;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add New Properties Binding Type", GUILayout.ExpandWidth(false))) {
                GenericMenu menu = new GenericMenu();

                foreach (SerializedProperty childProperty in propertiesBinding) {
                    if (childProperty.isArray && depth == childProperty.depth)
                        AddMenuItem(menu, childProperty);
                }

                menu.ShowAsContext();
                return;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            foreach (SerializedProperty childProperty in propertiesBinding) {
                if (childProperty.isArray && childProperty.arraySize > 0 && depth == childProperty.depth)
                    DrawPropertyBindingList(childProperty);
            }
        }
    }
}