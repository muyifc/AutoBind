/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:33 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:55:33 
 */
 
using System;

namespace Tools.AutoBindEditor
{
    public interface INamingStrategy
    {
        string GenerateFieldName(string originalName, Type componentType);
        string GeneratePropertyName(string fieldName);
    }
}