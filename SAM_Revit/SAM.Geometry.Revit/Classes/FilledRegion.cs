using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public class FilledRegion : Core.Revit.RevitInstance<FilledRegionType>, IBoundable2DObject
    {
        private IntegerId viewId;
        private List<Face2D> face2Ds;
        
        public FilledRegion(FilledRegionType filledRegionType, IntegerId viewId, IEnumerable<Face2D> face2Ds)
            : base(filledRegionType)
        {
            this.viewId = viewId == null ? null : new IntegerId(viewId);
            face2Ds = face2Ds?.ToList().FindAll(x => x != null).ConvertAll(x => new Face2D(x));
        }
        
        public FilledRegion(FilledRegion filledRegion) 
            : base(filledRegion)
        {
        }

        public FilledRegion(JObject jObject)
            : base(jObject)
        {
        }

        public List<Face2D> Face2Ds
        {
            get
            {
                return face2Ds?.ConvertAll(x => x == null ? null : new Face2D(x));
            }
            set
            {

            }
        }

        public IntegerId ViewId
        {
            get
            {
                if(viewId == null)
                {
                    return null;
                }

                return new IntegerId(viewId);
            }
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            List<BoundingBox2D> boundingBox2Ds = face2Ds?.FindAll(x => x != null).ConvertAll(x => x.GetBoundingBox(offset));
            if(boundingBox2Ds == null || boundingBox2Ds.Count == 0)
            {
                return null;
            }

            return new BoundingBox2D(boundingBox2Ds);
        }

        public override bool FromJObject(JObject jObject)
        {
            if(! base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Face2Ds"))
            {
                face2Ds = Create.ISAMGeometries<Face2D>(jObject.Value<JArray>("Face2Ds"));
            }

            if(jObject.ContainsKey("ViewId"))
            {
                viewId = new IntegerId(jObject.Value<JObject>("ViewId"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result =  base.ToJObject();
            if(result == null)
            {
                return result;
            }

            if(face2Ds != null)
            {
                result.Add("Face2Ds", Create.JArray(face2Ds));
            }

            if (viewId != null)
            {
                result.Add("ViewId", viewId.ToJObject());
            }

            return result;
        }
    }
}
