using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools.Repl
{
    class VsScalaReplSite:IScalaReplSite
    {
        internal static VsScalaReplSite Site = new VsScalaReplSite();

        #region IScalaReplSite Members

        public CommonProjectNode GetStartupProject()
        {
            return ScalaPackage.GetStartupProject(ScalaPackage.Instance);
        }

        public bool TryGetStartupFileAndDirectory(out string fileName, out string directory)
        {
            return ScalaPackage.TryGetStartupFileAndDirectory(ScalaPackage.Instance, out fileName, out directory);
        }

        #endregion
    }
}
