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
	/// Interaction logic for Input.xaml
	/// </summary>
	public partial class Input : UserControl
	{
		public List<Point> InputPoints { get; private set; } = new List<Point>();
		public Point OutputPoint { get; private set; }

        public event Action<bool> OnPush; // Event to notify when an impulse occurs

		public Input()
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

		// Event handler for click
		private async void GateBody_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// Simulate an impulse
			TriggerImpulse();
			await ResetAfterDelay();
		}

		// Trigger the impulse (state changes to true momentarily)
		private void TriggerImpulse()
		{
			GateBody.Fill = Brushes.Green; // Green for active (impulse)
			OnPush?.Invoke(true); // Notify listeners
		}

		// Reset the state after a short delay
		private async Task ResetAfterDelay()
		{
			await Task.Delay(100); // 100 ms delay for the impulse
			GateBody.Fill = Brushes.LightGray; // Reset to default (inactive)
			OnPush?.Invoke(false); // Notify listeners of reset
		}
	}
}
