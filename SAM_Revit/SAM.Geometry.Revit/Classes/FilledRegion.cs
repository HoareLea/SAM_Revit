// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using System.Text.Json.Nodes;
using SAM.Core;
using SAM.Geometry.Object.Planar;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public class FilledRegion : Core.Revit.ViewSpecificRevitInstance<FilledRegionType>, IBoundable2DObject
    {
        private List<Face2D> face2Ds;
        
        public FilledRegion(FilledRegionType filledRegionType, LongId viewId, IEnumerable<Face2D> face2Ds)
            : base(filledRegionType, viewId)
        {
            this.viewId = viewId == null ? null : new LongId(viewId);
            face2Ds = face2Ds?.ToList().FindAll(x => x != null).ConvertAll(x => new Face2D(x));
        }
        
        public FilledRegion(FilledRegion filledRegion) 
            : base(filledRegion)
        {
        }

        public FilledRegion(JsonObject jObject)
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

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            List<BoundingBox2D> boundingBox2Ds = face2Ds?.FindAll(x => x != null).ConvertAll(x => x.GetBoundingBox(offset));
            if(boundingBox2Ds == null || boundingBox2Ds.Count == 0)
            {
                return null;
            }

            return new BoundingBox2D(boundingBox2Ds);
        }

        public override bool FromJsonObject(JsonObject jObject)
        {
            if(!base.FromJsonObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Face2Ds"))
            {
                face2Ds = Create.ISAMGeometries<Face2D>(jObject["Face2Ds"] as JsonArray);
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result =  base.ToJsonObject();
            if(result == null)
            {
                return result;
            }

            if(face2Ds != null)
            {
                JsonArray jArray = new JsonArray();
                foreach(Face2D face2D in face2Ds)
                {
                    jArray.Add(face2D?.ToJsonObject());
                }
                result.Add("Face2Ds", jArray);
            }

            return result;
        }
    }
}
