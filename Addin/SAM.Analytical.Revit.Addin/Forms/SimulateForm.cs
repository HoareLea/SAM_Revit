using SAM.Weather;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Revit.Addin.Forms
{
    public partial class SimulateForm : Form
    {
        private WeatherData weatherData;

        public SimulateForm()
        {
            InitializeComponent();
        }

        public SimulateForm(string projectName, string outputDirectory)
        {
            InitializeComponent();

            TextBox_OutputDirectory.Text = outputDirectory;
            TextBox_ProjectName.Text = projectName;
        }

        private void SimulateForm_Load(object sender, EventArgs e)
        {
            foreach(SolarCalculationMethod solarCalculationMethod in Enum.GetValues(typeof(SolarCalculationMethod)))
            {
                if(solarCalculationMethod == SolarCalculationMethod.Undefined)
                {
                    continue;
                }

                ComboBox_SolarCalculationMethod.Items.Add(Core.Query.Description(solarCalculationMethod));
            }

            ComboBox_SolarCalculationMethod.SelectedText = Core.Query.Description(SolarCalculationMethod.SAM);
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                MessageBox.Show("Provide project name");
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputDirectory) || !System.IO.Directory.Exists(OutputDirectory))
            {
                MessageBox.Show("Given output directory does not exists. Please provide valid directory");
                return;
            }

            if(weatherData == null)
            {
                MessageBox.Show("Provide Wether Data");
                return;
            }

            DialogResult = DialogResult.OK;

            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            Close();
        }

        public string OutputDirectory
        {
            get
            {
                return TextBox_OutputDirectory.Text;
            }
        }

        public string ProjectName
        {
            get
            {
                return TextBox_ProjectName.Text;
            }
        }

        public WeatherData WeatherData
        {
            get
            {
                return weatherData;
            }
        }

        public bool UnmetHours
        {
            get
            {
                return CheckBox_UnmetHours.Checked;
            }
        }

        public SolarCalculationMethod SolarCalculationMethod
        {
            get
            {
                return Core.Query.Enum<SolarCalculationMethod>(ComboBox_SolarCalculationMethod.SelectedText);
            }
        }

        private void Button_OutputDirectory_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select Output Directory";
                folderBrowserDialog.ShowNewFolderButton = true;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    TextBox_OutputDirectory.Text = folderBrowserDialog.SelectedPath;
                    TextBox_OutputDirectory.SelectionStart = TextBox_OutputDirectory.Text.Length;
                    TextBox_OutputDirectory.SelectionLength = 0;
                }
            }
        }

        private void Button_WeatherData_Click(object sender, EventArgs e)
        {
            Autodesk.Revit.UI.Result result = Query.TryGetWeatherData(out WeatherData weatherData_Temp);

            if(result != Autodesk.Revit.UI.Result.Succeeded || weatherData_Temp == null)
            {
                return;
            }

            weatherData = weatherData_Temp;
        }
    }
}
