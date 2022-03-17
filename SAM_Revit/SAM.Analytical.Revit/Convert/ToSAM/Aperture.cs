using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Aperture ToSAM(this EnergyAnalysisOpening energyAnalysisOpening, ConvertSettings convertSettings)
        {
            if (energyAnalysisOpening == null)
                return null;

            Aperture result = convertSettings?.GetObject<Aperture>(energyAnalysisOpening.Id);
            if (result != null)
                return result;

            Polygon3D polygon3D = energyAnalysisOpening.GetPolyloop().ToSAM();
            if (polygon3D == null)
                return null;

            FamilyInstance familyInstance = Core.Revit.Query.Element(energyAnalysisOpening) as FamilyInstance;
            if (familyInstance == null)
                return new Aperture(null, polygon3D);

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObjects<Aperture>(familyInstance)?.FirstOrDefault();
                if (result != null)
                    return result;
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance, convertSettings);

            Point3D point3D_Location = Geometry.Revit.Query.Location(familyInstance);
            if (point3D_Location == null)
                return null;

            result = new Aperture(apertureConstruction, polygon3D, point3D_Location);
            result.UpdateParameterSets(energyAnalysisOpening, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            //result.UpdateParameterSets(familyInstance, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            //result.Add(Core.Revit.Query.ParameterSet(familyInstance));

            convertSettings?.Add(energyAnalysisOpening.Id, result);

            return result;
        }
    }
}