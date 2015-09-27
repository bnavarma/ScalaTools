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
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem.Items;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudio.ComponentModelHost;

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
    //[ProvideKeyBindingTable(ReplWindow.TypeGuid, 200)]        // Resource ID: "Interactive Console"
    //[ProvideToolWindow(typeof(ReplWindow), Style = VsDockStyle.Linked, Orientation = ToolWindowOrientation.none, Window = ToolWindowGuids80.Outputwindow, MultiInstances = true)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.NoSolution)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.SolutionExists)]
    public sealed class ScalaPackage : Package, IVsToolWindowFactory
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

        private VisualStudio.Utilities.IContentType _contentType;

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
            Microsoft.ScalaTools.Commands.OpenReplWindowCommand.Initialize(this);
			 
            //var commands = new List<Command> { new OpenReplWindowCommand() };

            //RegisterCommands(commands, Guids.ScalaCmdSet);
        }

        public new IComponentModel ComponentModel
        {
            get { return this.GetComponentModel(); }
        }

        internal IReplWindow2 OpenReplWindow(bool focus = true)
        {
            var compModel = (IComponentModel)this.GetService(typeof(SComponentModel));
            var provider = compModel.GetService<IReplWindowProvider>();

            var window = (IReplWindow2)provider.FindReplWindow(ScalaReplEvaluatorProvider.ScalaReplId);
            if (window == null)
            {
                window = (IReplWindow2)provider.CreateReplWindow(
                    ReplContentType,
                    "Scala Interactive Window",
                    /*typeof(ScalaLanguageInfo).GUID*/Guid.Empty,
                    ScalaReplEvaluatorProvider.ScalaReplId
                    );
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)((ToolWindowPane)window).Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            if (focus)
            {
                window.Focus();
            }
            return window;
        }

        internal static bool TryGetStartupFileAndDirectory(IServiceProvider serviceProvider, out string fileName, out string directory)
        {
            //var startupProject = GetStartupProject(serviceProvider);
            //if(startupProject != null)
            //{
            //    fileName = startupProject.GetStartupFile();
            //    directory = startupProject.GetWorkingDirectory();
            //}
            //else
            //{
            //    var textView = CommonPackage.GetActiveTextView(serviceProvider);
            //    if(textView == null)
            //    {
            //        fileName = null;
            //        directory = null;
            //        return false;
            //    }
            //    fileName = textView.GetFilePath();
            //    directory = Path.GetDirectoryName(fileName);
            //}
            fileName = null;
            directory = null;
            return true;
        }

        private VisualStudio.Utilities.IContentType ReplContentType
        {
            get
            {
                if (_contentType == null)
                {
                    _contentType = ComponentModel.GetService<IContentTypeRegistryService>().GetContentType(ReplConstants.ReplContentTypeName);
                }
                return _contentType;
            }
        }

        #endregion

        int IVsToolWindowFactory.CreateToolWindow(ref Guid toolWindowType, uint id)
        {
            if (toolWindowType == typeof(ReplWindow).GUID)
            {
                var model = (IComponentModel)GetService(typeof(SComponentModel));
                var replProvider = (ReplWindowProvider)model.GetService<IReplWindowProvider>();

                return replProvider.CreateFromRegistry(model, (int)id) ? VSConstants.S_OK : VSConstants.E_FAIL;
            }

            return VSConstants.E_FAIL;
        }

        //internal override LibraryManager CreateLibraryManager(CommonPackage package)
        //{
        //    throw new NotImplementedException();
        //}

        //public override Type GetLibraryManagerType()
        //{
        //    throw new NotImplementedException();
        //}

        //public override bool IsRecognizedFile(string filename)
        //{
        //    var ext = Path.GetExtension(filename);
        //    return String.Equals(ext, ScalaConstants.ScalaExtenstion, StringComparison.OrdinalIgnoreCase);
        //}
    }
}
