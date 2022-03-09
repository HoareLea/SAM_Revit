using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Revit.Addin
{
    public static partial class Query
    {
        public static List<string> ParameterNames(this object[,] objects, int index_Group, int index_Name, IEnumerable<string> unselectedParameterGroups = null)
        {
            List<dynamic> dynamics = new List<dynamic>();
            for (int i = 5; i <= objects.GetLength(0); i++)
            {
                string parameterGroup = objects[i, index_Group] as string;
                if (string.IsNullOrWhiteSpace(parameterGroup))
                {
                    continue;
                }

                string name = objects[i, index_Name] as string;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                dynamic @dynamic = new ExpandoObject();
                dynamic.Name = name;
                dynamic.Group = parameterGroup;
                if(unselectedParameterGroups != null)
                {
                    dynamic.Checked = unselectedParameterGroups.Contains(parameterGroup);
                }

                dynamics.Add(dynamic);
            }

            dynamics.Sort((x, y) => (x.Group + x.Name).CompareTo(y.Group + y.Name));

            using (Core.Windows.Forms.TreeViewForm<dynamic> treeViewForm = new Core.Windows.Forms.TreeViewForm<dynamic>("Select Parameters", dynamics, (dynamic @dynamic) => dynamic.Name, (dynamic @dynamic) => dynamic.Group, (dynamic @dynamic) => dynamic.Checked))
            {
                treeViewForm.CollapseAll();
                treeViewForm.Size = new System.Drawing.Size(350, 700);

                if (treeViewForm.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                return treeViewForm?.SelectedItems?.ConvertAll(x => x.Name as string);
            }
        }
    }
}