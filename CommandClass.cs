using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACAD_XRecords_Manager
{
    /// <summary>
    /// AutoCAD creates a new object of the class every time a command is called from AutoCAD.
    /// </summary>
    public static class CommandClass
    {
        [CommandMethod("XRecTestWrite")]
        public static void Write()
        {
            ACADExtention ext = new ACADExtention();

            ext.WriteXRec();
        }

        [CommandMethod("XRecTestRead")]
        public static void Read()
        {
            ACADExtention ext = new ACADExtention();

            ext.ReadXRec();
        }
    }
}
