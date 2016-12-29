using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyProduct("UsageStats")]
[assembly: AssemblyCompany("UsageStats")]
[assembly: AssemblyCopyright("© UsageStats contributors. All rights reserved.")]
[assembly: AssemblyTrademark("")]

// [assembly: CLSCompliant(true)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// Version numbers will be updated by the build script
[assembly: AssemblyVersion("2.0.0")]
[assembly: AssemblyFileVersion("2.0.0")]