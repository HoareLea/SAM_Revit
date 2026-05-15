using System.Text.Json.Nodes;
using SAM.Core.Revit;
using SAM.Geometry.Object.Planar;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public class RevitInstance2D : RevitInstance<RevitType2D>, ISAMGeometry2DObject, IRevitInstance
    {
        private List<ISAMGeometry2D> geometries;
        
        public RevitInstance2D(RevitInstance2D revitInstance2D)
            :base(revitInstance2D)
        {

        }

        public RevitInstance2D(JsonObject jObject)
            : base(jObject)
        {

        }

        public RevitInstance2D(RevitType2D revitType2D, IEnumerable<ISAMGeometry2D> geometries)
            : base(revitType2D)
        {
            if(geometries != null)
            {
                this.geometries = new List<ISAMGeometry2D>();
                foreach(ISAMGeometry2D sAMGeometry2D in geometries)
                {
                    this.geometries.Add(sAMGeometry2D?.Clone() as ISAMGeometry2D);
                }
            }
        }

        public override bool FromJsonObject(JsonObject jObject)
        {
            if (!base.FromJsonObject(jObject))
                return false;

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jObject = base.ToJsonObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }
    }
}
