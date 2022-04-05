//Note: Install SonarLint!!!

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S3904:Provide an assembly version", Justification = "Unity conventions")]
[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Unity conventions")]
[assembly: SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "Unity conventions")]
[assembly: SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Unity conventions")]
[assembly: SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "Unity conventions, bad performance")]