using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace ACAD_XRecord_Manager.Tests
{
    public class ACADExtention : IExtensionApplication
    {
        XRecordManager XMan;

        private static string XRecName = "Test1";

        public ACADExtention()
        {
            XMan = new XRecordManager("KITNG_Dev", "CommonStorage", "");
        }

        public void Initialize()
        { }

        public void Terminate()
        { }

        public void WriteXRec()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            PromptResult res = doc.Editor.GetString("Write text for XRecord:");

            if (res.Status != PromptStatus.OK) return;

            string xRecText = res.StringResult;

            XMan.AddXRecord(doc,new List<TypedValue>() {new TypedValue((int)DxfCode.Text, xRecText)},XRecName);
        }

        public void ReadXRec()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            List<TypedValue> result = XMan.GetXRecord(doc, XRecName);

            if (result == null)
                doc.Editor.WriteMessage("\nThis document does not contains a XRecord with name {0}.\n", XRecName);

            int i = 0;

            foreach (TypedValue val in result)
            {
                doc.Editor.WriteMessage("\nRecord \"{0}\" value number {1}:{2}.\n", XRecName, i, val.Value.ToString());

                i++;
            }
        }
    }
}
