/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:54:55 
 * @Last Modified by: MuYiFC
 * @Last Modified time: 2025-03-09 18:03:16
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Tools.AutoBind;
using System.IO;

namespace Tools.AutoBindEditor
{
    [CustomEditor(typeof(AutoBindComponent))]
    public class AutoBindComponentEditor : Editor
    {
        #region 在Hierarchy窗口中显示绑定标记
        // 缓存所有被绑定的GameObject的instanceID
        private static HashSet<int> _boundObjectIds = new HashSet<int>();
        static AutoBindComponentEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (_boundObjectIds.Contains(instanceID))
            {
                Rect markRect = new Rect(selectionRect);
                markRect.x = selectionRect.xMax - 20;
                markRect.width = 20;

                GUIStyle markStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(0.7f, 0.7f, 0.0f) },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
                GUI.Label(markRect, "★", markStyle);
            }
        }

        /// <summary>
        /// 更新当前的绑定，作为HierarchyWindow标记的信息源
        /// </summary>
        private void UpdateCurrentBindings()
        {
            _boundObjectIds.Clear();

            if (_target != null && _target.bindings != null)
            {
                foreach (var binding in _target.bindings)
                {
                    if (binding.component == null) continue;

                    GameObject boundObject = null;
                    if (binding.component is GameObject go)
                    {
                        boundObject = go;
                    }
                    else if (binding.component is UnityEngine.Component comp)
                    {
                        boundObject = comp.gameObject;
                    }

                    if (boundObject != null)
                    {
                        _boundObjectIds.Add(boundObject.GetInstanceID());
                    }
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }
        #endregion

        #region 代码生成器
        private Dictionary<string, IBindingCodeGenerator> _generators;
        private string[] _generatorNames;
        private int _selectedGeneratorIndex;
        #endregion
        private AutoBindComponent _target;
        private Vector2 _scrollPosition;

        // 用于存储GameObject的所有组件选择
        // Key = BindInfo.GetHashCode(), Value = 所有组件
        private Dictionary<int, Component[]> _bindingComponents =
            new Dictionary<int, Component[]>();

        // Key = BindInfo.GetHashCode(), Value = 当前选中的组件索引
        private Dictionary<int, int> _selectedComponentIndex = new Dictionary<int, int>();

        private void OnEnable()
        {
            _target = target as AutoBindComponent;

            UpdateBindings();

            UpdateCurrentBindings();

            InitializeGenerators();
        }

        private void UpdateBindings()
        {
            // 清空并重新初始化缓存
            _bindingComponents.Clear();
            _selectedComponentIndex.Clear();

            // 重新加载所有绑定的组件信息
            if (_target != null && _target.bindings != null)
            {
                foreach (var bindInfo in _target.bindings)
                {
                    GameObject go = GetGameObjectFromBinding(bindInfo.component);
                    if (go != null)
                    {
                        var bindingKey = GetBindingKey(bindInfo);
                        Component[] components = go.GetComponents<Component>();
                        _bindingComponents[bindingKey] = components;

                        // 设置当前选中的组件索引
                        if (bindInfo.component is GameObject)
                        {
                            _selectedComponentIndex[bindingKey] = 0;
                        }
                        else if (bindInfo.component is Component currentComp)
                        {
                            int foundIndex = System.Array.FindIndex(components, c => c == currentComp);
                            _selectedComponentIndex[bindingKey] = foundIndex != -1 ? foundIndex + 1 : 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测空引用
        /// </summary>
        /// <param name="autoBindComponent"></param>
        /// <returns></returns>
        private bool CheckMissingComponents(AutoBindComponent autoBindComponent)
        {
            if (autoBindComponent.bindings == null || autoBindComponent.bindings.Count == 0)
                return false;

            foreach (var bindInfo in autoBindComponent.bindings)
            {
                if (bindInfo.component == null)
                    return true;
            }

            return false;
        }

        private void InitializeGenerators()
        {
            var config = AutoBindGeneratorConfig.Instance;
            if (config == null) return;

            _generators = new Dictionary<string, IBindingCodeGenerator>();
            foreach (var langConfig in config.languageConfigs)
            {
                switch (langConfig.name.ToLower())
                {
                    case "c#":
                        _generators[langConfig.name] = new CSharpBindingGenerator(langConfig);
                        break;
                    case "lua":
                        _generators[langConfig.name] = new LuaBindingGenerator(langConfig);
                        break;
                }
            }

            _generatorNames = _generators.Keys.ToArray();
        }

        public override void OnInspectorGUI()
        {
            UpdateDragArea();

            var hasMissing = CheckMissingComponents(_target);
            if (hasMissing)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("警告：存在丢失的组件引用！请检查下方标红的项目。", MessageType.Error);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // 显示现有绑定
            for (int i = 0; i < _target.bindings.Count; i++)
            {
                var bindInfo = _target.bindings[i];
                var isMissing = bindInfo.component == null;
                var isDel = false;
                if (isMissing)
                {
                    GUI.backgroundColor = Color.red;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                }

                UpdateValidBind(i, bindInfo, out isDel);

                EditorGUILayout.EndVertical();

                if (isDel)
                {
                    break;
                }
            }

            EditorGUILayout.EndScrollView();

            // 添加新绑定按钮
            // if (GUILayout.Button("添加新绑定"))
            // {
            //     _target.bindings.Add(new AutoBindComponent.BindInfo());
            // }

            UpdateGenerate();

            // 应用修改
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }

        /// <summary>
        /// 创建拖拽区域
        /// </summary>
        private void UpdateDragArea()
        {
            GUI.backgroundColor = Color.green;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "拖拽组件到这里");
            GUI.backgroundColor = Color.white;

            // 处理拖拽事件
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    Debug.Log("DragUpdated");
                    if (!dropArea.Contains(evt.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject gameObject)
                            {
                                // 存储组件列表供下拉框使用
                                int instanceId = gameObject.GetInstanceID();
                                Component[] components = gameObject.GetComponents<Component>();
                                if (components.Length <= 1) // 只有Transform组件
                                {
                                    // 直接添加GameObject
                                    AddBinding(gameObject);
                                    UpdateCurrentBindings();
                                }
                                else
                                {
                                    // 添加一个临时绑定
                                    var bindInfo = new AutoBindComponent.BindInfo
                                    {
                                        name = GenerateSuggestedName(gameObject),
                                        component = gameObject,
                                    };
                                    _target.bindings.Add(bindInfo);
                                    var bindingKey = GetBindingKey(bindInfo);
                                    _bindingComponents[bindingKey] = components;
                                    _selectedComponentIndex[bindingKey] = 0;
                                    UpdateCurrentBindings();
                                }
                            }
                            if (draggedObject is Component component)
                            {
                                AddBinding(component);
                                UpdateCurrentBindings();
                            }
                        }
                        EditorUtility.SetDirty(_target);
                    }
                    evt.Use();
                    break;
            }
        }

        private void UpdateGenerate()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("代码生成", EditorStyles.boldLabel);

            // 选择生成器
            _selectedGeneratorIndex = EditorGUILayout.Popup("脚本类型",
                _selectedGeneratorIndex,
                _generatorNames
            );

            if (GUILayout.Button("生成绑定代码"))
            {
                var generator = _generators[_generatorNames[_selectedGeneratorIndex]];
                string className = _target.gameObject.name;
                string outputPath = $"{generator.GetOutputPath(className)}";

                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                // 生成代码
                generator.GenerateCode(className, _target.bindings, outputPath);

                AssetDatabase.Refresh();
                Debug.Log($"绑定代码已生成到: {outputPath}");
            }
        }

        private void UpdateValidBind(int index, AutoBindComponent.BindInfo bindInfo, out bool isDel)
        {
            isDel = false;

            var bindingKey = GetBindingKey(bindInfo);

            // 显示建议名称（只读）
            string suggestedName = GenerateSuggestedName(bindInfo.component);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("建议名称", suggestedName);
            EditorGUI.EndDisabledGroup();

            // 显示自定义名称（可编辑）
            EditorGUILayout.BeginHorizontal();
            bindInfo.name = EditorGUILayout.TextField("自定义名称", bindInfo.name);

            // 添加"使用建议名称"按钮
            if (GUILayout.Button("使用建议", GUILayout.Width(60)))
            {
                bindInfo.name = suggestedName;
                EditorUtility.SetDirty(_target);
            }

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                _bindingComponents.Remove(bindingKey);
                _selectedComponentIndex.Remove(bindingKey);
                _target.bindings.RemoveAt(index);
                UpdateCurrentBindings();
                isDel = true;
            }

            EditorGUILayout.EndHorizontal();

            if (isDel)
            {
                return;
            }

            // 显示GameObject字段
            GameObject currentGO = GetGameObjectFromBinding(bindInfo.component);
            GameObject newGO =
                EditorGUILayout.ObjectField("游戏物体", currentGO, typeof(GameObject), true)
                as GameObject;

            // 如果GameObject改变，更新组件列表
            if (newGO != currentGO)
            {
                UpdateComponentsList(bindInfo, newGO);
                UpdateCurrentBindings();
            }

            // 如果有组件列表，显示组件选择
            if (currentGO != null)
            {
                if (
                    _bindingComponents.TryGetValue(bindingKey, out Component[] components)
                    && components != null
                    && components.Length > 0
                )
                {
                    // 添加GameObject选项
                    var componentList = new List<string> { "GameObject" };
                    componentList.AddRange(
                        components.Select(c => c?.GetType().Name ?? "Missing")
                    );
                    string[] componentNames = componentList.ToArray();

                    // 确保选中索引有效
                    if (!_selectedComponentIndex.ContainsKey(bindingKey))
                    {
                        _selectedComponentIndex[bindingKey] = 0;
                    }

                    // 找到当前组件的索引
                    int currentIndex = _selectedComponentIndex[bindingKey];
                    if (bindInfo.component != null)
                    {
                        if (bindInfo.component is GameObject)
                        {
                            currentIndex = 0;
                        }
                        else
                        {
                            int foundIndex = System.Array.FindIndex(
                                components,
                                c => c == bindInfo.component
                            );
                            if (foundIndex != -1)
                            {
                                // 在组件数组中查找，需要+1因为添加了GameObject选项
                                currentIndex = foundIndex + 1;
                                _selectedComponentIndex[bindingKey] = currentIndex;
                            }
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    var newSelectedIndex = EditorGUILayout.Popup(
                        "选择组件",
                        currentIndex,
                        componentNames
                    );

                    if (EditorGUI.EndChangeCheck())
                    {
                        Object newComponent;
                        if (newSelectedIndex == 0) // GameObject选项
                        {
                            newComponent = currentGO;
                        }
                        else
                        {
                            newComponent = components[newSelectedIndex - 1];
                        }

                        // 检查是否存在重复绑定
                        bool isDuplicate = _target.bindings.Any(b =>
                            b != bindInfo
                            && // 不是当前项
                            (
                                (
                                    newComponent is GameObject
                                    && b.component is GameObject
                                    && GetGameObjectFromBinding(b.component)
                                        == GetGameObjectFromBinding(newComponent)
                                )
                                || // GameObject重复判断
                                (
                                    newComponent is Component newComp
                                    && b.component is Component existComp
                                    && existComp.GetType() == newComp.GetType()
                                    && // 组件类型相同
                                    GetGameObjectFromBinding(b.component)
                                        == GetGameObjectFromBinding(newComponent)
                                )
                            ) // 相同GameObject
                        );

                        if (isDuplicate)
                        {
                            // 显示警告对话框
                            if (
                                EditorUtility.DisplayDialog(
                                    "警告",
                                    $"该{(newSelectedIndex == 0 ? "GameObject" : components[newSelectedIndex - 1].GetType().Name)}已经被绑定，是否继续？",
                                    "继续",
                                    "取消"
                                )
                            )
                            {
                                // 用户确认，更新组件
                                _selectedComponentIndex[bindingKey] = newSelectedIndex;
                                bindInfo.component = newComponent;
                                bindInfo.name = GenerateSuggestedName(bindInfo.component);
                                EditorUtility.SetDirty(_target);
                            }
                        }
                        else
                        {
                            // 没有重复，直接更新
                            _selectedComponentIndex[bindingKey] = newSelectedIndex;
                            bindInfo.component = newComponent;
                            bindInfo.name = GenerateSuggestedName(bindInfo.component);
                            EditorUtility.SetDirty(_target);
                        }
                    }
                }
            }
        }

        private int GetBindingKey(AutoBindComponent.BindInfo bindInfo)
        {
            return bindInfo.GetHashCode();
        }

        private void UpdateComponentsList(AutoBindComponent.BindInfo bindInfo, GameObject go)
        {
            if (go != null)
            {
                int bindingKey = GetBindingKey(bindInfo);
                Component[] components = go.GetComponents<Component>();
                _bindingComponents[bindingKey] = components;
                _selectedComponentIndex[bindingKey] = 0;

                // 更新绑定信息
                if (components.Length > 0)
                {
                    bindInfo.component = components[0];
                }
                else
                {
                    bindInfo.component = go;
                }
                // 如果名称为空或者等于之前的建议名称，则更新为新的建议名称
                if (string.IsNullOrEmpty(bindInfo.name))
                {
                    bindInfo.name = GenerateSuggestedName(go);
                }

                EditorUtility.SetDirty(_target);
            }
        }

        private void UpdateBindSelectIndex(AutoBindComponent.BindInfo bindInfo)
        {
            var bindingKey = GetBindingKey(bindInfo);
            if (!_bindingComponents.TryGetValue(bindingKey, out var components))
            {
                components = GetGameObjectFromBinding(bindInfo.component)?.GetComponents<Component>();
                if (components != null)
                {
                    _bindingComponents[bindingKey] = components;
                }
            }

            // 设置当前选中的组件索引
            if (bindInfo.component is GameObject)
            {
                _selectedComponentIndex[bindingKey] = 0;
            }
            else if (bindInfo.component is Component currentComp)
            {
                int foundIndex = System.Array.FindIndex(components, c => c == currentComp);
                _selectedComponentIndex[bindingKey] = foundIndex != -1 ? foundIndex + 1 : 0;
            }
        }

        private GameObject GetGameObjectFromBinding(Object obj)
        {
            if (obj != null)
            {
                if (obj is GameObject go)
                    return go;
                if (obj is Component comp)
                    return comp.gameObject;
            }
            return null;
        }

        private void AddBinding(Object obj)
        {
            GameObject go = GetGameObjectFromBinding(obj);
            if (go != null)
            {
                var bindInfo = new AutoBindComponent.BindInfo();
                UpdateComponentsList(bindInfo, go);

                if (!_target.bindings.Any(b => b.component == bindInfo.component))
                {
                    // 新添加的绑定默认使用建议名称
                    bindInfo.name = GenerateSuggestedName(bindInfo.component);
                    _target.bindings.Add(bindInfo);
                }
            }
        }

        private string GenerateSuggestedName(Object component)
        {
            if (component == null)
                return "";

            // 如果是 GameObject，直接使用对象名称
            if (component is GameObject go)
            {
                return AutoBindGeneratorConfig.Instance.NamingStrategy.GenerateFieldName(go.name, typeof(GameObject));
            }

            var type = component.GetType();
            var name = GetGameObjectFromBinding(component).name;

            return AutoBindGeneratorConfig.Instance.NamingStrategy.GenerateFieldName(name, type);
        }
    }
}
#endif
