using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Webdogz.UI")]
[assembly: AssemblyDescription("Modern Windows UI Controls")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Webdog Studios")]
[assembly: AssemblyProduct("Webdogz.UI")]
[assembly: AssemblyCopyright("Copyright © webdog 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
//[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1e96ef2a-cd85-4d65-8b7c-b2cb93d045f1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: XmlnsDefinition("http://schema.webdogz/wui", "Webdogz.UI")]
[assembly: XmlnsDefinition("http://schema.webdogz/wui", "Webdogz.UI.Presentation")]
[assembly: XmlnsDefinition("http://schema.webdogz/wui", "Webdogz.UI.Controls")]
[assembly: XmlnsDefinition("http://schema.webdogz/wui", "Webdogz.UI.Converters")]
[assembly: XmlnsDefinition("http://schema.webdogz/wui", "Webdogz.UI.Navigation")]
[assembly: XmlnsDefinition("http://schema.webdogz/wui", "Webdogz.UI.TaskbarNotification")]
[assembly: XmlnsPrefix("http://schema.webdogz/wui", "wui")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]
[assembly: NeutralResourcesLanguageAttribute("en-US")]