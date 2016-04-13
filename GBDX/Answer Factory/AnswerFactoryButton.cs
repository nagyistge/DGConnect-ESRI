using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Gbdx
{
    using ESRI.ArcGIS.esriSystem;

    public class AnswerFactoryButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public AnswerFactoryButton()
        {
        }

        protected override void OnClick()
        {
            UID theUid = new UIDClass();
            theUid.Value = ThisAddIn.IDs.Gbdx_Answer_Factory_AnswerFactoryDockableWindow;
            var window = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
            window.Show(!window.IsVisible());
        }

        protected override void OnUpdate()
        {
        }
    }
}
