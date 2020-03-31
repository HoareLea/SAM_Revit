using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class GooConvertSettings : GH_Goo<ConvertSettings>
    {
        public GooConvertSettings()
            : base()
        {

        }

        public GooConvertSettings(ConvertSettings convertSettings)
        {
            Value = convertSettings;
        }

        public override bool IsValid => Value != null;

        public override string TypeName => typeof(ConvertSettings).Name;

        public override string TypeDescription => typeof(ConvertSettings).FullName.Replace(".", " ");

        public override IGH_Goo Duplicate()
        {
            return new GooConvertSettings(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            if (Value == null)
                return false;

            writer.SetString(typeof(ConvertSettings).FullName, Value.ToJObject().ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            string value = null;
            if (!reader.TryGetString(typeof(ConvertSettings).FullName, ref value))
                return false;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            Value = Create.IJSAMObject<ConvertSettings>(value);
            return true;
        }

        public override string ToString()
        {
            string value = typeof(ConvertSettings).FullName;

            return value;
        }

        public override bool CastFrom(object source)
        {
            if (source is ConvertSettings)
            {
                Value = (ConvertSettings)(object)source;
                return true;
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (typeof(Y) == typeof(ConvertSettings))
            {
                target = (Y)(object)Value;
                return true;
            }

            if (typeof(Y) == typeof(object))
            {
                target = (Y)(object)Value;
                return true;
            }

            return base.CastTo<Y>(ref target);
        }

        public ConvertSettings ConvertSettings()
        {
            return Value;
        }
    }

    public class GooConvertSettingsParam : GH_PersistentParam<GooConvertSettings>
    {
        public override Guid ComponentGuid => new Guid("5af7e0dc-8d0c-4d51-8c85-6f2795c2fc37");
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooConvertSettingsParam()
            : base(typeof(GooConvertSettings).Name, typeof(GooConvertSettings).Name, typeof(GooConvertSettings).FullName.Replace(".", " "), "Params", "SAM")
        {

        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooConvertSettings> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooConvertSettings value)
        {
            throw new NotImplementedException();
        }
    }
}
