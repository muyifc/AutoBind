/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:54:47 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:54:47 
 */
 
using System;
using System.Collections.Generic;
using System.IO;
using Tools.AutoBind;
using UnityEngine;

namespace Tools.AutoBindEditor
{
    public abstract class BaseBindingGenerator : IBindingCodeGenerator
    {
        protected AutoBindGeneratorConfig.LanguageConfig Config { get; private set; }

        public BaseBindingGenerator(AutoBindGeneratorConfig.LanguageConfig config)
        {
            Config = config;
        }

        public string FileExtension => Config.fileExtension;

        public string TemplatePath => Path.Combine(
            AutoBindGeneratorConfig.Instance.templateBasePath,
            Config.templatePath
        );

        public string GetOutputPath(string className)
        {
            return Path.Combine(
                AutoBindGeneratorConfig.Instance.baseOutputPath,
                Config.outputPath,
                $"{className}.Bindings{FileExtension}"
            );
        }

        public abstract void GenerateCode(string className, List<AutoBindComponent.BindInfo> bindings, string outputPath);

        protected string GetNamespace()
        {
            return Config.namespaceName;
        }

        protected string GetTypeString(UnityEngine.Object component)
        {
            if (component is GameObject)
                return "GameObject";
            return component.GetType().Name;
        }
    }
}