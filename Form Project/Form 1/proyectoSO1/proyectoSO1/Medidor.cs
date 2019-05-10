using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace proyectoSO1
{
    public partial class Medidor : Form
    {
        int x = Environment.ProcessorCount;
        List<PerformanceCounter> cores = new List<PerformanceCounter>();
        public Medidor()
        {
            InitializeComponent();
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 100;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            for (int y=0; y < x; y++)
            {
                cores.Add(new PerformanceCounter("Processor", "% Processor Time", y.ToString()));
                cores[y].CategoryName = "Processor";
                cores[y].CounterName = "% Processor Time";
                cores[y].InstanceName = "_Total";
                chart1.Series["Nucleos"].Points.AddXY("Nucleo " + (y + 1).ToString(), this.cores[y].NextValue());
            }
            Timer relok = new Timer();
            relok.Interval = 1000;
            relok.Tick += new EventHandler(actualizar);
            relok.Enabled = true;
            relok.Start();
        }

        private void Medidor_Load(object sender, EventArgs e)
        {
        }
        private void actualizar(object Sender, EventArgs e)
        {
            for (int y = 0; y < this.x; y++)
            {
                try
                {
                    chart1.Series["Nucleos"].Points[y].SetValueXY("Nucleo " + (y + 1).ToString(), this.cores[y].NextValue());
                }
                catch
                {

                }
            }
        }

       
    }
}
