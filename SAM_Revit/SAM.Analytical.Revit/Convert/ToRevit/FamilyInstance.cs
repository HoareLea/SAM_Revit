using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static FamilyInstance ToRevit(this Document document, Aperture aperture, HostObject hostObject)
        {
            if (aperture == null || document == null)
                return null;

            string fullName = null;
            if (aperture.ApertureConstruction != null)
                fullName = aperture.ApertureConstruction.Name;

            if (string.IsNullOrWhiteSpace(fullName))
                fullName = aperture.Name;

            if (string.IsNullOrWhiteSpace(fullName))
                return null;

            string familyName;
            string familyTypeName;
            if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(fullName, out familyName, out familyTypeName))
                return null;

            Geometry.Spatial.Point3D point3D_Location = aperture.PlanarBoundary3D?.Plane?.Origin;
            if (point3D_Location == null)
                return null;

            Level level = level = Geometry.Revit.Query.LowLevel(document, point3D_Location.Z);
            if (level == null)
                return null;

            List<FamilySymbol> familySymbols = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();
            if (familySymbols == null || familySymbols.Count == 0)
                return null;

            familySymbols.RemoveAll(x => string.IsNullOrWhiteSpace(x.Name) || !x.Name.Equals(familyTypeName));
            if (!string.IsNullOrWhiteSpace(familyName))
                familySymbols.RemoveAll(x => string.IsNullOrWhiteSpace(x.FamilyName) || !x.FamilyName.Equals(familyName));

            familySymbols.RemoveAll(x => x.Family == null && x.Family.FamilyPlacementType != FamilyPlacementType.OneLevelBasedHosted);

            if (familySymbols.Count == 0)
                return null;

            FamilySymbol familySymbol = familySymbols.First();
            if (familySymbol == null)
                return null;

            if (!familySymbol.IsActive)
                familySymbol.Activate();

            FamilyInstance familyInstance;
            if(hostObject is RoofBase)
                familyInstance = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, aperture.Plane.BaseX.ToRevit(), hostObject, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            else
                familyInstance = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, hostObject, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            
            
            if (familyInstance != null)
            {
                Core.Revit.Modify.Values(aperture, familyInstance);
                Core.Revit.Modify.Values(ActiveSetting.Setting, aperture, familyInstance);
                bool simplified = false;
                if (!Geometry.Planar.Query.Rectangular(aperture.PlanarBoundary3D?.Edge2DLoop?.GetClosed2D()))
                    simplified = true;

                if (Core.Revit.Modify.Simplified(familyInstance, simplified))
                    Core.Revit.Modify.Json(familyInstance, aperture.ToJObject()?.ToString());
            }

            return familyInstance;
        
        }
    }
}
