using System;
using System.Diagnostics;
using System.Threading;

namespace Governor.Server.Domain
{
    public class Service : IEquatable<Service>
    {
        public string Name { get; }
        public Process Process { get; }
        public string Url { get; }
        public bool IsRunning { get; private set; }

        public Service(string name, Process process, string url = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Service name cannot be empty.", nameof(name));
            }
            
            if (process is null)
            {
                throw new ArgumentException("Process cannot be null.", nameof(name));
            }
            
            Name = name;
            Process = process;
            Url = url;
            
            process.Exited += ProcessOnExited;
        }

        public void Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException();
            }

            Process.Start();
            IsRunning = true;
        }

        public void Kill()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException();
            }

            Process.Kill();
            Process.WaitForExit();
            IsRunning = false;
        }

        public bool Equals(Service other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Service) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            IsRunning = false;
        }
    }
}