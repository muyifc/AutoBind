/*
 * @Author: MuYiFC 
 * @Date: 2025-03-09 17:55:28 
 * @Last Modified by:   MuYiFC 
 * @Last Modified time: 2025-03-09 17:55:28 
 */
 
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tools.AutoBindEditor
{
    public static class HungarianNotation
    {
        // 缓存已处理过的类型前缀
        private static readonly Dictionary<Type, string> _prefixCache = new Dictionary<Type, string>();

        // 基础类型前缀映射
        private static readonly Dictionary<Type, string> _basicTypePrefixes = new Dictionary<Type, string>
        {
            // 基础类型
            { typeof(bool), "b" },
            { typeof(byte), "by" },
            { typeof(sbyte), "sb" },
            { typeof(char), "c" },
            { typeof(decimal), "dec" },
            { typeof(double), "d" },
            { typeof(float), "f" },
            { typeof(int), "n" },
            { typeof(uint), "u" },
            { typeof(long), "l" },
            { typeof(ulong), "ul" },
            { typeof(short), "s" },
            { typeof(ushort), "us" },
            { typeof(string), "str" },
            { typeof(object), "obj" }
        };

        // Unity常用组件前缀映射
        private static readonly Dictionary<string, string> _unityTypePrefixes = new Dictionary<string, string>
        {
            // Transform
            { "Transform", "trans" },
            { "RectTransform", "rect" },
            
            // Renderers
            { "Renderer", "rdr" },
            { "MeshRenderer", "mesh" },
            { "SkinnedMeshRenderer", "skin" },
            { "ParticleSystemRenderer", "ps" },
            
            // Colliders
            { "Collider", "col" },
            { "BoxCollider", "box" },
            { "SphereCollider", "sphere" },
            { "CapsuleCollider", "capsule" },
            { "MeshCollider", "meshCol" },
            
            // UI Components
            { "Text", "txt" },
            { "Button", "btn" },
            { "Image", "img" },
            { "RawImage", "raw" },
            { "Slider", "sld" },
            { "Toggle", "tgl" },
            { "Scrollbar", "scroll" },
            { "ScrollRect", "scrollRect" },
            { "Dropdown", "drop" },
            { "InputField", "input" },
            { "Canvas", "canvas" },
            { "CanvasGroup", "canvasGroup" },
            { "GraphicRaycaster", "raycaster" },
            { "LayoutGroup", "layout" },
            { "ContentSizeFitter", "sizer" },
            { "AspectRatioFitter", "aspect" },
            
            // Audio
            { "AudioSource", "audio" },
            { "AudioListener", "listener" },
            
            // Animation
            { "Animator", "anim" },
            { "Animation", "anim" },
            
            // Physics
            { "Rigidbody", "rb" },
            { "Rigidbody2D", "rb2d" },
            
            // Other Common Components
            { "GameObject", "go" },
            { "ParticleSystem", "ps" },
            { "Camera", "cam" },
            { "Light", "light" },
            { "NavMeshAgent", "agent" }
        };

        // 集合类型前缀
        private static readonly Dictionary<Type, string> _collectionPrefixes = new Dictionary<Type, string>
        {
            { typeof(Array), "arr" },
            { typeof(List<>), "lst" },
            { typeof(Dictionary<,>), "dict" },
            { typeof(HashSet<>), "set" },
            { typeof(Queue<>), "queue" },
            { typeof(Stack<>), "stack" }
        };

        public static string GetPrefix(Type type)
        {
            if (type == null) return string.Empty;

            // 检查缓存
            if (_prefixCache.TryGetValue(type, out string cachedPrefix))
            {
                return cachedPrefix;
            }

            string prefix = GetPrefixInternal(type);
            _prefixCache[type] = prefix;
            return prefix;
        }

        private static string GetPrefixInternal(Type type)
        {
            // 检查基础类型
            if (_basicTypePrefixes.TryGetValue(type, out string basicPrefix))
            {
                return basicPrefix;
            }

            // 检查是否是数组
            if (type.IsArray)
            {
                return "arr" + GetPrefix(type.GetElementType());
            }

            // 检查是否是泛型集合
            if (type.IsGenericType)
            {
                Type genericTypeDef = type.GetGenericTypeDefinition();
                if (_collectionPrefixes.TryGetValue(genericTypeDef, out string collectionPrefix))
                {
                    // 对于简单的泛型类型，只使用集合前缀
                    return collectionPrefix;

                    // 如果需要包含元素类型的前缀，可以使用下面的代码
                    // Type elementType = type.GetGenericArguments()[0];
                    // return collectionPrefix + GetPrefix(elementType);
                }
            }

            // 检查Unity组件前缀
            string typeName = type.Name;
            if (_unityTypePrefixes.TryGetValue(typeName, out string unityPrefix))
            {
                return unityPrefix;
            }

            // 处理自定义类型
            return GenerateCustomTypePrefix(typeName);
        }

        private static string GenerateCustomTypePrefix(string typeName)
        {
            // 移除常见后缀
            typeName = RemoveCommonSuffixes(typeName);

            // 处理驼峰或帕斯卡命名的类型名
            var words = SplitCamelCase(typeName);

            // 如果只有一个单词，使用前三个字母
            if (words.Length == 1)
            {
                return words[0].Length <= 3 ?
                    words[0].ToLower() :
                    words[0].Substring(0, 3).ToLower();
            }

            // 如果有多个单词，使用首字母缩写
            return string.Concat(Array.ConvertAll(words, word =>
                word.Length > 0 ? word[0].ToString().ToLower() : ""));
        }

        private static string RemoveCommonSuffixes(string typeName)
        {
            string[] suffixes = new[]
            {
                "Component",
                "Behaviour",
                "Behavior",
                "Controller",
                "Manager",
                "System",
                "Script",
                "Base",
                "Interface",
                "Abstract",
                "Impl"
            };

            foreach (var suffix in suffixes)
            {
                if (typeName.EndsWith(suffix))
                {
                    return typeName.Substring(0, typeName.Length - suffix.Length);
                }
            }

            return typeName;
        }

        private static string[] SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];

            // 在大写字母前添加空格，但避免拆分连续的大写字母（如UI、IO等）
            var words = Regex.Split(input, @"(?<!^)(?<![\p{Lu}])(?=[\p{Lu}])|(?<!^)(?<=[a-z])(?=[\p{Lu}])");

            // 过滤空字符串
            return Array.FindAll(words, word => !string.IsNullOrEmpty(word));
        }

        // 清理缓存
        public static void ClearCache()
        {
            _prefixCache.Clear();
        }
    }
}
