/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:54:35 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:54:35 
 */
 
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Tools.AutoBind;

namespace Tools.AutoBindEditor
{
    public class CSharpBindingGenerator : BaseBindingGenerator
    {
        public CSharpBindingGenerator(AutoBindGeneratorConfig.LanguageConfig config) : base(config)
        {
        }

        public override void GenerateCode(string className, List<AutoBindComponent.BindInfo> bindings, string outputPath)
        {
            // 读取模板
            string template = File.ReadAllText(TemplatePath);

            // 生成字段代码
            var fieldsBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    string typeStr = GetTypeString(binding.component);
                    fieldsBuilder.AppendLine($"        private {typeStr} {binding.name};");
                }
            }

            // 生成绑定代码
            var bindingBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    string typeStr = GetTypeString(binding.component);
                    bindingBuilder.AppendLine($"            {binding.name} = binder.Get<{typeStr}>(\"{binding.name}\");");
                }
            }

            var unBindingBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                if (binding.component != null)
                {
                    unBindingBuilder.AppendLine($"            {binding.name} = null;");
                }
            }

            // 替换模板中的占位符
            string code = template
                .Replace("${Namespace}", GetNamespace())
                .Replace("${ClassName}", className)
                .Replace("${Fields}", fieldsBuilder.ToString().TrimEnd())
                .Replace("${BindingCode}", bindingBuilder.ToString().TrimEnd())
                .Replace("${UnBindingCode}", unBindingBuilder.ToString().TrimEnd());

            // 写入文件
            File.WriteAllText(outputPath, code);
        }
    }
}