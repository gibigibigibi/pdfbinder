﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;

using Microsoft.Win32;

namespace PDFBinder
{
    [RunInstaller(true)]
    public class ShellInstaller : Installer
    {
        const string SHELL_LABEL = "Add to PDFBinder...";

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            AddShellExtension();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            RemoveShellExtension();
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            RemoveShellExtension();
        }

        private void AddShellExtension()
        {
            bool installed;
            using (var shell = OpenPdfShellKey(out installed))
            {
                if (shell != null && !installed)
                {
                    using (var binder = shell.CreateSubKey(SHELL_LABEL))
                    {
                        using (var command = binder.CreateSubKey("command"))
                        {
                            command.SetValue(null, string.Format("\"{0}.exe\" \"%1\"",
                                Path.Combine(Context.Parameters["pdfbinder"], typeof(ShellInstaller).Assembly.GetName().Name)));
                        }
                    }
                }
            }
        }

        private void RemoveShellExtension()
        {
            bool installed;
            using (var shell = OpenPdfShellKey(out installed))
            {
                if (installed)
                {
                    shell.DeleteSubKeyTree(SHELL_LABEL);
                }
            }
        }

        private RegistryKey OpenPdfShellKey(out bool installed)
        {
            installed = false;

            string pdfClientKey;
            using (var pdf = Registry.ClassesRoot.OpenSubKey(".pdf", false))
                pdfClientKey = pdf == null ? null : pdf.GetValue(null) as string;

            if (pdfClientKey == null)
                return null;

            var shell = Registry.ClassesRoot.OpenSubKey(pdfClientKey + "\\shell", true);

            if (shell == null)
                return null;

            using (var binder = shell.OpenSubKey(SHELL_LABEL))
                installed = (binder != null);

            return shell;
        }
    }
}
