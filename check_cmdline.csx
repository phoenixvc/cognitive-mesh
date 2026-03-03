using System.Reflection;
var asm = Assembly.LoadFrom(\"/home/runner/.nuget/packages/system.commandline/2.0.3/lib/netstandard2.0/System.CommandLine.dll\");
var cmdType = asm.GetType(\"System.CommandLine.Command\");
if (cmdType != null) {
  foreach (var m in cmdType.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(m => m.Name).Take(30))
    Console.WriteLine(m.Name);
}
