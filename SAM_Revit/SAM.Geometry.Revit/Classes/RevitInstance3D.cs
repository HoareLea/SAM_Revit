using Newtonsoft.Json.Linq;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public class RevitInstance3D: RevitInstance<RevitType3D>, ISAMGeometry3DObject, IRevitInstance
    {
        private List<ISAMGeometry3D> geometries;
        
        public RevitInstance3D(RevitInstance3D revitInstance3D)
            :base(revitInstance3D)
        {

        }

        public RevitInstance3D(JObject jObject)
            : base(jObject)
        {

        }

        public RevitInstance3D(RevitType3D revitType3D, IEnumerable<ISAMGeometry3D> geometries)
            : base(revitType3D)
        {
            if(geometries != null)
            {
                this.geometries = new List<ISAMGeometry3D>();
                foreach(ISAMGeometry3D sAMGeometry3D in geometries)
                {
                    this.geometries.Add(sAMGeometry3D?.Clone() as ISAMGeometry3D);
                }
            }
        }

        public List<ISAMGeometry3D> Geometries
        {
            get
            {
                return geometries?.ConvertAll(x => x?.Clone() as ISAMGeometry3D);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public void Move(Vector3D vector3D)
        {
            if(vector3D == null)
            {
                return;
            }

            geometries = geometries?.ConvertAll(x => x.GetMoved(vector3D));
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }

        public void Transform(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return;
            }

            geometries = geometries?.ConvertAll(x => x.GetTransformed(transform3D));
        }
    }
}
