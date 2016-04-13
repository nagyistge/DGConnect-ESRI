using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Gbdx
{
    using ESRI.ArcGIS.esriSystem;

    public class AnswerFactoryCreateProject : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public AnswerFactoryCreateProject()
        {
        }

        protected override void OnClick()
        {
                UID theUid = new UIDClass();
                theUid.Value = ThisAddIn.IDs.AnswerFactoryProjects;
                var window = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
                window.Show(!window.IsVisible());
        }

        protected override void OnUpdate()
        {
        }
    }
}
