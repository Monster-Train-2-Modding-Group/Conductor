// Allows use of C# 9 'init' accessors in Unity.
// Safe to include in any runtime/assembly definition.
// https://docs.unity3d.com/2022.1/Documentation/Manual/CSharpCompiler.html
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}