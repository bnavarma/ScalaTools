using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools.Repl
{
    [Export(typeof(IReplEvaluatorProvider))]
    class ScalaReplEvaluatorProvider:IReplEvaluatorProvider
    {
        internal const string ScalaReplId = "{F1369600-956D-4654-A422-5585407F9295}";

        #region IAltReplEvaluatorProvider Members

        public IReplEvaluator GetEvaluator(string replId)
        {
            if(replId == ScalaReplId)
            {
                return new ScalaReplEvaluator();
            }
            return null;
        }

        #endregion
    }
}
