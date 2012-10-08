using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;
using NUnit.Framework;

namespace Autofac.Tests.PartialTrust
{
    [TestFixture]
    public class PartialTrustTestExecutor
    {
        // Put the actual tests in the PartialTrustTests class as public void methods
        // with no parameters. This fixture will automatically enumerate them and execute them
        // in a partial trust domain.
        private AppDomain _remoteDomain;

        [SetUp]
        public void SetUp()
        {
            _remoteDomain = CreateSandboxDomain();
        }

        [TearDown]
        public void TearDown()
        {
            AppDomain.Unload(_remoteDomain);
        }

        [Test(Description = "Executes the partial trust tests.")]
        [TestCaseSource("ExecutePartialTrustTestsSource")]
        public void ExecutePartialTrustTests(MethodInfo testMethod)
        {
            var fixture = CreateRemoteFixture();
            try
            {
                testMethod.Invoke(fixture, null);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }
        }

        public IEnumerable<MethodInfo> ExecutePartialTrustTestsSource()
        {
            return
                typeof(PartialTrustTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(void) && m.ContainsGenericParameters == false && m.GetParameters().Length == 0);
        }

        private PartialTrustTests CreateRemoteFixture()
        {
            var assemblyName = typeof(PartialTrustTests).Assembly.FullName;
            var typeName = typeof(PartialTrustTests).FullName;
            var executor = (PartialTrustTests)_remoteDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
            return executor;
        }

        private static AppDomain CreateSandboxDomain()
        {
            // Normally from a security perspective we'd put the sandboxed app in its own
            // base directory, but to make things easier (so we don't have to copy the NUnit
            // assembly, etc.) we'll just mirror the current test domain settings.
            var info = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = AppDomain.CurrentDomain.RelativeSearchPath
            };

            // Grant set is the same set of permissions as ASP.NET medium trust EXCEPT
            // it excludes the FileIOPermission, IsolatedStorageFilePermission, and PrintingPermission.
            var grantSet = new PermissionSet(null);
            grantSet.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));
            grantSet.AddPermission(new DnsPermission(PermissionState.Unrestricted));
            grantSet.AddPermission(new EnvironmentPermission(EnvironmentPermissionAccess.Read, "TEMP;TMP;USERNAME;OS;COMPUTERNAME"));
            grantSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.ControlThread | SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.RemotingConfiguration));
            grantSet.AddPermission(new SmtpPermission(SmtpAccess.Connect));
            grantSet.AddPermission(new SqlClientPermission(PermissionState.Unrestricted));
            grantSet.AddPermission(new TypeDescriptorPermission(PermissionState.Unrestricted));
            grantSet.AddPermission(new WebPermission(PermissionState.Unrestricted));
            grantSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));

            return AppDomain.CreateDomain("Sandbox", null, info, grantSet);
        }
    }
}
