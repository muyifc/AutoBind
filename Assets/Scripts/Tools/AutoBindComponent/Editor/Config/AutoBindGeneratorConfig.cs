/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:49 
 * @Last Modified by: MuYiFC
 * @Last Modified time: 2025-03-09 19:55:25
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Tools.AutoBindEditor
{
    [CreateAssetMenu(fileName = "AutoBindGeneratorConfig", menuName = "Tools/AutoBind/自动绑定配置")]
    public class AutoBindGeneratorConfig : ScriptableObject
    {
        public enum NamingStrategyType
        {
            CamelCase,
            PascalCase,
            Hungarian
        }

        [Serializable]
        public class NamingConfig
        {
            public NamingStrategyType namingStrategyType;
            public string fieldPrefix = "_";
            public string propertyPrefix = "";
        }

        [Serializable]
        public class LanguageConfig
        {
            public string name;
            public string fileExtension;
            public string templatePath;
            public string outputPath;
            public string namespaceName;
            public List<string> additionalNamespaces = new List<string>();
        }

        [Header("Language Settings")]
        public List<LanguageConfig> languageConfigs = new List<LanguageConfig>();

        [Header("Naming Convention")]
        public NamingConfig namingConfig;
        private INamingStrategy _currentNamingStrategy;
        public INamingStrategy NamingStrategy
        {
            get
            {
                if (_currentNamingStrategy == null)
                {
                    _currentNamingStrategy = CreateNamingStrategy();
                }
                return _currentNamingStrategy;
            }
        }

        [Header("Path Settings")]
        public string baseOutputPath = "Assets/Scripts";
        public string templateBasePath = "Assets/Scripts/Tools/AutoBindComponent/Editor/Templates";

        private static AutoBindGeneratorConfig _instance;
        public static AutoBindGeneratorConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<AutoBindGeneratorConfig>(AutoBindConfigWindow.DefaultConfigPath);
                    if (_instance == null)
                    {
                        Debug.LogWarning("未创建配置，请在菜单栏===Tools/AutoBind/配置===窗口中创建");
                    }
                }
                return _instance;
            }
        }

        private INamingStrategy CreateNamingStrategy()
        {
            switch (namingConfig.namingStrategyType)
            {
                case NamingStrategyType.Hungarian:
                    return new HungarianNamingStrategy(namingConfig);
                case NamingStrategyType.PascalCase:
                    return new PascalCaseNamingStrategy(namingConfig);
                case NamingStrategyType.CamelCase:
                default:
                    return new CamelCaseNamingStrategy(namingConfig);
            }
        }

        public void ResetNamingStrategy()
        {
            _currentNamingStrategy = null;
        }
    }
}