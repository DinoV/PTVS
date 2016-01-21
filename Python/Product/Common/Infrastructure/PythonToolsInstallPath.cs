// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Microsoft.PythonTools.Infrastructure {
    public static class PythonToolsInstallPath {
        private static string GetFromAssembly(Assembly assembly, string filename, bool isFile) {
            string path = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                filename
            );
            if (isFile ? File.Exists(path) : Directory.Exists(path)) {
                return path;
            }
            return string.Empty;
        }

        private static string GetFromRegistry(string filename, bool isFile) {
            const string ROOT_KEY = "Software\\Microsoft\\PythonTools\\" + AssemblyVersionInfo.VSVersion;

            string installDir = null;
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var configKey = baseKey.OpenSubKey(ROOT_KEY)) {
                if (configKey != null) {
                    installDir = configKey.GetValue("InstallDir") as string;
                }
            }

            if (string.IsNullOrEmpty(installDir)) {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                using (var configKey = baseKey.OpenSubKey(ROOT_KEY)) {
                    if (configKey != null) {
                        installDir = configKey.GetValue("InstallDir") as string;
                    }
                }
            }

            if (!string.IsNullOrEmpty(installDir)) {
                var path = Path.Combine(installDir, filename);
                if (isFile ? File.Exists(path) : Directory.Exists(path)) {
                    return path;
                }
            }

            return string.Empty;
        }

        public static string TryGetFile(string filename) {
            string path = GetFromAssembly(typeof(PythonToolsInstallPath).Assembly, filename, true);

            if (string.IsNullOrEmpty(path)) {
                path = GetFromRegistry(filename, true);
            }

            return path;
        }

        public static string GetFile(string filename) {
            var path = TryGetFile(filename);

#if DEBUG
            if (string.IsNullOrEmpty(path)) {
                Debugger.Launch();
                path =  TryGetFile(filename);
            }
#endif

            if (string.IsNullOrEmpty(path)) {
                throw new InvalidOperationException(
                    "Unable to determine Python Tools installation path"
                );
            }

            return path;
        }

        public static string TryGetDirectory(string filename) {
            string path = GetFromAssembly(typeof(PythonToolsInstallPath).Assembly, filename, false);

            if (string.IsNullOrEmpty(path)) {
                path = GetFromRegistry(filename, true);
            }

            return path;
        }

        public static string GetDirectory(string filename) {
            var path = TryGetDirectory(filename);

#if DEBUG
            if (string.IsNullOrEmpty(path)) {
                Debugger.Launch();
                path =  TryGetDirectory(filename);
            }
#endif

            if (string.IsNullOrEmpty(path)) {
                throw new InvalidOperationException(
                    "Unable to determine Python Tools installation path"
                );
            }

            return path;
        }
    }
}