/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:45 
 * @Last Modified by: MuYiFC
 * @Last Modified time: 2025-03-09 19:55:17
 */

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.AutoBindEditor
{
    public class AutoBindConfigWindow : EditorWindow
    {
        public static string DefaultConfigPath = "Assets/Scripts/Tools/AutoBindComponent/Editor/Config/AutoBindGeneratorConfig.asset";
        private Vector2 _scrollPosition;
        private AutoBindGeneratorConfig _config;
        private bool _isDirty;
        private double _lastSaveTime;
        private string _previewStr = "scoreText";
        private const float SAVE_DELAY = 2.0f; // 延迟保存的时间（秒）


        [MenuItem("Tools/AutoBind/配置")]
        public static void ShowWindow()
        {
            var window = GetWindow<AutoBindConfigWindow>("AutoBind Config");
            window.Show();
        }

        private void OnEnable()
        {
            _config = AutoBindGeneratorConfig.Instance;
            // 添加编辑器更新回调
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            // 移除编辑器更新回调
            EditorApplication.update -= OnEditorUpdate;
            // 确保关闭窗口时保存更改
            SaveIfDirty(true);
        }

        private void OnEditorUpdate()
        {
            // 检查是否需要保存
            SaveIfDirty(false);
        }

        private void SaveIfDirty(bool forceSave)
        {
            if (!_isDirty) return;

            double currentTime = EditorApplication.timeSinceStartup;
            if (forceSave || (currentTime - _lastSaveTime >= SAVE_DELAY))
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                _isDirty = false;
                _lastSaveTime = currentTime;
            }
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                if (GUILayout.Button("创建配置"))
                {
                    CreateConfig();
                }
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawLanguageSettings();
            DrawNamingConvention();
            DrawPathSettings();

            EditorGUILayout.EndScrollView();

            // 检查是否有修改
            if (EditorGUI.EndChangeCheck())
            {
                _isDirty = true;
                _lastSaveTime = EditorApplication.timeSinceStartup;
            }
        }

        private void CreateConfig()
        {
            var config = CreateInstance<AutoBindGeneratorConfig>();

            // 添加默认配置
            config.languageConfigs.Add(new AutoBindGeneratorConfig.LanguageConfig
            {
                name = "C#",
                fileExtension = ".cs",
                templatePath = "CSharpBinding.txt",
                outputPath = "Scripts/UI/Generated",
                namespaceName = "Game.UI"
            });

            config.languageConfigs.Add(new AutoBindGeneratorConfig.LanguageConfig
            {
                name = "Lua",
                fileExtension = ".lua",
                templatePath = "LuaBinding.txt",
                outputPath = "LuaScripts/UI/Generated",
                namespaceName = ""
            });

            var dir = Path.GetDirectoryName(DefaultConfigPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            AssetDatabase.CreateAsset(config, DefaultConfigPath);
            AssetDatabase.SaveAssets();

            _config = config;
        }

        private void DrawLanguageSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Language Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            for (int i = 0; i < _config.languageConfigs.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                var config = _config.languageConfigs[i];

                // 使用延迟字段以提高性能
                config.name = DelayedTextField("Language Name", config.name);
                config.fileExtension = DelayedTextField("File Extension", config.fileExtension);
                config.templatePath = DelayedTextField("Template Path", config.templatePath);
                config.outputPath = DelayedTextField("Output Path", config.outputPath);
                config.namespaceName = DelayedTextField("Namespace", config.namespaceName);

                DrawNamespacesList(config);

                if (GUILayout.Button("Remove Language"))
                {
                    _config.languageConfigs.RemoveAt(i);
                    _isDirty = true;
                    break;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add Language"))
            {
                _config.languageConfigs.Add(new AutoBindGeneratorConfig.LanguageConfig());
                _isDirty = true;
            }

            EditorGUI.indentLevel--;
        }

        private void DrawNamespacesList(AutoBindGeneratorConfig.LanguageConfig config)
        {
            EditorGUILayout.LabelField("Additional Namespaces");
            EditorGUI.indentLevel++;

            for (int j = 0; j < config.additionalNamespaces.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                config.additionalNamespaces[j] = EditorGUILayout.TextField(config.additionalNamespaces[j]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    config.additionalNamespaces.RemoveAt(j);
                    _isDirty = true;
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Namespace"))
            {
                config.additionalNamespaces.Add("");
                _isDirty = true;
            }

            EditorGUI.indentLevel--;
        }

        private void DrawNamingConvention()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Naming Convention", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var last = _config.namingConfig.namingStrategyType;
            _config.namingConfig.namingStrategyType = (AutoBindGeneratorConfig.NamingStrategyType)
                EditorGUILayout.EnumPopup("Naming Strategy", _config.namingConfig.namingStrategyType);
            if (last != _config.namingConfig.namingStrategyType)
            {
                _config.ResetNamingStrategy();
            }
            _config.namingConfig.fieldPrefix = EditorGUILayout.TextField("Field Prefix", _config.namingConfig.fieldPrefix);
            _config.namingConfig.propertyPrefix = EditorGUILayout.TextField("Property Prefix", _config.namingConfig.propertyPrefix);

            // 显示命名预览
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            _previewStr = EditorGUILayout.TextField("eg.", _previewStr);
            EditorGUI.BeginDisabledGroup(true);
            var strategy = _config.NamingStrategy;
            string fieldName = strategy.GenerateFieldName(_previewStr, typeof(Text));
            string propertyName = strategy.GeneratePropertyName(fieldName);
            EditorGUILayout.TextField("Field", fieldName);
            EditorGUILayout.TextField("Property", propertyName);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;
        }

        private void DrawPathSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Path Settings", EditorStyles.boldLabel);
            _config.baseOutputPath = DelayedTextField("Base Output Path", _config.baseOutputPath);
            _config.templateBasePath = DelayedTextField("Template Base Path", _config.templateBasePath);
        }

        private string DelayedTextField(string label, string value)
        {
            return EditorGUILayout.DelayedTextField(label, value);
        }
    }
}