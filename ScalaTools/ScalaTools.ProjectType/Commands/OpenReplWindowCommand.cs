using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools;

namespace Microsoft.ScalaTools.Commands
{
    internal sealed class OpenReplWindowCommand :Command
    {
        public override void DoCommand(object sender, EventArgs args)
        {
            ScalaPackage.Instance.OpenReplWindow();
        }

        public override int CommandId
        {
            get
            {
                return (int)PkgCmdId.cmdidReplWindow;
            }
        }
    }
}
