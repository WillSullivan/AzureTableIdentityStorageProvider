using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("AzureTableIdentityStorageProvider")]
[assembly: AssemblyDescription("An implementation of ASP.NET Identity storage classes that uses Azure tables.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("AzureTableIdentityStorageProvider")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("c6c125f6-259d-475d-a09d-c0d5aa4e2441")]
[assembly: AssemblyVersion("1.3.*")]
[assembly: AssemblyFileVersion("1.3.3.0")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(false)]

//TODO:  Refactor strings into resources.  Some day.  Because serious guys.

//1.0.* assembly/1.0.1 nuget:  Original release
//1.1.* assembly 1.1 nuget:  Implementations are generic
// 1.2.* User partition not static, table per provider, Id algorithm enforced on creation
// 1.3.* damnit, missed updating this; can reconstruct later
// 1.3.2.0 Google's provider key sucks html balls
// 1.3.3.0 FindByNameAsync now performs a case-insensitive search

