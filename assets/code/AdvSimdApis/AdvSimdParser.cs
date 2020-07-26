using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace projs
{
    class AdvSimdParser
    {
        private static Dictionary<string, List<Tuple<string, string>>> advSimdMethods = new Dictionary<string, List<Tuple<string, string>>>();
        private static Dictionary<string, List<Tuple<string, string>>> arm64Methods = new Dictionary<string, List<Tuple<string, string>>>();

        public static void Main(string[] args)
        {
            PopulateMethods();
            GenerateText();
            Console.WriteLine("done");
        }

        private static void GenerateText()
        {
            string header =
 @"
---
layout: post
title: Hardware Intrinsics APIs for ARM64
subtitle: With examples
tags: [work, arm64, intrinsics]
---

### Introduction

In my [last post](../2019-01-01-Vectorization-APIs), I describe SIMD datatypes `Vector64<T>` and `Vector128<T>` that operates on ""hardware intrisic"" APIs. In this post I will describe the intrinsic APIs for ARM64 and how you can use them to optimize your code if you are writing a .NET API targetting ARM64. This is 3 post series TODO.

### APIs examples
";
            string template =
            @"`{0}. {4}`

Performs '{2}' operation.

```csharp
private {5}
{{
  return {1}.{2}({3});
}}
// Returns: 
```
Below is the assembly code.

```asm
; TODO
```

Similar APIs that operate on different sizes:

```csharp
{6}
```

See MSDN reference {7}.
";
            string advSimdRefTemplate = "https://docs.microsoft.com/en-us/dotNet/api/system.runtime.intrinsics.arm.advsimd.{0}?view=net-5.0";
            string arm64RefTemplate = "https://docs.microsoft.com/en-us/dotNet/api/system.runtime.intrinsics.arm.advsimd.arm64.{0}?view=net-5.0";
            HashSet<string> methodsDone = new HashSet<string>();
            StringBuilder textBuilder = new StringBuilder();
            textBuilder.AppendLine(header);
            int count = 1;

            // Process AdvSimd methods
            foreach (string methodName in advSimdMethods.Keys)
            {
                var details = advSimdMethods[methodName].First();
                string signature = details.Item1;
                string testMethodSig = signature.Replace(methodName, methodName + "Test");
                string args = details.Item2;
                string advsimdReference = "[here](" + string.Format(advSimdRefTemplate, methodName.ToLower()) + ")";
                string arm64Reference = "[here](" + string.Format(arm64RefTemplate, methodName.ToLower()) + ")";

                StringBuilder overloadBuilder = new StringBuilder();
                var otherOverloads = advSimdMethods[methodName].Skip(1);
                if (otherOverloads.Count() > 0)
                {
                    overloadBuilder.AppendLine("// class System.Runtime.Intrinisics.AdvSimd");
                }

                overloadBuilder.Append(string.Join(Environment.NewLine, otherOverloads.Select(sm => sm.Item1)));

                // Process AdvSimd.Arm64 methods, if present
                if (arm64Methods.ContainsKey(methodName))
                {
                    var similarMethods = arm64Methods[methodName];
                    overloadBuilder.AppendLine();
                    overloadBuilder.AppendLine();
                    overloadBuilder.AppendLine("// class System.Runtime.Intrinisics.AdvSimd.Arm64");
                    overloadBuilder.Append(string.Join(Environment.NewLine, similarMethods.Select(sm => sm.Item1)));

                    advsimdReference += " and " + arm64Reference;
                }

                string contentForMethod = string.Format(template,
                    count,      // 0
                    "AdvSimd",  // 1
                    methodName, // 2
                    args, // 3
                    signature, // 4
                    testMethodSig, // 5
                    overloadBuilder.ToString(), // 6
                    advsimdReference // 7
                    );

                textBuilder.AppendLine(contentForMethod);
                Debug.Assert(methodsDone.Add(methodName), $"{methodName} already present.");
                count++;
            }

            // Process AdvSimd.Arm64 methods
            foreach (string methodName in arm64Methods.Keys)
            {
                if (methodsDone.Contains(methodName))
                {
                    continue;
                }

                var details = arm64Methods[methodName].First();
                string signature = details.Item1;
                string testMethodSig = signature.Replace(methodName, methodName + "Test");
                string args = details.Item2;
                string arm64Reference = "[here](" + string.Format(arm64RefTemplate, methodName.ToLower()) + ")";

                StringBuilder overloadBuilder = new StringBuilder();
                var otherOverloads = arm64Methods[methodName].Skip(1);
                if (otherOverloads.Count() > 0)
                {
                    overloadBuilder.AppendLine("// class System.Runtime.Intrinisics.AdvSimd.Arm64");
                }

                overloadBuilder.Append(string.Join(Environment.NewLine, otherOverloads.Select(sm => sm.Item1)));

                string contentForMethod = string.Format(template,
                    count,      // 0
                    "AdvSimd.Arm64",  // 1
                    methodName, // 2
                    args, // 3
                    signature, // 4
                    testMethodSig, // 5
                    overloadBuilder.ToString(), // 6
                    arm64Reference // 7
                    );

                textBuilder.AppendLine(contentForMethod);
                Debug.Assert(methodsDone.Add(methodName), $"{methodName} already present.");
                count++;
            }

            File.WriteAllText(@"blog-contents.md", textBuilder.ToString());
        }

        private static void PopulateMethods()
        {
            // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Runtime/Intrinsics/Arm/AdvSimd.cs
            string contents = File.ReadAllText(@"AdvSimd.cs");
            SyntaxTree tree = CSharpSyntaxTree.ParseText(contents);
            var members = tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>();
            foreach (var member in members)
            {
                var method = member as MethodDeclarationSyntax;
                if (method != null)
                {
                    string className = ((Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)method.Parent).Identifier.ValueText;
                    string methodName = method.Identifier.Text;
                    var tableToUse = className == "Arm64" ? arm64Methods : advSimdMethods;

                    var parameters = method.ParameterList.Parameters.Select(p => p.ToString());
                    var arguments = method.ParameterList.Parameters.Select(p => p.Identifier.Text);
                    var returnType = method.ReturnType.ToString();
                    if (!tableToUse.ContainsKey(methodName))
                    {
                        tableToUse[methodName] = new List<Tuple<string, string>>();
                    }

                    string signature = $"{returnType} {methodName}({string.Join(", ", parameters)})";
                    string args = string.Join(", ", arguments);

                    tableToUse[methodName].Add(Tuple.Create(signature, args));
                }
            }
        }
    }
}
