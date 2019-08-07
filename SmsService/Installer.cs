using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SmsService
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        ServiceProcessInstaller processInstaller;
        ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            processInstaller = new ServiceProcessInstaller {
                Account = ServiceAccount.User
            };
            
            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "[===== SMS HRBS SERVICE =====]",
                Description = "Sends sms when agents are inactive",
                StartType = ServiceStartMode.Automatic                
            };
            serviceInstaller.AfterInstall += ProjectInstaller_AfterInstall;

            base.Installers.Add(processInstaller);
            base.Installers.Add(serviceInstaller);
        }

        private void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            string myAssembly = Path.GetFullPath(this.Context.Parameters["assemblypath"]);
            string logPath = Path.Combine(Path.GetDirectoryName(myAssembly) ?? "C:/", "Logs");
            Directory.CreateDirectory(logPath);
            ReplacePermissions(logPath, WellKnownSidType.NetworkServiceSid, FileSystemRights.FullControl);
        }

        static void ReplacePermissions(string filepath, WellKnownSidType sidType, FileSystemRights allow)
        {
            FileSecurity sec = File.GetAccessControl(filepath);
            SecurityIdentifier sid = new SecurityIdentifier(sidType, null);
            sec.PurgeAccessRules(sid); //remove existing
            sec.AddAccessRule(new FileSystemAccessRule(sid, allow, AccessControlType.Allow));
            File.SetAccessControl(filepath, sec);
        }
    }
}
