using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyRevitCommands
{
    //This transaction is needed
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class GetElementId : IExternalCommand
    {
        //This is the minimum required for a IExternalCommand
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;


            try
            {
                //Pick object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);


                //Display element Id
                if (pickedObj != null)
                {
                    TaskDialog.Show("Element Id", pickedObj.ElementId.ToString());
                }

                //Either return succeeded, cancelled or failed
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
     
        }
    }
}
