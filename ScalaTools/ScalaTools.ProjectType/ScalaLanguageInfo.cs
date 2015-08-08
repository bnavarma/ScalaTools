using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools
{
    [Guid(Guids.ScalaLanguageInfoString)]
    internal sealed class ScalaLanguageInfo:IVsLanguageInfo,IVsLanguageDebugInfo
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IComponentModel _componentModel;

        public ScalaLanguageInfo(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            ppCodeWinMgr = new CodeWindowManager(_componentModel.GetService<IVsEditorAdaptersFactoryService>());
            return VSConstants.S_OK;
        }

        public int GetFileExtensions(out string pbstrExtenstions)
        {
            pbstrExtenstions = ScalaConstants.ScalaExtenstion;
            return VSConstants.S_OK;
        }

        public int GetLanguageName(out string bstrName)
        {
            bstrName = ScalaConstants.Scala;
            return VSConstants.S_OK;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_FAIL;
        }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public int GetLanguageID(IVsTextBuffer pBuffer, int iLine, int iCol, out Guid pguidLanguageID)
        {
            pguidLanguageID = Guids.ScalaDebugLanguage;
            return VSConstants.S_OK;
        }

        public int GetLocationOfName(string pszName, out string pbstrMkDoc, TextSpan[] pspanLocation)
        {
            pbstrMkDoc = null;
            return VSConstants.E_FAIL;
        }

        public int GetNameOfLocation(IVsTextBuffer pBuffer, int iLine,int iCol,out string pbstrName,out int piLineOffset)
        {
            pbstrName = null;
            piLineOffset = 0;
            return VSConstants.E_FAIL;
        }

        public int GetProximityExpressions(IVsTextBuffer pBuffer,int iLine,int iCol,int cLines, out IVsEnumBSTR ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_FAIL;
        }

        public int IsMappedLocation(IVsTextBuffer pBuffer, int iLine,int iCol)
        {
            return VSConstants.E_FAIL;
        }

        public int ResolveName(string pszName, uint dwFlags, out IVsEnumDebugName ppNames)
        {
            ppNames = null;
            return VSConstants.E_FAIL;
        }

        public int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol,TextSpan[] pCodeSpan)
        {
            pCodeSpan[0].iStartIndex = iLine;
            pCodeSpan[0].iEndLine = iLine;
            return VSConstants.E_NOTIMPL;
        }
    }
}
