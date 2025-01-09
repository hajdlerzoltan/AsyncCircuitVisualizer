using AsyncCircuitVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
	/// Interaction logic for MemoryModul.xaml
	/// </summary>
	public partial class MemoryModul : UserControl
	{
		public List<Point> InputPoints { get; private set; } = new List<Point>();
		public Point OutputPoint { get; private set; }
		public ObservableCollection<Gate> InputGates { get; set; } = new ObservableCollection<Gate>();
		public ObservableCollection<Gate> OutputGates { get; set; } = new ObservableCollection<Gate>();

		public MemoryModul()
		{
			InitializeComponent();
			InputGates.CollectionChanged += Gates_CollectionChanged;
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

		private void Gates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (Gate gate in InputGates)
				{
					gate.PropertyChanged += Gate_PropertyChanged;
				}
			}

			if (e.OldItems != null)
			{
				foreach (Gate gate in InputGates)
				{
					gate.PropertyChanged -= Gate_PropertyChanged;
				}
			}
		}

		private void Gate_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Gate.State))
			{
				// Handle State change here
				Gate gate = (Gate)sender;

				string output = "0";

				if (gate.State) 
				{
					output = "1";
				}
				else
				{
					output = "0";
				}	

				Application.Current.Dispatcher.Invoke(() =>
				{
					OutputValue.Text = output;
				});

				//System.Diagnostics.Debug.WriteLine($"Gate state changed to: {gate.State}");
				//System.Diagnostics.Debug.WriteLine($"Gate state changed to: {gate.Id}");
			}
		}
	}
}
