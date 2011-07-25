using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

using MigSharp.Migrate;

using NUnit.Framework;

namespace MigSharp.NUnit.Migrate
{
    internal static class MigrateProcess
    {
        public static int Execute(string connectionString, string providerName, Assembly assembly, long timestamp)
        {
            string pathToExe = GetPathToAssembly(typeof(Program).Assembly);
            Configuration migrateExeConfig = ConfigurationManager.OpenExeConfiguration(pathToExe);

            // make a backup of the config file
            string backupPath = migrateExeConfig.FilePath + ".bak";
            File.Copy(migrateExeConfig.FilePath, backupPath, true);

            // add connection string to config file
            const string testTarget = "test-target";
            migrateExeConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(testTarget, connectionString));

            try
            {
                migrateExeConfig.Save();

                // call Migrate.exe
                return Execute(string.Format(CultureInfo.InvariantCulture, "{0} {1} -provider {2} -to {3} -traceLevel Verbose",
                    testTarget,
                    GetPathToAssembly(assembly),
                    providerName,
                    timestamp));
            }
            finally
            {
                // restore backup
                File.Copy(backupPath, migrateExeConfig.FilePath, true);
            }
        }

        public static int Execute(string arguments)
        {
            string pathToExe = GetPathToAssembly(typeof(Program).Assembly);
            var startInfo = new ProcessStartInfo(pathToExe, arguments)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
            process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine("Migrate.exe [ERR]: " + args.Data);
            process.OutputDataReceived += (sender, args) => Console.WriteLine("Migrate.exe [OUT]: " + args.Data);
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            Assert.IsNotNull(process, "Could not start Migrate.exe.");
            process.WaitForExit();
            return process.ExitCode;
        }

        private static string GetPathToAssembly(Assembly assembly)
        {
            var uri = new UriBuilder(assembly.CodeBase);
            return Uri.UnescapeDataString(uri.Path);
        }
    }
}