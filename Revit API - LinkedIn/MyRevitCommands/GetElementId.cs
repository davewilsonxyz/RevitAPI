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


            //Get document(Added 2.1)
            Document doc = uidoc.Document;



            try
            {
                //Pick object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //Retrieve Element (Added 2.1)
                ElementId eleId = pickedObj.ElementId;
                Element ele = doc.GetElement(eleId);

                //Get element type (Added 2.2)
                ElementId eTypeId = ele.GetTypeId();
                ElementType eType = doc.GetElement(eTypeId) as ElementType;


                //Display element Id
                if (pickedObj != null)
                {
                    //Show information (amended 2.2)
                    TaskDialog.Show("Element classification", eleId.ToString() + Environment.NewLine
                        + "Category: " + ele.Category.Name + Environment.NewLine
                        + "Instance: " + ele.Name + Environment.NewLine
                        + "Symbol: " + eType.Name + Environment.NewLine
                        + "Family: " + eType.FamilyName);
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
