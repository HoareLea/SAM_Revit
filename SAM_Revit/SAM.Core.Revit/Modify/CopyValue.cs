using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool CopyValue(this Parameter parameter_Source, Parameter parameter_Destination)
        {
            if (parameter_Source == null || parameter_Destination == null || parameter_Destination.IsReadOnly)
                return false;

            switch(parameter_Destination.StorageType)
            {
                case StorageType.Double:

                    switch (parameter_Source.StorageType)
                    {
                        case StorageType.Double:
                            return parameter_Destination.Set(parameter_Source.AsDouble());

                        case StorageType.Integer:
                            return parameter_Destination.Set(System.Convert.ToDouble(parameter_Source.AsInteger()));

                        case StorageType.ElementId:
                            ElementId elementId = parameter_Source.AsElementId();
                            if (elementId != null)
                                return parameter_Destination.Set(System.Convert.ToDouble(elementId.IntegerValue));
                            return false;

                        case StorageType.String:
                            if (double.TryParse(parameter_Source.AsString(), out double value))
                                return parameter_Destination.Set(value);
                            return false;
                    }

                    return false;


                case StorageType.Integer:

                    switch (parameter_Source.StorageType)
                    {
                        case StorageType.Double:
                            return parameter_Destination.Set(System.Convert.ToInt32(parameter_Source.AsDouble()));

                        case StorageType.Integer:
                            return parameter_Destination.Set(parameter_Source.AsInteger());

                        case StorageType.ElementId:
                            ElementId elementId = parameter_Source.AsElementId();
                            if (elementId != null)
                                return parameter_Destination.Set(elementId.IntegerValue);
                            return false;

                        case StorageType.String:
                            if (int.TryParse(parameter_Source.AsString(), out int value))
                                return parameter_Destination.Set(value);
                            return false;
                    }

                    return false;

                case StorageType.ElementId:
                    switch (parameter_Source.StorageType)
                    {
                        case StorageType.Double:
                            return parameter_Destination.Set(new ElementId(System.Convert.ToInt32(parameter_Source.AsDouble())));

                        case StorageType.Integer:
                            return parameter_Destination.Set(new ElementId(parameter_Source.AsInteger()));

                        case StorageType.ElementId:
                            return parameter_Destination.Set(parameter_Source.AsElementId());

                        case StorageType.String:
                            if (int.TryParse(parameter_Source.AsString(), out int value))
                                return parameter_Destination.Set(new ElementId(value));
                            return false;
                    }

                    return false;

                case StorageType.String:
                    switch (parameter_Source.StorageType)
                    {
                        case StorageType.Double:
                            return parameter_Destination.Set(parameter_Source.AsDouble().ToString());

                        case StorageType.Integer:
                            return parameter_Destination.Set(parameter_Source.AsInteger().ToString());

                        case StorageType.ElementId:
                            return parameter_Destination.Set(parameter_Source.AsElementId()?.IntegerValue.ToString());

                        case StorageType.String:
                            return parameter_Destination.Set(parameter_Source.AsString());
                    }

                    return false;
            }

            return false;
        }
    }
}