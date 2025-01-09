using AsyncCircuitVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsyncCircuitVisualizer.Views
{
	/// <summary>
	/// Interaction logic for Inverter.xaml
	/// </summary>
	public partial class Inverter : UserControl
	{
		public List<Point> InputPoints { get; private set; } = new List<Point>();
		public Point OutputPoint { get; private set; }
        public List<Gate>? ConnectedGates = new List<Gate>();

        public Inverter()
		{
			InitializeComponent();
		}

		public void ConfigureGate(string label, int inputCount)
		{
			// Update label
			GateLabel.Text = label;

			// Adjust size based on input count
			double height = Math.Max(50, inputCount * 20);
			GateBody.Height = height;
			GateBody.Width = 80;

			// Generate input and output points dynamically
			InputPoints.Clear();
			for (int i = 0; i < inputCount; i++)
			{
				double y = 10 + i * (height - 20) / (inputCount - 1);
				InputPoints.Add(new Point(0, y));
			}

			// Output always at the center-right
			OutputPoint = new Point(GateBody.Width, height / 2);
		}
	}
}
