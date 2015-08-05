/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.ScalaTools.Commands;
using Microsoft.ScalaTools.Repl;

namespace Microsoft.ScalaTools
{
    /// <summary>
    /// This class implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// This package is required if you want to define adds custom commands (ctmenu)
    /// or localized resources for the strings that appear in the New Project and Open Project dialogs.
    /// Creating project extensions or project types does not actually require a VSPackage.
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description("A custom project type based on CPS")]
    [Guid(ScalaPackage.PackageGuid)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ScalaPackage : CommonPackage
    {
        /// <summary>
        /// The GUID for this package.
        /// </summary>
        public const string PackageGuid = "6de842fa-c9e0-4a6a-b7df-f077ca3357fd";

        /// <summary>
        /// The GUID for this project type.  It is unique with the project file extension and
        /// appears under the VS registry hive's Projects key.
        /// </summary>
        public const string ProjectTypeGuid = "28fe952d-0ddf-4fb5-b5dd-be4f6f36ef2e";

        /// <summary>
        /// The file extension of this project type.  No preceding period.
        /// </summary>
        public const string ProjectExtension = "scalaproj";

        /// <summary>
        /// The default namespace this project compiles with, so that manifest
        /// resource names can be calculated for embedded resources.
        /// </summary>
        internal const string DefaultNamespace = "Microsoft.ScalaTools";

        internal static ScalaPackage Instance;
        
        public ScalaPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Debug.Assert(Instance == null, "ScalaPackage create multiple times");
            Instance = this;
        }

        public EnvDTE.DTE DTE { get { return (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE)); } }

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            var commands = new List<Command> { new OpenReplWindowCommand() };

            RegisterCommands(commands, Guids.ScalaCmdSet);
        }

        internal IReplWindow2 OpenReplWindow(bool focus = true)
        {
            var compModel = ComponentModel;
            var provider = compModel.GetService<IReplWindowProvider>();

            var window = (IReplWindow2)provider.FindReplWindow(ScalaReplEvaluatorProvider.ScalaReplId);

        }

        #endregion
    }
}
