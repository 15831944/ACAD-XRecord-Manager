using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACAD_XRecord_Manager
{
    public class XRecordManager
    {
        private string kCompanyDict = "";
        private string kApplicationDict = "";
        private string kXrecPrefix = "";

        public XRecordManager(string CompanyDictionaryName, string ApplicationDictionaryName, string XrecPrefix)
        {
            if (CompanyDictionaryName == "")
            {
                throw new Exception("\"CompanyDictionaryName\" - The parameter must be not empty.");
            }

            if (ApplicationDictionaryName == "")
            {
                throw new Exception("\"ApplicationDictionaryName\" - The parameter must be not empty.");
            }

            kCompanyDict = CompanyDictionaryName;
            kApplicationDict = ApplicationDictionaryName;
            kXrecPrefix = XrecPrefix;
        }

        // Helper function to get (optionally create)
        // the nested dictionary for our xrecord objects
        // This method is taken from Kean Walmsley example.
        // https://through-the-interface.typepad.com/through_the_interface/2006/11/linking_circles_1.html
        public ObjectId GetDictionaryId(Database db, bool createIfNotExisting)
        {
            ObjectId appDictId = ObjectId.Null;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                // Our outer level ("company") dictionary
                // does not exist
                if (!nod.Contains(kCompanyDict))
                {
                    if (!createIfNotExisting) return ObjectId.Null;

                    // Create both the "company" dictionary...
                    DBDictionary compDict = new DBDictionary();
                    nod.UpgradeOpen();
                    nod.SetAt(kCompanyDict, compDict);
                    tr.AddNewlyCreatedDBObject(compDict, true);

                    // ... and the inner "application" dictionary.
                    DBDictionary appDict = new DBDictionary();
                    appDictId = compDict.SetAt(kApplicationDict, appDict);
                    tr.AddNewlyCreatedDBObject(appDict, true);
                }
                else
                {
                    // Our "company" dictionary exists...
                    DBDictionary compDict = (DBDictionary)tr.GetObject(nod.GetAt(kCompanyDict), OpenMode.ForRead);
                    /// So check for our "application" dictionary
                    if (!compDict.Contains(kApplicationDict))
                    {
                        if (!createIfNotExisting) return ObjectId.Null;

                        // Create the "application" dictionary
                        DBDictionary appDict = new DBDictionary();
                        compDict.UpgradeOpen();
                        appDictId = compDict.SetAt(kApplicationDict, appDict);
                        tr.AddNewlyCreatedDBObject(appDict, true);
                    }
                    else
                    {
                        // Both dictionaries already exist...
                        appDictId = compDict.GetAt(kApplicationDict);
                    }
                }
                tr.Commit();
            }
            return appDictId;
        }

        /// <summary>
        /// Add an XRecord with list of values to the AutoCAD document.
        /// </summary>
        /// <param name="doc">AutoCAD document</param>
        /// <param name="valueList">List of values</param>
        /// <param name="recordName">Name of XRecord</param>
        public void AddXRecord(Document doc, List<TypedValue> valueList, string recordName)
        {
            // Add the prefix to the record name
            recordName = kXrecPrefix + recordName;

            ObjectId dictId = this.GetDictionaryId(doc.Database, true);

            Transaction acTrans = doc.TransactionManager.TopTransaction;

            bool useLocal = false;

            if (acTrans == null)
            {
                useLocal = true;

                acTrans = doc.Database.TransactionManager.StartTransaction();
            }

            DBDictionary dict = (DBDictionary)acTrans.GetObject(dictId, OpenMode.ForWrite);

            ResultBuffer rb = new ResultBuffer();

            foreach (TypedValue item in valueList)
            {
                rb.Add(item);
            }

            Xrecord xrec = new Xrecord();

            xrec.XlateReferences = true;

            xrec.Data = (ResultBuffer)rb;

            dict.SetAt(recordName, xrec);

            acTrans.AddNewlyCreatedDBObject(xrec, true);

            if (useLocal)
            {
                acTrans.Commit();

                acTrans.Dispose();
            }
        }

        /// <summary>
        /// Returns XRecord values.
        /// Returns null when XRecord does not contains in document or it empty.
        /// </summary>
        /// <param name="doc">AutoCAD document</param>
        /// <param name="recordName">Name of XRecord</param>
        /// <returns></returns>
        public List<TypedValue> GetXRecord(Document doc, string recordName)
        {
            recordName = kXrecPrefix + recordName;

            ObjectId dictId = this.GetDictionaryId(doc.Database, true);

            List<TypedValue> result = new List<TypedValue>();

            Transaction acTrans = doc.TransactionManager.TopTransaction;

            bool useLocal = false;

            if (acTrans == null)
            {
                useLocal = true;

                acTrans = doc.Database.TransactionManager.StartTransaction();
            }

            DBDictionary dict = (DBDictionary)acTrans.GetObject(dictId, OpenMode.ForWrite);

            if (!dict.Contains(recordName)) return null;

            ObjectId recId = dict.GetAt(recordName);

            DBObject obj = acTrans.GetObject(recId, OpenMode.ForRead);

            Xrecord xrec = obj as Xrecord;

            if (xrec == null)
            {
                return null;
            }

            foreach (TypedValue val in xrec.Data)
            {
                result.Add(val);
            }

            if (useLocal)
            {
                acTrans.Commit();

                acTrans.Dispose();
            }

            return result;
        }
    }

}
