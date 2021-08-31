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
                    //Show information (Amended 2.2)
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

    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class CollectWindows : IExternalCommand
    {
        //This is the minimum required for a IExternalCommand
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get document
            Document doc = uidoc.Document;

            //Create Filtered Element Collector (2.4) Im filtering the document but what else can I filter?
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            //Create Filter (2.4)
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);

            //Apply filter (2.4)
            IList<Element> windows = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Dialog (2.4)
            TaskDialog.Show("Windows", string.Format("{0} windows counted", windows.Count));

            return Result.Succeeded;


        }
    }

    //Added 2.5
    [TransactionAttribute(TransactionMode.Manual)]
    public class DeleteElement : IExternalCommand
    {
        //This is the minimum required for a IExternalCommand
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get document
            Document doc = uidoc.Document;

            try
            {
                //Pick object (Added 2.5)
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //Delete element (Added 2.5)
                if (pickedObj != null)
                {
                    using (Transaction trans = new Transaction(doc, "Delete Element"))
                    {
                        trans.Start();
                        doc.Delete(pickedObj.ElementId);

                        TaskDialog tDialog = new TaskDialog("Delete Element");
                        tDialog.MainContent = "Are you sure you want to delete this element?";
                        tDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;

                        if (tDialog.Show() == TaskDialogResult.Ok)
                        {
                            trans.Commit();
                            TaskDialog.Show("Delete", pickedObj.ElementId.ToString() + " deleted");

                        }
                        else
                        {
                            trans.RollBack();
                            TaskDialog.Show("Delete", pickedObj.ElementId.ToString() + " not deleted");
                        }
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }





        }
    }

    //Added 2.7
    [TransactionAttribute(TransactionMode.Manual)]
    public class PlaceFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get Family Symbol
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> symbols = collector.OfClass(typeof(FamilySymbol)).WhereElementIsElementType().ToElements();

            FamilySymbol symbol = null;
            foreach (Element ele in symbols)
            {
                if(ele.Name == "1525 x 762mm")
                {
                    symbol = ele as FamilySymbol;
                    break;
                }
            }

            try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();

                    if(! symbol.IsActive)
                    {
                        symbol.Activate();
                    }

                    doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    trans.Commit();
                }


                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }

    //Added 2.8
    [TransactionAttribute(TransactionMode.Manual)]
    public class PlaceFamilyImproved : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get Family Symbol
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FamilySymbol symbol = collector.OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .First(x => x.Name == "1525 x 762mm");

           try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();

                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                    }

                    doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    trans.Commit();
                }


                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }

    //Added 2.9
    [TransactionAttribute(TransactionMode.Manual)]
    public class PlaceLineElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get Level
            Level level = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .First(x => x.Name == "Ground Floor");

            //Create Points
            XYZ p1 = new XYZ(-10, -10, 0);
            XYZ p2 = new XYZ(10, -10, 0);
            XYZ p3 = new XYZ(15, 0, 0);
            XYZ p4 = new XYZ(10, 10, 0);
            XYZ p5 = new XYZ(-10, -10, 0);

            //Create Curve
            List<Curve> curves = new List<Curve>();
            Line l1 = Line.CreateBound(p1, p2);
            Arc l2 = Arc.Create(p2, p4, p3);
            Line l3 = Line.CreateBound(p4, p5);
            Line l4 = Line.CreateBound(p5, p1);

            curves.Add(l1);
            curves.Add(l2);
            curves.Add(l3);
            curves.Add(l4);


            try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();

                    foreach (Curve c in curves)
                    {
                        Wall.Create(doc, c, level.Id, false);
                    }

                    trans.Commit();
                }


                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }

    //Added 2.10
    [TransactionAttribute(TransactionMode.Manual)]
    public class PlaceLoopElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Create Points 
            XYZ p1 = new XYZ(-10, -10, 0);
            XYZ p2 = new XYZ(10, -10, 0);
            XYZ p3 = new XYZ(15, 0, 0);
            XYZ p4 = new XYZ(10, 10, 0);
            XYZ p5 = new XYZ(-10, 10, 0);

            //Create Curves
            List<Curve> curves = new List<Curve>();
            Line l1 = Line.CreateBound(p1, p2);
            Arc l2 = Arc.Create(p2, p4, p3);
            Line l3 = Line.CreateBound(p4, p5);
            Line l4 = Line.CreateBound(p5, p1);

            curves.Add(l1);
            curves.Add(l2);
            curves.Add(l3);
            curves.Add(l4);

            //Create curve loop
            CurveLoop crvloop = CurveLoop.Create(curves);
            double offset = UnitUtils.ConvertToInternalUnits(135, DisplayUnitType.DUT_MILLIMETERS);
            CurveLoop offsetcrv = CurveLoop.CreateViaOffset(crvloop, offset, new XYZ(0, 0, 1));
            CurveArray cArray = new CurveArray();
            foreach (Curve c in offsetcrv)
            {
                cArray.Append(c);
            }

            try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();
                    doc.Create.NewFloor(cArray, false);
                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }

    //Added 2.11
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class GetParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                //Pick Object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (pickedObj != null)
                {
                    //Retrieve Element
                    ElementId eleId = pickedObj.ElementId;
                    Element ele = doc.GetElement(eleId);

                    

                    //Get parameter _ External parameters are ones that are saved in a shared parameter file. 
                    Parameter param = ele.LookupParameter("Head Height");
                    InternalDefinition paramDef = param.Definition as InternalDefinition;

                    TaskDialog.Show("Parameters", string.Format("{0} parameter of type {1} with biltinparameter {2}",
                        paramDef.Name,
                        paramDef.UnitType,
                        paramDef.BuiltInParameter));

                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }

    //Added 2.12
    [TransactionAttribute(TransactionMode.Manual)]
    public class SetParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                //Pick Object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (pickedObj != null)
                {
                    //Retrieve Element
                    ElementId eleId = pickedObj.ElementId;
                    Element ele = doc.GetElement(eleId);

                    //Get Parameter Value
                    Parameter param = ele.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);

                    TaskDialog.Show("Parameter values", string.Format("Parameter storage type {0} and value {1}",
                        param.StorageType.ToString(),
                        param.AsDouble()));

                    //Set Parameter Value
                    using (Transaction trans = new Transaction(doc, "Set Parameter"))
                    {
                        trans.Start();

                        //This updates the element in Revit
                        param.Set(7.5);

                        trans.Commit();
                    }

                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }

    //Added 3.1
    [TransactionAttribute(TransactionMode.Manual)]
    public class ChangeLocation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument and Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                //Pick Object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //Display Element Id
                if (pickedObj != null)
                {
                    //Retrieve Element
                    ElementId eleId = pickedObj.ElementId;
                    Element ele = doc.GetElement(eleId);

                    using (Transaction trans = new Transaction(doc, "Change Location"))
                    {
                        trans.Start();

                        //Set Location


                        trans.Commit();
                    }
                }

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