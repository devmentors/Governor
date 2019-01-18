using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Governor.Server.Domain;
using Governor.Shared;

namespace Governor.Server.Builders
{
    public class ServiceBuilder
    {
        public Service Build(string name, string directory, string filename, string arguments, string url = null,
            bool sharedShell = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Service name cannot be empty.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Service directory cannot be empty.", nameof(directory));
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Service filename cannot be empty.", nameof(filename));
            }

            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    WorkingDirectory = directory,
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = !sharedShell,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                },
            };

            return new Service(name, process, url);
        }
    }
}