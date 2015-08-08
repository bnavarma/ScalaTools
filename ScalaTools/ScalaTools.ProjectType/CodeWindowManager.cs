using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools
{
    class CodeWindowManager:IVsCodeWindowManager,IVsCodeWindowEvents
    {
        private readonly IVsEditorAdaptersFactoryService _adapterService;

        public CodeWindowManager(IVsEditorAdaptersFactoryService adapterService)
        {
            _adapterService = adapterService;
        }

        public int OnNewView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnCloseView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int AddAdornments()
        {
            return VSConstants.S_OK;
        }

        public int RemoveAdornments()
        {
            return VSConstants.S_OK;
        }
    }
}
