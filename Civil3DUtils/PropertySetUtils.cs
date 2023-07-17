using AutoCADUtils.Utils;
using Autodesk.Aec.PropertyData.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public class PropertySetUtils
    {
        public static string GetValueOfProperty(string propertySetName, string propertyName, DBObject obj)
        {
            PropertySetDefinition propSetDef = null;
            //берем property set и property set definition
            var propSet = GetPropertySet(propertySetName, obj, ref propSetDef);
            //проверяем значения на пустоту
            string propValueFound = "";
            if (propSet == null || propSetDef == null)
                return propValueFound;
            //затем ищем среди всех свойств нужное
            foreach (PropertyDefinition propDef in propSetDef.Definitions)
            {
                //если заданное имя не совпадает, то идём в следующую итерацию
                if (!propDef.Name.Equals(propertyName))
                    continue;
                //находим значение свойства
                var propId = propSet.PropertyNameToId(propDef.Name);
                var propValue = propSet.GetAt(propId);
                if (propValue != null)
                    propValueFound = propValue.ToString();
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return propValueFound;
        }


        public static string GetValueOfProperty(PropertySetDefinition propertySetDefinition,
                                                PropertyDefinition propertyDefinition, DBObject obj)
        {
            //берем property set и property set definition
            string propValueFound = "";
            if (propertySetDefinition == null || propertyDefinition == null)
                return propValueFound;

            PropertySet propSet = GetPropertySet(propertySetDefinition, obj);

            //затем ищем среди всех свойств нужное
            foreach (PropertyDefinition propDef in propertySetDefinition.Definitions)
            {
                //находим значение свойства
                var propId = propSet.PropertyNameToId(propDef.Name);
                var propValue = propSet.GetAt(propId);
                if (propValue != null)
                    propValueFound = propValue.ToString();
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return propValueFound;
        }

        public static object GetPropertyValue(PropertySetDefinition propertySetDefinition,
                                                PropertyDefinition propertyDefinition, DBObject obj)
        {
            object value = null;

            //берем property set и property set definition
            if (propertySetDefinition == null || propertyDefinition == null)
                return value;

            PropertySet propSet = GetPropertySet(propertySetDefinition, obj);
            if (propSet == null)
                return value;
            //затем ищем среди всех свойств нужное

            foreach (PropertyDefinition currentPropDef in propertySetDefinition.Definitions)
            {
                if (currentPropDef != propertyDefinition)
                    continue;

                //находим значение свойства
                int propId = propSet.PropertyNameToId(currentPropDef.Name);
                object propValue = propSet.GetAt(propId);
                if (propValue != null)
                    value = propValue.ToString();
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return value;
        }


        public static string GetPropertyValueString(PropertySetDefinition propertySetDefinition,
                                                PropertyDefinition propertyDefinition, DBObject obj)
        {
            string value = string.Empty;

            //берем property set и property set definition
            if (propertySetDefinition == null || propertyDefinition == null)
                return value;

            PropertySet propSet = GetPropertySet(propertySetDefinition, obj);
            if (propSet == null)
                return value;
            //затем ищем среди всех свойств нужное

            foreach (PropertyDefinition currentPropDef in propertySetDefinition.Definitions)
            {
                if (currentPropDef != propertyDefinition)
                    continue;

                //находим значение свойства
                int propId = propSet.PropertyNameToId(currentPropDef.Name);
                object propValue = propSet.GetAt(propId);
                if (propValue != null)
                    value = propValue.ToString();
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return value;
        }

        public static string GetPropertyValueString(string propertySetName,
                                                    string propertyName, DBObject dbObject)
        {
            string value = string.Empty;
            if (string.IsNullOrEmpty(propertySetName) ||
                string.IsNullOrEmpty(propertyName))
                return value;

            PropertyDefinition propDef = null;
            PropertySetDefinition propSetDef = null;

            GetPropertySetAndPropertyDefinitionByName(OpenMode.ForRead, propertySetName, propertyName,
                                                      ref propSetDef, ref propDef);
            PropertySet propSet = GetPropertySet(propSetDef, dbObject);

            if (propSet == null)
                return value;
            //затем ищем среди всех свойств нужное

            foreach (PropertyDefinition currentPropDef in propSetDef.Definitions)
            {
                if (currentPropDef != propDef)
                    continue;

                //находим значение свойства
                int propId = propSet.PropertyNameToId(currentPropDef.Name);
                object propValue = propSet.GetAt(propId);
                if (propValue != null)
                    value = propValue.ToString();
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return value;
        }

        public static double GetPropertyValueDouble(string propertySetName,
                                                    string propertyName, DBObject dbObject)
        {
            double value = 0;
            if (string.IsNullOrEmpty(propertySetName) ||
                string.IsNullOrEmpty(propertyName))
                return value;

            PropertyDefinition propDef = null;
            PropertySetDefinition propSetDef = null;

            GetPropertySetAndPropertyDefinitionByName(OpenMode.ForRead, propertySetName, propertyName,
                                                      ref propSetDef, ref propDef);
            PropertySet propSet = GetPropertySet(propSetDef, dbObject);

            if (propSet == null)
                return value;
            //затем ищем среди всех свойств нужное

            foreach (PropertyDefinition currentPropDef in propSetDef.Definitions)
            {
                if (currentPropDef != propDef)
                    continue;

                //находим значение свойства
                int propId = propSet.PropertyNameToId(currentPropDef.Name);
                object propValue = propSet.GetAt(propId);
                if (propValue != null)
                    value = (double)propValue;
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return value;
        }

        public static bool HasValueOfPropertySetAndProperty(DBObject dbObject, PropertySetDefinition propertySetDefinition,
                                                    PropertyDefinition propertyDefinition)
        {
            if (dbObject == null || propertySetDefinition == null ||
                propertyDefinition == null)
                return false;
            string propValue = GetValueOfProperty(propertySetDefinition, propertyDefinition, dbObject);
            if (string.IsNullOrEmpty(propValue))
                return false;

            return true;
        }

        public static string SetValueToProperty(string propertySetName, string propertyName, DBObject obj, string value)
        {
            PropertySetDefinition propSetDef = null;
            //берем property set и property set definition
            PropertySet propSet = GetPropertySet(propertySetName, obj, ref propSetDef);
            //проверяем значения на пустоту
            string propValueFound = "";
            if (propSet == null || propSetDef == null)
            {
                PropertyDataServices.AddPropertySet(obj, propSetDef.Id);
                return propValueFound;
            }

            //затем ищем среди всех свойств нужное
            foreach (PropertyDefinition propDef in propSetDef.Definitions)
            {
                //если заданное имя не совпадает, то идём в следующую итерацию
                if (!propDef.Name.Equals(propertyName))
                    continue;
                //находим значение свойства

                int propId = propSet.PropertyNameToId(propDef.Name);

                try
                {
                    propSet.SetAt(propId, value);
                }
                catch (Exception)
                {
                    continue;
                }
                //когда нашли свойство, то выходим из цикла
            }

            return propValueFound;
        }


        public static string SetValueToProperty(string propertySetName, string propertyName, DBObject obj, double value)
        {
            PropertySetDefinition propSetDef = null;
            //берем property set и property set definition
            var propSet = GetPropertySet(propertySetName, obj, ref propSetDef);
            //проверяем значения на пустоту
            string propValueFound = "";
            if (propSet == null || propSetDef == null)
                return propValueFound;
            //затем ищем среди всех свойств нужное
            foreach (PropertyDefinition propDef in propSetDef.Definitions)
            {
                //если заданное имя не совпадает, то идём в следующую итерацию
                if (!propDef.Name.Equals(propertyName))
                    continue;
                //находим значение свойства
                int propId = -1;
                propId = propSet.PropertyNameToId(propDef.Name);

                try
                {
                    propSet.SetAt(propId, value);
                }
                catch (Exception)
                {
                    continue;
                }

                //когда нашли свойство, то выходим из цикла
            }

            return propValueFound;
        }

        public static void SetValueToProperty(PropertySetDefinition propertySetDefinition,
                                PropertyDefinition propertyDefinition,
                                DBObject obj, string value)
        {

            using (Transaction transaction = CivilDocumentService.TransactionManager.StartTransaction())
            {
                if (!obj.IsWriteEnabled)
                {
                    obj = transaction.GetObject(obj.Id, OpenMode.ForWrite, false, true) as DBObject;
                }

                //берем property set и property set definition
                //проверяем значения на пустоту
                if (propertySetDefinition == null || propertyDefinition == null)
                {
                    transaction.Commit();
                    return;
                }
                //затем ищем среди всех свойств нужное
                PropertySet propSet = GetPropertySet(propertySetDefinition, obj);
                if (propSet == null)
                {
                    PropertyDataServices.AddPropertySet(obj, propertySetDefinition.Id);
                    propSet = GetPropertySet(propertySetDefinition, obj);
                }

                int propId = propSet.PropertyNameToId(propertyDefinition.Name);
                try
                {
                    propSet.SetAt(propId, value);
                }
                catch (Exception)
                { }
                transaction.Commit();
            }
        }

        public static void SetValueToProperty(PropertySetDefinition propertySetDefinition,
                                PropertyDefinition propertyDefinition,
                                DBObject obj, double value)
        {

            using (Transaction transaction = CivilDocumentService.TransactionManager.StartTransaction())
            {
                if (!obj.IsWriteEnabled)
                {
                    obj = transaction.GetObject(obj.Id, OpenMode.ForWrite, false, true) as DBObject;
                }

                //берем property set и property set definition
                //проверяем значения на пустоту
                if (propertySetDefinition == null || propertyDefinition == null)
                {
                    transaction.Commit();
                    return;
                }
                //затем ищем среди всех свойств нужное
                PropertySet propSet = GetPropertySet(propertySetDefinition, obj);
                if (propSet == null)
                {
                    PropertyDataServices.AddPropertySet(obj, propertySetDefinition.Id);
                    propSet = GetPropertySet(propertySetDefinition, obj);
                }

                int propId = propSet.PropertyNameToId(propertyDefinition.Name);
                try
                {
                    propSet.SetAt(propId, value);
                }
                catch (Exception)
                { }
                transaction.Commit();
            }
        }

        public static void SetValueToProperty(PropertySetDefinition propertySetDefinition,
                                PropertyDefinition propertyDefinition,
                                DBObject obj, int value)
        {

            using (Transaction transaction = CivilDocumentService.TransactionManager.StartTransaction())
            {
                if (!obj.IsWriteEnabled)
                {
                    obj = transaction.GetObject(obj.Id, OpenMode.ForWrite, false, true) as DBObject;
                }

                //берем property set и property set definition
                //проверяем значения на пустоту
                if (propertySetDefinition == null || propertyDefinition == null)
                {
                    transaction.Commit();
                    return;
                }
                //затем ищем среди всех свойств нужное
                PropertySet propSet = GetPropertySet(propertySetDefinition, obj);
                if (propSet == null)
                {
                    PropertyDataServices.AddPropertySet(obj, propertySetDefinition.Id);
                    propSet = GetPropertySet(propertySetDefinition, obj);
                }

                int propId = propSet.PropertyNameToId(propertyDefinition.Name);
                try
                {
                    propSet.SetAt(propId, value);
                }
                catch (Exception)
                { }
                transaction.Commit();
            }
        }

        public static int SetValueToProperty(string propertySetName, string propertyName, DBObject obj, int value)
        {
            PropertySetDefinition propSetDef = null;
            //берем property set и property set definition
            var propSet = GetPropertySet(propertySetName, obj, ref propSetDef);
            //проверяем значения на пустоту
            int propValueFound = 0;
            if (propSet == null || propSetDef == null)
                return propValueFound;
            //затем ищем среди всех свойств нужное
            foreach (PropertyDefinition propDef in propSetDef.Definitions)
            {
                //если заданное имя не совпадает, то идём в следующую итерацию
                if (!propDef.Name.Equals(propertyName))
                    continue;
                //находим значение свойства

                var propId = propSet.PropertyNameToId(propDef.Name);
                try
                {
                    propSet.SetAt(propId, value);
                }
                catch (Exception ex)
                {
                    continue;
                }
                //когда нашли свойство, то выходим из цикла
            }

            return propValueFound;
        }

        public static double GetDoubleValueOfProperty(string propertySetName, string propertyName, DBObject obj)
        {
            PropertySetDefinition propSetDef = null;
            //берем property set и property set definition
            var propSet = GetPropertySet(propertySetName, obj, ref propSetDef);
            //проверяем значения на пустоту
            double propValueFound = 0;
            if (propSet == null || propSetDef == null)
                return propValueFound;
            //затем ищем среди всех свойств нужное
            foreach (PropertyDefinition propDef in propSetDef.Definitions)
            {
                //если заданное имя не совпадает, то идём в следующую итерацию
                if (!propDef.Name.Equals(propertyName))
                    continue;
                //находим значение свойства
                var propId = propSet.PropertyNameToId(propDef.Name);
                var propValue = propSet.GetAt(propId);
                if (propValue != null)
                    propValueFound = (double)propValue;
                //когда нашли свойство, то выходим из цикла
                break;
            }
            return propValueFound;
        }


        public static PropertySet GetPropertySet(
        PropertySetDefinition propSetDef,
            DBObject obj)
        {
            var db = HostApplicationServices.WorkingDatabase;
            PropertySet propSet = null;
            if (propSetDef == null)
                return null;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                ObjectId propSetId = ObjectId.Null;
                //открываем определение property set
                try
                {
                    var propSets = PropertyDataServices.GetPropertySets(obj);
                    propSetId = PropertyDataServices.GetPropertySet(obj, propSetDef.Id);
                }
                catch (Exception ex)
                {
                    return null;
                }

                if (propSetId == ObjectId.Null)
                    return null;

                propSet = ts.GetObject(propSetId, OpenMode.ForWrite) as PropertySet;
                ts.Commit();
            }
            return propSet;
        }

        public static void RemovePropertySet(
            PropertySetDefinition propSetDef,
            DBObject obj)
        {
            var db = HostApplicationServices.WorkingDatabase;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                ObjectId propSetId = ObjectId.Null;
                //открываем определение property set
                try
                {
                    if (!obj.IsWriteEnabled)
                        obj = ts.GetObject(obj.Id, OpenMode.ForWrite, false, true) as DBObject;
                    PropertyDataServices.RemovePropertySet(obj, propSetDef.Id);
                }
                catch (Exception)
                {
                    return;
                }
                ts.Commit();
            }
            return;
        }

        public static List<DBObject> GetDBObjectsByPropertySetAndProperty(
            PropertySetDefinition propertySetDefinition, PropertyDefinition propertyDefinition, string propertyValueFromUser, OpenMode openMode)
        {
            List<DBObject> matchedObjects = new List<DBObject>();

            List<DBObject> dbObjects = new List<DBObject>();
            foreach (string objectClassName in propertySetDefinition.AppliesToFilter)
            {
                dbObjects.AddRange(DbObjectUtils.GetObjectsFromModel(objectClassName, openMode));
            }

            foreach (DBObject dbObject in dbObjects)
            {
                string currentPropValule = GetPropertyValueString(propertySetDefinition, propertyDefinition, dbObject);
                if (currentPropValule.Equals(propertyValueFromUser))
                {
                    matchedObjects.Add(dbObject);
                }
            }
            return matchedObjects;
        }

        public static List<DBObject> GetDBObjectsByPropertySetAndProperty(List<DBObject> dBObjects,
           PropertySetDefinition propertySetDefinition, PropertyDefinition propertyDefinition, string propertyValueFromUser)
        {
            List<DBObject> matchedObjects = new List<DBObject>();

            foreach (DBObject dbObject in dBObjects)
            {
                string currentPropValule = GetPropertyValueString(propertySetDefinition, propertyDefinition, dbObject);
                if (currentPropValule.Equals(propertyValueFromUser))
                {
                    matchedObjects.Add(dbObject);
                }
            }
            return matchedObjects;
        }

        public static List<DBObject> GetDBObjectsByPropertySetAndPropertyContains(
           PropertySetDefinition propertySetDefinition, PropertyDefinition propertyDefinition, string propertyValueFromUser, OpenMode openMode)
        {
            List<DBObject> matchedObjects = new List<DBObject>();

            List<DBObject> dbObjects = new List<DBObject>();
            foreach (string objectClassName in propertySetDefinition.AppliesToFilter)
            {
                dbObjects.AddRange(DbObjectUtils.GetObjectsFromModel(objectClassName, openMode));
            }

            foreach (DBObject dbObject in dbObjects)
            {
                string currentPropValule = GetPropertyValueString(propertySetDefinition, propertyDefinition, dbObject);
                if (currentPropValule.Contains(propertyValueFromUser))
                {
                    matchedObjects.Add(dbObject);
                }
            }
            return matchedObjects;
        }




        public static List<DBObject> GetDBObjectsByPropertySetAppliedClasses(
            PropertySetDefinition propertySetDefinition, OpenMode openMode)
        {
            List<DBObject> dbObjects = new List<DBObject>();
            foreach (string objectClassName in propertySetDefinition.AppliesToFilter)
            {
                dbObjects.AddRange(DbObjectUtils.GetObjectsFromModel(objectClassName, openMode));
            }

            return dbObjects;
        }


        public static List<string> GetAppliedObjectClassNamesOfPropertySet(
                                        PropertySetDefinition propertySetDefinition)
        {
            var objectClassNames = new List<string>();
            foreach (var objectClassName in propertySetDefinition.AppliesToFilter)
            {
                objectClassNames.Add(objectClassName);
            }
            return objectClassNames;
        }

        public static List<DBObject> GetDBObjectsByPropertySetAndPropertyName(
            string propertySetName, string propertyName, string propertyValueFromUser, OpenMode openMode)
        {
            var propertySetAndPropertyDefinitions = PropertySetUtils.GetAllPropertySetAndPropertyDefinitions(OpenMode.ForRead);

            List<DBObject> matchedObjects = new List<DBObject>();

            foreach (var propSetDict in propertySetAndPropertyDefinitions)
            {
                if (!propSetDict.Key.Name.Equals(propertySetName))
                    continue;

                bool isPropNameAndValueMatched = false;
                PropertyDefinition matchedPropDef = null;
                foreach (PropertyDefinition propDef in propSetDict.Value)
                {
                    if (propDef.Name.Equals(propertyName))
                    {
                        isPropNameAndValueMatched = true;
                        matchedPropDef = propDef;
                        break;
                    }
                }
                if (!isPropNameAndValueMatched)
                    continue;

                List<DBObject> dbObjects = new List<DBObject>();
                foreach (string objectClassName in propSetDict.Key.AppliesToFilter)
                {
                    dbObjects.AddRange(DbObjectUtils.GetObjectsFromModel(objectClassName, openMode));
                }

                foreach (DBObject dbObject in dbObjects)
                {
                    string currentPropValule = GetPropertyValueString(propSetDict.Key, matchedPropDef, dbObject);
                    if (currentPropValule.Equals(propertyValueFromUser))
                    {
                        matchedObjects.Add(dbObject);
                    }
                }

            }

            return matchedObjects;
        }

        public static List<DBObject> GetDBObjectsByPropertySetAndPropertyName(
            string propertySetName, string propertyName, OpenMode openMode)
        {
            var propertySetAndPropertyDefinitions = PropertySetUtils.GetAllPropertySetAndPropertyDefinitions(OpenMode.ForRead);

            List<DBObject> matchedObjects = new List<DBObject>();

            foreach (var propSetDict in propertySetAndPropertyDefinitions)
            {
                if (!propSetDict.Key.Name.Equals(propertySetName))
                    continue;

                bool isPropNameAndValueMatched = false;
                PropertyDefinition matchedPropDef = null;
                foreach (PropertyDefinition propDef in propSetDict.Value)
                {
                    if (propDef.Name.Equals(propertyName))
                    {
                        isPropNameAndValueMatched = true;
                        matchedPropDef = propDef;
                        break;
                    }
                }
                if (!isPropNameAndValueMatched)
                    continue;

                List<DBObject> dbObjects = new List<DBObject>();
                foreach (string objectClassName in propSetDict.Key.AppliesToFilter)
                {
                    dbObjects.AddRange(DbObjectUtils.GetObjectsFromModel(objectClassName, openMode));
                }

                foreach (DBObject dbObject in dbObjects)
                {
                    string currentPropValule = GetPropertyValueString(propSetDict.Key, matchedPropDef, dbObject);
                    if (currentPropValule.Count() > 0)
                    {
                        matchedObjects.Add(dbObject);
                    }
                }

            }

            return matchedObjects;
        }


        public static PropertySet GetPropertySet(
            string propertySetName,
            DBObject obj, ref PropertySetDefinition propSetDef)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            var dictPropSetDef = new DictionaryPropertySetDefinitions(db);
            ObjectId propSetDefId = ObjectId.Null;
            try
            {
                propSetDefId = dictPropSetDef.GetAt(propertySetName);

            }
            catch (Exception)
            {
                return null;
            }

            PropertySet propSet = null;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                ObjectId propSetId = ObjectId.Null;
                //открываем определение property set
                propSetDef = ts.GetObject(propSetDefId, OpenMode.ForWrite) as PropertySetDefinition;
                try
                {
                    propSetId = PropertyDataServices.GetPropertySet(obj, propSetDefId);
                }
                catch (Exception ex)
                {
                    return null;
                }

                if (propSetId == ObjectId.Null)
                    return null;

                //открываем сам propserty set (там содержатся все нужные свойства)
                propSet = ts.GetObject(propSetId, OpenMode.ForWrite) as PropertySet;
                ts.Commit();
            }
            return propSet;
        }

        public static PropertySet GetPropertySet(string propertySetName, DBObject obj)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            var dictPropSetDef = new DictionaryPropertySetDefinitions(db);
            var propSetDefId = dictPropSetDef.GetAt(propertySetName);
            PropertySet propSet = null;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                ObjectId propSetId = ObjectId.Null;
                //открываем определение property set
                PropertySetDefinition propSetDef = ts.GetObject(propSetDefId, OpenMode.ForWrite) as PropertySetDefinition;
                try
                {
                    propSetId = PropertyDataServices.GetPropertySet(obj, propSetDefId);
                }
                catch (Exception ex)
                {
                    return null;
                }

                if (propSetId == ObjectId.Null)
                    return null;

                //открываем сам propserty set (там содержатся все нужные свойства)
                propSet = ts.GetObject(propSetId, OpenMode.ForWrite) as PropertySet;
                ts.Commit();
            }
            return propSet;
        }

        public static PropertyDefinition CreatePropertyDefinition(
            PropertySetDefinition propertySetDefinition,
            string propertyName,
            Autodesk.Aec.PropertyData.DataType dataType)
        {
            var db = HostApplicationServices.WorkingDatabase;
            PropertyDefinition propDefManual = null;
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                propDefManual = new PropertyDefinition();
                propDefManual.SetToStandard(db);
                propDefManual.SubSetDatabaseDefaults(db);
                propDefManual.Name = propertyName;
                propDefManual.DataType = dataType;
                if (!propertySetDefinition.IsWriteEnabled)
                    propertySetDefinition = ts.GetObject(propertySetDefinition.Id,
                                                    OpenMode.ForWrite, false, true) as PropertySetDefinition;

                foreach (PropertyDefinition definition in propertySetDefinition.Definitions)
                {
                    if (definition.Name.Equals(propertyName))
                    {
                        ts.Commit();
                        return definition;
                    }
                }
                try
                {
                    propertySetDefinition.Definitions.Add(propDefManual);
                }
                catch (Exception) { }
                ts.Commit();
            }

            return propDefManual;
        }

        public static PropertySetDefinition CreatePropertySetDefinition(string propertySetName, List<string> classNames)
        {
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            Database database = HostApplicationServices.WorkingDatabase;

            PropertySetDefinition propSetDef = GetAllPropertySetDefinitions(OpenMode.ForWrite).FirstOrDefault(g => g.Name.Equals(propertySetName));
            if (propSetDef == null)
            {
                using (Transaction ts = database.TransactionManager.StartTransaction())
                {
                    DictionaryPropertySetDefinitions dictPropSetDef = new DictionaryPropertySetDefinitions(database);
                    if (!dictPropSetDef.Has(propertySetName, ts))
                    {
                        propSetDef = new PropertySetDefinition();
                        propSetDef.SetToStandard(database);
                        propSetDef.SubSetDatabaseDefaults(database);
                        StringCollection applliedTo = new StringCollection();
                        foreach (string className in classNames)
                        {
                            applliedTo.Add(className);
                        }
                        propSetDef.SetAppliesToFilter(applliedTo, byStyle: false);
                        dictPropSetDef.AddNewRecord(propertySetName, propSetDef);
                        ts.AddNewlyCreatedDBObject(propSetDef, true);
                    }
                    ts.Commit();
                }
            }
            return propSetDef;
        }

        public static List<PropertySetDefinition> GetAllPropertySetDefinitions(OpenMode openMode)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            DictionaryPropertySetDefinitions dictPropSetDef = new DictionaryPropertySetDefinitions(db);

            List<PropertySetDefinition> propertySetDefinitions = new List<PropertySetDefinition>();

            using (CivilDocumentService.Document.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < dictPropSetDef.Records.Count; i++)
                    {
                        var propSetDefId = dictPropSetDef.Records[i];
                        if (propSetDefId.IsNull)
                            continue;
                        PropertySetDefinition propSetDef = ts.GetObject(propSetDefId, openMode) as PropertySetDefinition;
                        if (propSetDef == null)
                        {
                            continue;
                        }

                        propertySetDefinitions.Add(propSetDef);
                    }
                    ts.Commit();
                }
            }

            return propertySetDefinitions;
        }

        public static List<PropertyDefinition> GetAllPropertyDefinitions(OpenMode openMode)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            DictionaryPropertySetDefinitions dictPropSetDef = new DictionaryPropertySetDefinitions(db);
            //ObjectId propSetDefId = dictPropSetDef.GetAt(propertySetName);
            //PropertySet propSet = null;
            List<PropertyDefinition> propDefList = new List<PropertyDefinition>();
            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < dictPropSetDef.Records.Count; i++)
                {
                    var propSetDefId = dictPropSetDef.Records[i];
                    if (propSetDefId.IsNull)
                        continue;

                    //открываем определение property set
                    PropertySetDefinition propSetDef = ts.GetObject(propSetDefId, openMode) as PropertySetDefinition;
                    foreach (PropertyDefinition propDef in propSetDef.Definitions)
                    {
                        propDefList.Add(propDef);
                    }
                }
                ts.Commit();
            }

            return propDefList;
        }


        public static PropertySetDefinition GetPropertySetDefinitionByName(OpenMode openMode, string propSetName)
        {
            var db = HostApplicationServices.WorkingDatabase;
            DictionaryPropertySetDefinitions dictPropSetDef = new DictionaryPropertySetDefinitions(db);
            Dictionary<PropertySetDefinition, List<PropertyDefinition>> propDefDict =
                            new Dictionary<PropertySetDefinition, List<PropertyDefinition>>();

            using (CivilDocumentService.Document.LockDocument())
            {
                using (Transaction ts = db.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < dictPropSetDef.Records.Count; i++)
                    {
                        var propSetDefId = dictPropSetDef.Records[i];
                        if (propSetDefId.IsNull)
                            continue;

                        //открываем определение property set
                        PropertySetDefinition propSetDef = ts.GetObject(propSetDefId, openMode) as PropertySetDefinition;
                        if (propSetDef == null)
                            continue;

                        if (propSetDef.Name.Equals(propSetName))
                        {
                            ts.Commit();
                            return propSetDef;
                        }


                    }
                    ts.Commit();
                }
            }

            return null;
        }

        public static void GetPropertySetAndPropertyDefinitionByName(OpenMode openMode,
            string propSetName, string propName,
            ref PropertySetDefinition propSetDefFounded, ref PropertyDefinition propDefFounded)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            DictionaryPropertySetDefinitions dictPropSetDef = new DictionaryPropertySetDefinitions(db);
            Dictionary<PropertySetDefinition, List<PropertyDefinition>> propDefDict =
                            new Dictionary<PropertySetDefinition, List<PropertyDefinition>>();

            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < dictPropSetDef.Records.Count; i++)
                {
                    var propSetDefId = dictPropSetDef.Records[i];
                    if (propSetDefId.IsNull)
                        continue;

                    //открываем определение property set
                    PropertySetDefinition propSetDef = ts.GetObject(propSetDefId, openMode) as PropertySetDefinition;
                    if (!propSetDef.Name.Equals(propSetName))
                        continue;

                    if (propSetDef == null)
                        continue;

                    propSetDefFounded = propSetDef;

                    foreach (PropertyDefinition propDef in propSetDef.Definitions)
                    {
                        if (propDef == null)
                            continue;

                        if (!propDef.Name.Equals(propName))
                            continue;


                        propDefFounded = propDef;

                        if (propDefDict.ContainsKey(propSetDef))
                        {
                            propDefDict[propSetDef].Add(propDef);
                        }
                        else
                        {
                            propDefDict[propSetDef] = new List<PropertyDefinition>();
                            propDefDict[propSetDef].Add(propDef);
                        }
                    }
                }
                ts.Commit();
            }
        }

        public static Dictionary<PropertySetDefinition, List<PropertyDefinition>>
            GetAllPropertySetAndPropertyDefinitions(OpenMode openMode)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            DictionaryPropertySetDefinitions dictPropSetDef = new DictionaryPropertySetDefinitions(db);
            Dictionary<PropertySetDefinition, List<PropertyDefinition>> propDefDict =
                            new Dictionary<PropertySetDefinition, List<PropertyDefinition>>();

            using (Transaction ts = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < dictPropSetDef.Records.Count; i++)
                {
                    var propSetDefId = dictPropSetDef.Records[i];
                    if (propSetDefId.IsNull)
                        continue;

                    //открываем определение property set
                    PropertySetDefinition propSetDef = ts.GetObject(propSetDefId, openMode) as PropertySetDefinition;
                    if (propSetDef == null)
                        continue;
                    foreach (PropertyDefinition propDef in propSetDef.Definitions)
                    {
                        if (propDef == null)
                            continue;

                        if (propDefDict.ContainsKey(propSetDef))
                        {
                            propDefDict[propSetDef].Add(propDef);
                        }
                        else
                        {
                            propDefDict[propSetDef] = new List<PropertyDefinition>();
                            propDefDict[propSetDef].Add(propDef);
                        }
                    }
                }
                ts.Commit();
            }

            return propDefDict;
        }


        public static List<PropertyDefinition> GetAllPropertyDefinitionsOfPropertySet(
            PropertySetDefinition propertySet)
        {
            if (propertySet == null)
                return new List<PropertyDefinition>();
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var db = HostApplicationServices.WorkingDatabase;
            List<PropertyDefinition> propDefList = new List<PropertyDefinition>();

            //открываем определение property set
            foreach (PropertyDefinition propDef in propertySet.Definitions)
            {
                if (propDef == null)
                    continue;

                propDefList.Add(propDef);
            }

            return propDefList;
        }

        public static PropertyDefinition GetPropertyDefinitionOfPropertySetByName(
            PropertySetDefinition propertySet, string propertyName)
        {
            List<PropertyDefinition> propertyDefinitions = GetAllPropertyDefinitionsOfPropertySet(propertySet);
            PropertyDefinition propertyDefinition = propertyDefinitions.FirstOrDefault(prop => prop.Name == propertyName);
            return propertyDefinition;
        }

        public static List<PropertyDefinition> GetAllProperties()
        {
            var propertySets = PropertySetUtils.GetAllPropertySetDefinitions(OpenMode.ForRead);

            if (propertySets == null)
                return new List<PropertyDefinition>();
            List<PropertyDefinition> propDefList = new List<PropertyDefinition>();

            foreach (var propertySet in propertySets)
            {
                //открываем определение property set
                foreach (PropertyDefinition propDef in propertySet.Definitions)
                {
                    if (propDef == null)
                        continue;

                    propDefList.Add(propDef);
                }
            }


            return propDefList;
        }
    }
}
