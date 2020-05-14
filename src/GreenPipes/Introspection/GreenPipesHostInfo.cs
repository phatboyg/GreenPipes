namespace GreenPipes.Introspection
{
    using System;
    using System.Diagnostics;
    using System.Reflection;


    [Serializable]
    public class GreenPipesHostInfo :
        ProbeHostInfo
    {
        public GreenPipesHostInfo()
        {
        }

        public GreenPipesHostInfo(bool initialize)
        {
            MachineName = Environment.MachineName;

            GreenPipesVersion = GetAssemblyFileVersion(typeof(Pipe).GetTypeInfo().Assembly);

            var currentProcess = Process.GetCurrentProcess();
            ProcessId = currentProcess.Id;
            ProcessName = currentProcess.ProcessName;

            FrameworkVersion = Environment.Version.ToString();
            OperatingSystemVersion = Environment.OSVersion.ToString();

            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
            entryAssembly ??= System.Reflection.Assembly.GetCallingAssembly();

            var assemblyName = entryAssembly.GetName();
            Assembly = assemblyName.Name;
            AssemblyVersion = GetAssemblyFileVersion(entryAssembly);
        }

        public string MachineName { get; private set; }
        public string ProcessName { get; private set; }
        public int ProcessId { get; private set; }
        public string Assembly { get; private set; }
        public string AssemblyVersion { get; private set; }
        public string FrameworkVersion { get; private set; }
        public string GreenPipesVersion { get; private set; }
        public string OperatingSystemVersion { get; private set; }

        static string GetAssemblyFileVersion(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (attribute != null)
                return attribute.Version;

            var assemblyLocation = assembly.Location;
            if (assemblyLocation != null)
                return FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;

            return "Unknown";
        }
    }
}
