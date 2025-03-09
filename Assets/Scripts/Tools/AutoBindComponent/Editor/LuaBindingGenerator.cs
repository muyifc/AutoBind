/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:53:54 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:53:54 
 */

using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Tools.AutoBind;
namespace Tools.AutoBindEditor
{
    public class LuaBindingGenerator : BaseBindingGenerator
    {
        public LuaBindingGenerator(AutoBindGeneratorConfig.LanguageConfig config) : base(config)
        {
        }

        public override void GenerateCode(string className, List<AutoBindComponent.BindInfo> bindings, string outputPath)
        {
            // 读取模板
            string template = File.ReadAllText(TemplatePath);

            // 生成类型定义注释（用于EmmyLua提示）
            var typeDefsBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    string typeStr = GetTypeString(binding.component);
                    string fieldName = $"_{binding.name}";
                    typeDefsBuilder.AppendLine($"---@field private {fieldName} {typeStr}");
                }
            }

            // 生成字段初始化
            var fieldsBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    string fieldName = $"_{binding.name}";
                    fieldsBuilder.AppendLine($"    {fieldName} = nil,");
                }
            }

            // 生成绑定代码
            var bindingBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    string fieldName = binding.name;

                    if (binding.component is GameObject)
                    {
                        bindingBuilder.AppendLine($"    self.fields.{fieldName} = self.binder:Get(\"{binding.name}\", typeof(UnityEngine.GameObject))");
                    }
                    else
                    {
                        bindingBuilder.AppendLine($"    self.fields.{fieldName} = self.binder:Get(\"{binding.name}\", typeof(CS.{binding.component.GetType().FullName}))");
                    }
                }
            }

            // 生成清理代码
            var clearBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    string fieldName = $"_{binding.name}";
                    clearBuilder.AppendLine($"    self.fields.{fieldName} = nil");
                }
            }

            // 替换模板中的占位符
            string code = template
                .Replace("${ClassName}", className)
                .Replace("${TypeDefs}", typeDefsBuilder.ToString().TrimEnd())
                .Replace("${Fields}", fieldsBuilder.ToString().TrimEnd())
                .Replace("${BindingCode}", bindingBuilder.ToString().TrimEnd())
                .Replace("${ClearCode}", clearBuilder.ToString().TrimEnd());

            // 写入文件
            File.WriteAllText(outputPath, code);
        }
    }
}