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
                            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                                return parameter_Destination.Set(System.Convert.ToDouble(elementId.IntegerValue));
#else
                                return parameter_Destination.Set(System.Convert.ToDouble(elementId.Value));
#endif

                            }

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
                            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                                return parameter_Destination.Set(elementId.IntegerValue);
#else
                                return parameter_Destination.Set(elementId.Value);
#endif
                            }
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
                            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                                return parameter_Destination.Set(parameter_Source.AsElementId()?.IntegerValue.ToString());
#else
                                return parameter_Destination.Set(parameter_Source.AsElementId()?.Value.ToString());
#endif
                            }


                        case StorageType.String:
                            return parameter_Destination.Set(parameter_Source.AsString());
                    }

                    return false;
            }

            return false;
        }
    }
}