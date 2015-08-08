using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WaveForms;
using Microsoft.SmallBasic.Library;

namespace TestHarness
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            comboBoxMethod.SelectedIndex = 0;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            switch (comboBoxMethod.SelectedIndex)
            {
                case 0:
                    WaveForm.PlaySineWave(textBoxFrequency.Text, textBoxDuration.Text);
                    break;
                case 1:
                    WaveForm.PlaySquareWave(textBoxFrequency.Text, textBoxDuration.Text);
                    break;
                case 2:
                    Primitive channels = new Primitive();
                    channels[1] = trackBar1.Value / 10.0;
                    channels[2] = trackBar2.Value / 10.0;
                    channels[3] = trackBar3.Value / 10.0;
                    channels[4] = trackBar4.Value / 10.0;
                    channels[5] = trackBar5.Value / 10.0;
                    channels[6] = trackBar6.Value / 10.0;
                    channels[7] = trackBar7.Value / 10.0;
                    channels[8] = trackBar8.Value / 10.0;
                    WaveForm.PlayDX7(channels);
                    break;
            }
        }
    }
}
