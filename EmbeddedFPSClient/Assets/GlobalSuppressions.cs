//Note: Install SonarLint!!!

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

//SonarLint
[assembly: SuppressMessage("Major Code Smell", "S3904:Provide an assembly version", Justification = "Unity conventions")]
[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Unity conventions")]
[assembly: SuppressMessage("Major Bug", "S3903:Types should be defined in named namespaces", Justification = "Unity conventions")]
[assembly: SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Unity conventions")]
[assembly: SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "Unity conventions, bad performance")]

//StyleCop
//order rules have been temp disabled due to enormous diffs caused
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Not applicable to swiftly changing game code read inline.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:Single line comments should begin with single space", Justification = "No value.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "No value (especially if I can get leading underscore back).")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "No value.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Massive diffs.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1028:Code should not contain trailing whitespace", Justification = "Annoying on line breaks.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Massive diffs.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Leads to many small files syndrome.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields", Justification = "Massive diffs.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:Elements should be separated by blank line", Justification = "Prevents logical groups of fields/properties.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1512:Single-line comments should not be followed by blank line", Justification = "What if they're not more closely related to line below than above?")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity MonoBehaviour standard")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "Massive diffs.")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:Arithmetic expressions should declare precedence", Justification = "Learn elementary school math.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1122:Use string.Empty for empty strings", Justification = "Will cause issues for fast coding")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Prefer good enum names.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1413:Use trailing comma in multi-line initializers", Justification = "No hard rule, can be used to make modifier stop and think before placing elements after count sentinel.")]