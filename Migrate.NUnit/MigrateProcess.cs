using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using MigSharp;
using MigSharp.Migrate;
using NUnit.Framework;

namespace Migrate.NUnit
{
    internal static class MigrateProcess
    {
        private const string TestTarget = "test-target";

        public static int SetupAndExecute(string connectionString, DbPlatform dbPlatform, Assembly assembly, long timestamp)
        {
            string pathToExe = GetPathToExe();
            Configuration migrateExeConfig = ConfigurationManager.OpenExeConfiguration(pathToExe);

            // make a backup of the config file
            string backupPath = migrateExeConfig.FilePath + ".bak";
            File.Copy(migrateExeConfig.FilePath, backupPath, true);

            // add connection string to config file
            migrateExeConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(TestTarget, connectionString));

            try
            {
                migrateExeConfig.Save();

                return Execute(string.Format(CultureInfo.InvariantCulture, "{0} {1} -platform {2} -version {3} -driver {4} -to {5} -traceLevel Verbose",
                    TestTarget,
                    GetPathToAssembly(assembly),
                    dbPlatform.Platform,
                    dbPlatform.MajorVersion,
                    dbPlatform.Driver,
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
            return Execute(GetPathToExe(), arguments);
        }

        private static int Execute(string pathToExe, string arguments)
        {
            var startInfo = new ProcessStartInfo(pathToExe, arguments)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            Console.WriteLine($"Starting: {pathToExe} {arguments}");
            Process process = Process.Start(startInfo);
            Assert.IsNotNull(process, $"Could not start '{pathToExe}'.");
            process.ErrorDataReceived += (sender, args) => Console.WriteLine($"{process.ProcessName} [ERR]: " + args.Data);
            process.OutputDataReceived += (sender, args) => Console.WriteLine($"{process.ProcessName} [OUT]: " + args.Data);
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }

        private static string GetPathToExe()
        {
            return GetPathToAssembly(typeof(Program).Assembly);
        }

        private static string GetPathToAssembly(Assembly assembly)
        {
            var uri = new UriBuilder(assembly.CodeBase);
            return Uri.UnescapeDataString(uri.Path);
        }
    }
}
