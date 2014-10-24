﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using WebAnnotationModel.Service;
using WebAnnotationModel.Objects;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Specialized; 

namespace WebAnnotationModel
{
    /// <summary>
    /// This base class implements the basic functionality to talk to a WCF Service
    /// </summary>
    public abstract class StoreBaseWithIndexKey<PROXY, INTERFACE, KEY, KEYGEN, OBJECT, WCFOBJECT> : StoreBaseWithKey<PROXY, INTERFACE, KEY, OBJECT, WCFOBJECT>
        where INTERFACE : class
        where KEY : struct
        where KEYGEN : IKeyGenerator<KEY>, new()
        where PROXY : System.ServiceModel.ClientBase<INTERFACE>
        where WCFOBJECT : WebAnnotationModel.Service.DataObjectWithKeyOflong, new()
        where OBJECT : WCFObjBaseWithKey<KEY, WCFOBJECT>, new()
    {
        /// <summary>
        /// When we create an object we don't know the ID the database will give it.  We use this value on the local machine until the DB
        /// tells us the new ID on insert
        /// </summary>
        static KEYGEN keyGenerator = new KEYGEN();
        public KEY GetTempKey()
        {
            return keyGenerator.NextKey(); 
        }

       public virtual bool Save()
        {
            List<OBJECT> changed = new List<OBJECT>(ChangedObjects.Count);

            while (ChangedObjects.Count > 0)
            {
                KeyValuePair<KEY, OBJECT> KeyValue = ChangedObjects.FirstOrDefault();

                OBJECT obj = null;
                bool success = ChangedObjects.TryRemove(KeyValue.Key, out obj);
                if (!success)
                    continue; 

                if (obj.DBAction == DBACTION.NONE)
                    continue;

                changed.Add(obj);
            }

            return Save(changed);
        }

        

        /// <summary>
        /// Save all changes to locations, returns true if the method completed without errors, otherwise false
        /// Update key values with the index generated by the database
        /// </summary>
        protected override bool Save(List<OBJECT> input)
        {
            Trace.WriteLine("Saving this number of objects: " + input.Count, "WebAnnotation");

            /*Don't make the call if there are no changes */
            if (input.Count == 0)
                return true;

            ChangeInventory<OBJECT> inventory;
            List<OBJECT> output = new List<OBJECT>(input.Count);
            List<WCFOBJECT> changedDBObj = null; 
            try
            {
                changedDBObj = new List<WCFOBJECT>(input.Count);

                
                foreach (OBJECT dbObj in input)
                {
                    changedDBObj.Add(dbObj.GetData()); 
                }
                                
                PROXY proxy =null;
                    
                KEY[] newIDs = new KEY[0];
                try
                {
                    proxy = CreateProxy();
                    proxy.Open();


                    newIDs = ProxyUpdate(proxy, changedDBObj.ToArray());
                }
                catch (Exception e)
                {
                    Trace.WriteLine("An error occurred during the update:\n" + e.Message);
                    return false;
                }
                finally
                {
                    if (proxy != null)
                    {
                        proxy.Close();
                        proxy = null;
                    }
                }

                Debug.Assert(changedDBObj.Count == newIDs.Length);

                inventory = ProcessUpdateResults(newIDs, input, changedDBObj);
            }
            catch (FaultException )
            {
                //  System.Windows.Forms.MessageBox.Show("An exception occurred while saving structure types.  Viking is pretending none of the changes happened.  Exception Data: " + e.Message, "Error");

                if (changedDBObj != null)
                {
                    List<OBJECT> listDeleted = new List<OBJECT>(changedDBObj.Count);
                    //Update ID's of new objects
                    for (int iObj = 0; iObj < changedDBObj.Count; iObj++)
                    {
                        WCFOBJECT data = changedDBObj[iObj];
                        WCFObjBaseWithKey<KEY, WCFOBJECT> keyObj = data as WCFObjBaseWithKey<KEY, WCFOBJECT>;
                        if (keyObj == null)
                        {
                            Debug.Fail("Could not convert WCFOBJECT template to proper type, StoreBaseWithIndexKey.cs");
                            continue;
                        }

                        if (IDToObject.ContainsKey(keyObj.ID) == false)
                        {
                            data.DBAction = DBACTION.NONE;
                            continue;
                        }

                        OBJECT obj = IDToObject[keyObj.ID];

                        switch (data.DBAction)
                        {
                            case DBACTION.INSERT:
                                //Remove from our old spot in the database
                                listDeleted.Add(InternalDelete(keyObj.ID));

                                break;

                            case DBACTION.DELETE:
                                //Just reset our DBState to none after case statement
                                break;

                            default:
                                break;
                        }

                        data.DBAction = DBACTION.NONE;
                    }

                    CallOnCollectionChangedForDelete(listDeleted);
                }

                //If we caught an exception return false
                return false;
            }

            if(inventory != null)
            {
                CallOnCollectionChanged(inventory);
            }

            //CallOnAllUpdatesCompleted(new OnAllUpdatesCompletedEventArgs(output.ToArray()));

            return true;
        }


        protected ChangeInventory<OBJECT> ProcessUpdateResults(KEY[] newIDs, List<OBJECT> input, List<WCFOBJECT> changedDBObj)
        {
            List<OBJECT> newObjList = new List<OBJECT>(newIDs.Length);
            List<OBJECT> updateObjList = new List<OBJECT>(changedDBObj.Count);
            List<KEY> delObjList = new List<KEY>(changedDBObj.Count);


            List<KEY> replacedKeysList = new List<KEY>(newIDs.Length);
            List<OBJECT> replacedObjList = new List<OBJECT>(newIDs.Length); 


            //Update ID's of new objects
            for (int iObj = 0; iObj < input.Count; iObj++)
            {
                WCFOBJECT data = changedDBObj[iObj];
                //I have to do this because WCF does not marshal the templates for the WCFObject types
                //This should be fairly future proof
                WCFObjBaseWithKey<KEY, WCFOBJECT> keyObj = input[iObj];
                if (IDToObject.ContainsKey(keyObj.ID) == false)
                {
                    data.DBAction = DBACTION.NONE;
                    continue;
                }

                OBJECT obj = IDToObject[keyObj.ID];

                switch (data.DBAction)
                {
                    case DBACTION.INSERT:
                        if (newIDs.Length > iObj)
                        {
                            replacedKeysList.Add(keyObj.ID);
                            obj.ID = newIDs[iObj]; 
                            //keyObj.ServerGeneratedID = newIDs[iObj];
                            //OBJECT objCopy = obj.Clone() as OBJECT;
                            //objCopy.ID = newIDs[iObj];
                            replacedObjList.Add(obj);
                            //keyObj.GetData().ID = newIDs[iObj];
                            newObjList.Add(obj); 
                        }

                        // obj.FireAfterSaveEvent();
                        break;
                    case DBACTION.UPDATE:
                        //newobj = InternalUpdate(obj);
                        updateObjList.Add(obj);
                        // obj.FireAfterSaveEvent();
                        break;

                    case DBACTION.DELETE:
                        //Remove from our old spot in the database
                        delObjList.Add(obj.ID);
                        //InternalDelete(obj.ID);
                        // obj.FireAfterDeleteEvent();
                        break;

                    default:
                        break;
                }

                data.DBAction = DBACTION.NONE;
                // if (ChangedObjects.ContainsKey(keyObj.ID))
                // {
                //     OBJECT Trash; 
                //     ChangedObjects.TryRemove(keyObj.ID, out Trash); 
                // }
            }

            
            ChangeInventory<OBJECT> output = InternalAdd(newObjList.ToArray());
            output.UpdatedObjects.AddRange(InternalUpdate(updateObjList.ToArray()) );
            output.DeletedObjects = InternalDelete(delObjList.ToArray());

            /*
            if (replacedKeysList.Count > 0)
            {
                ChangeInventory<OBJECT> new_objects_for_replacements = InternalGetObjectsByIDs(replacedKeysList, true);
                ChangeInventory<OBJECT> replacement_output = InternalReplace(replacedKeysList.ToArray(), new_objects_for_replacements.AddedObjects.ToArray());
                output.Add(replacement_output);
            }
            */

            //Fetch the inserted version of our objects from the server
            //replacementObjList = this.GetObjectsByIDs(replacedKeysList, true);
            //InternalReplace(replacedKeysList.ToArray(), replacementObjList.ToArray());
              
            return output;
        }

        /// <summary>
        /// Replace an existing object with a new object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newObj"></param>
        /// /// <param name="ObjectAdded">Return true if the new object was added</param>
        /// <returns></returns>
        protected override OBJECT TryReplaceObject(KEY key, OBJECT newObj, out bool ObjectAdded)
        {
            //InternalUpdate(keyObj); 
            //Remove from our old spot in the database 
            OBJECT ExistingObj = TryRemoveObject(key);
            
            ObjectAdded = TryAddObject(newObj);

            return ExistingObj;
        }


    }
}
