/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:52:32 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:52:32 
 */

using System.Collections.Generic;
using Tools.AutoBind;

namespace Tools.AutoBindEditor
{
    public interface IBindingCodeGenerator
    {
        void GenerateCode(string className, List<AutoBindComponent.BindInfo> bindings, string outputPath);
        string FileExtension { get; }
        string TemplatePath { get; }
        string GetOutputPath(string className);
    }
}