using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools.Repl
{
    interface IScalaReplSite
    {
        CommonProjectNode GetStartupProject();
        bool TryGetStartupFileAndDirectory(out string fileName, out string directory);
    }
}
