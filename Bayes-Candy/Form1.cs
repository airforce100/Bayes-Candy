using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private double h1 = 0.1, h2 = 0.2, h3 = 0.4, h4 = 0.2, h5 = 0.1; // default values for the hypotheses

        private bool properHSum = true; // check for whether the hypotheses probabilities add up to 1
        private bool properHVal = true; // check for whether the hypotheses textboxes contain numeric values
        private bool fileSelected = false; // check for whether a file is selected
        private bool parseSuccessful = true; // check for whether the file was successfully parsed
        private bool loaded = false; // check whether the graph has been loaded yet

        private char[] data; // the data from the file
        private string[] filepath; // the array containing parts of the full filepath (separated by the '\' delimiter)
        private string[] lines; // the set of all strings in the input text file
        private string path; // the full filepath string

        // the series of points corresponding to the hypothesis of the same number
        private System.Windows.Forms.DataVisualization.Charting.Series series1, series2;

        public Form1()
        {
            InitializeComponent();
        }

        private void fileSelectButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "txt files (*.txt)|*.txt";
            file.RestoreDirectory = true;
            
            // get the filepath, read the file's contents, and update the graph
            if (file.ShowDialog() == DialogResult.OK)
            {
                path = file.FileName;
                filepath = file.FileName.Split(new char[] {'\\'});
                this.label1.Text = filepath[filepath.Length - 1];
                fileSelected = true;
            }
        }

        private void plotButton_Click(object sender, EventArgs e)
        {
            properHSum = true;
            properHVal = true;
            parseSuccessful = true;
            label7.Text = "";

            if (!fileSelected)
                label7.Text = "File is not selected";
            else
            {
                // check that h values are Doubles
                try
                {
                    h1 = Convert.ToDouble(textBox1.Text.ToString());
                    h2 = Convert.ToDouble(textBox2.Text.ToString());
                    h3 = Convert.ToDouble(textBox3.Text.ToString());
                    h4 = Convert.ToDouble(textBox4.Text.ToString());
                    h5 = Convert.ToDouble(textBox5.Text.ToString());
                }
                catch (FormatException)
                {
                    label7.Text = "h values must be real numbers between 0 and 1, inclusive";
                    properHVal = false;
                }
                catch (OverflowException)
                {
                    label7.Text = "h values must be within the range of a Double";
                    properHVal = false;
                }

                if (properHVal)
                {
                    // check that h values are positive
                    if (h1 < 0 || h2 < 0 || h3 < 0 || h4 < 0 || h5 < 0)
                    {
                        label7.Text = "h values must be real numbers between 0 and 1, inclusive";
                        properHVal = false;
                    }
                    // check that h values sum to 1 (this procedure is different for floating-point values)
                    else if (Math.Abs((h1 + h2 + h3 + h4 + h5) - 1) >= 0.000000001)
                    {
                        label7.Text = "The sum of h values must be 1";
                        properHSum = false;
                    }
                }
            }

            if (properHSum && properHVal && fileSelected)
            {
                // read all lines in the file and store them into variable lines
                lines = System.IO.File.ReadAllLines(path);

                // initialize the character array for the flavor data
                data = new char[lines.Length];

                // variable storing the current newline-delimited character in the file
                char check;

                for (int i = 0; i < lines.Length; i++)
                {
                    // if the current line in the file has more than one character, then parse was unsuccessful
                    if (lines[i].Length != 1)
                    {
                        label7.Text = "Each line in the file must contain one character (c or l)";
                        parseSuccessful = false;
                        i = lines.Length;
                    }
                    else
                    {
                        check = lines[i].ToLower()[0];

                        // if the current newline-delimited character is not 'c' or 'l', then parse was unsuccessful
                        if (check != 'c' && check != 'l')
                        {
                            label7.Text = "File can only contain newline-delimited characters (c or l)";
                            parseSuccessful = false;
                            i = lines.Length;
                        }
                        else
                            data[i] = check;
                    }
                }
                
                // fill the graph with the data points for each hypothesis if file parsing was successful
                if(parseSuccessful)
                    fillGraph();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = h1.ToString();
            textBox2.Text = h2.ToString();
            textBox3.Text = h3.ToString();
            textBox4.Text = h4.ToString();
            textBox5.Text = h5.ToString();

            series1 = chart1.Series["P(c | D)"];

            // empty series that exists in order to properly plot the hypotheses data points
            series2 = chart1.Series["Series2"];
        }

        private void fillGraph()
        {
            series1.Points.Clear();

            if (!loaded)
            {
                series2.Points.AddXY(0, 0);
                series2.Points.AddXY(1, 0);
                series2.Points.FindByValue(0).IsEmpty = true;
                loaded = true;
            }

            int cherryCount = 0;
            int limeCount = 0;

            double alpha;
            double P_h1, P_h2, P_h3, P_h4, P_h5;
            double x_value, y_value;

            P_h1 = Math.Pow(1, 0) * Math.Pow(0, limeCount) * h1;
            P_h2 = Math.Pow(1, 0) * Math.Pow(0, limeCount) * h2;
            P_h3 = Math.Pow(1, 0) * Math.Pow(0, limeCount) * h3;
            P_h4 = Math.Pow(1, 0) * Math.Pow(0, limeCount) * h4;
            P_h5 = Math.Pow(1, 0) * Math.Pow(0, limeCount) * h5;

            alpha = 1 / (P_h1 + P_h2 + P_h3 + P_h4 + P_h5);

            x_value = 0;
            y_value = 1 * P_h1 + 0.75 * P_h2 + 0.5 * P_h3 + 0.25 * P_h4 + 0 * P_h5;

            // plot the initial point
            series1.Points.AddXY(x_value, y_value);
            
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 'c')
                    cherryCount++;
                else
                    limeCount++;

                // Formula: P(hi | D) = alpha * P(D | hi) * P(hi)
                // where alpha = 1 / ( Sum of P(hi | D) over i = 1 to 5 )

                P_h1 = Math.Pow(1, cherryCount) * Math.Pow(0, limeCount) * h1;
                P_h2 = Math.Pow(0.75, cherryCount) * Math.Pow(0.25, limeCount) * h2;
                P_h3 = Math.Pow(0.5, cherryCount) * Math.Pow(0.5, limeCount) * h3;
                P_h4 = Math.Pow(0.25, cherryCount) * Math.Pow(0.75, limeCount) * h4;
                P_h5 = Math.Pow(0, cherryCount) * Math.Pow(1, limeCount) * h5;

                alpha = 1 / (P_h1 + P_h2 + P_h3 + P_h4 + P_h5);

                P_h1 *= alpha;
                P_h2 *= alpha;
                P_h3 *= alpha;
                P_h4 *= alpha;
                P_h5 *= alpha;

                x_value = i + 1;
                y_value = 1 * P_h1 + 0.75 * P_h2 + 0.5 * P_h3 + 0.25 * P_h4 + 0 * P_h5;

                series1.Points.AddXY(x_value, y_value);
            }
        }
    }
}
