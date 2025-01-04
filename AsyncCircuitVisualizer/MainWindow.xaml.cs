using AsyncCircuitVisualizer.Models;
using AsyncCircuitVisualizer.Views;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsyncCircuitVisualizer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

    public partial class MainWindow : Window
    {
		List<char> _InputVariables;
		List<UIElement> _Gates = new List<UIElement>();
		List<Gate> _Ands;
		List<Gate> _Or;
		List<Gate> _Inverters;
		List<Gate> _MemoryModules;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Calculate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string input = InputTextBox.Text;
				var minterms = input.Split(',').Select(int.Parse).ToList();
				int variableCount = CountVariables(minterms);

				var simplifiedBinary = QuineMcCluskey(minterms, variableCount);
				var booleanExpression = ConvertToBooleanExpression(simplifiedBinary, GenerateVariableList(variableCount));
				_InputVariables = GenerateVariableList(variableCount);
				DrawCircuit(booleanExpression);
				Output.Text = booleanExpression;
			}
			catch (Exception ex)
			{
				ErrorMessageTextBox.Text = "Error: " + ex.Message;
			}
		}

		private int CountVariables(List<int> minterms)
		{
			return minterms.Max() == 0 ? 1 : (int)Math.Floor(Math.Log2(minterms.Max())) + 1;
		}

		private List<char> GenerateVariableList(int count)
		{
			return Enumerable.Range(0, count).Select(i => (char)('A' + i)).ToList();
		}

		private string QuineMcCluskey(List<int> minterms, int variableCount)
		{
			var groups = new Dictionary<int, List<string>>();

			foreach (var minterm in minterms)
			{
				string binary = Convert.ToString(minterm, 2).PadLeft(variableCount, '0');
				int onesCount = binary.Count(c => c == '1');

				if (!groups.ContainsKey(onesCount))
					groups[onesCount] = new List<string>();

				groups[onesCount].Add(binary);
			}

			var primes = new HashSet<string>();
			bool changes;

			do
			{
				var newGroups = new Dictionary<int, List<string>>();
				var combined = new HashSet<string>();
				changes = false;

				foreach (var group in groups.OrderBy(g => g.Key))
				{
					if (!groups.ContainsKey(group.Key + 1)) continue;

					foreach (var term1 in group.Value)
					{
						foreach (var term2 in groups[group.Key + 1])
						{
							string combinedTerm = CombineTerms(term1, term2);
							if (combinedTerm != null)
							{
								changes = true;
								combined.Add(term1);
								combined.Add(term2);

								int onesCount = combinedTerm.Count(c => c == '1');
								if (!newGroups.ContainsKey(onesCount))
									newGroups[onesCount] = new List<string>();

								if (!newGroups[onesCount].Contains(combinedTerm))
									newGroups[onesCount].Add(combinedTerm);
							}
						}
					}
				}

				foreach (var group in groups)
				{
					foreach (var term in group.Value)
					{
						if (!combined.Contains(term))
							primes.Add(term);
					}
				}

				groups = newGroups;
			} while (changes);

			return string.Join(", ", primes);
		}

		private string CombineTerms(string term1, string term2)
		{
			int diffCount = 0;
			var result = new StringBuilder();

			for (int i = 0; i < term1.Length; i++)
			{
				if (term1[i] != term2[i])
				{
					diffCount++;
					result.Append('-');
				}
				else
				{
					result.Append(term1[i]);
				}

				if (diffCount > 1)
					return null;
			}

			return diffCount == 1 ? result.ToString() : null;
		}

		private string ConvertToBooleanExpression(string binaryTerms, List<char> variables)
		{
			var terms = binaryTerms.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			var result = new List<string>();

			foreach (var term in terms)
			{
				var sb = new StringBuilder();
				for (int i = 0; i < term.Length; i++)
				{
					if (term[i] == '1')
						sb.Append(variables[i]);
					else if (term[i] == '0')
						sb.Append(variables[i] + "'");
				}
				result.Add(sb.ToString());
			}

			return string.Join(" + ", result);
		}

		private UIElement CreateGateControl(string type, string output, List<string> inputs, string inputName = null)
		{
			UserControl gateControl;

			switch (type)
			{
				case "AND":
					gateControl = new ANDgate();
					((ANDgate)gateControl).ConfigureGate("AND", inputs.Count);
					break;

				case "OR":
					gateControl = new ORgate();
					((ORgate)gateControl).ConfigureGate("OR", inputs.Count);
					break;

				case "Inverter":
					gateControl = new Inverter();
					((Inverter)gateControl).ConfigureGate("1 "+output, 1);
					break;

				case "Input":
					gateControl = new Input();
					((Input)gateControl).ConfigureGate(inputName, 1);
					break;

				case "MemoryModul":
					gateControl = new MemoryModul();
					((MemoryModul)gateControl).ConfigureGate("MemoryModule", 1);
					break;

				case "Output":
					gateControl = new Output();
					((Output)gateControl).ConfigureGate("Output", 1);
					break;

				default:
					throw new ArgumentException("Unsupported gate type");
			}

			_Gates.Add(gateControl);
			return gateControl;
		}

		private void DrawCircuit(string booleanExpression)
		{
			CircuitCanvas.Children.Clear();

			var gates = ParseBooleanExpression(booleanExpression);
			_MemoryModules = IdentifyMemoryModules(gates);

			// Initialize column positions for each gate type
			double inputX = 50, memoryX = 150, inverterX = 250, andX = 350, orX = 450, outputX = 550;
			double ySpacing = 100;

			double inputY = 50, memoryY = 50, inverterY = 50, andY = 50, orY = 50, outputY = 50;

			var gatePositions = new Dictionary<string, Point>();

			// Draw input gates
			foreach (var input in _InputVariables)
			{
				UIElement gateControl = CreateGateControl("Input", input.ToString(), null, input.ToString());
				Canvas.SetLeft(gateControl, inputX);
				Canvas.SetTop(gateControl, inputY);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[input.ToString()] = new Point(inputX + 40, inputY + 25);

				inputY += ySpacing;
			}

			// Draw memory modules
			foreach (var memory in _MemoryModules)
			{
				UIElement gateControl = CreateGateControl("Memory", memory.Output, memory.Inputs , memory.Output);
				Canvas.SetLeft(gateControl, memoryX);
				Canvas.SetTop(gateControl, memoryY);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[memory.Output] = new Point(memoryX + 40, memoryY + 25);

				// Connect memory module to its input
				if (gatePositions.ContainsKey(memory.Inputs[0]))
				{
					DrawConnection(gatePositions[memory.Inputs[0]], gatePositions[memory.Output]);
				}

				memoryY += ySpacing;
			}

			// Draw inverters
			foreach (var gate in gates.Where(g => g.Type == "Inverter"))
			{
				double x = inverterX, y = inverterY;

				// Create and position the inverter control
				var gateControl = CreateGateControl(gate.Type, gate.Output, gate.Inputs);
				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[gate.Output] = new Point(x + 40, y + 25);

				// Draw connections from inputs to the current gate
				foreach (var input in gate.Inputs)
				{
					if (gatePositions.ContainsKey(input))
					{
						DrawConnection(gatePositions[input], gatePositions[gate.Output]);
					}
				}

				inverterY += ySpacing;
			}

			// Draw AND gates
			foreach (var gate in gates.Where(g => g.Type == "AND"))
			{
				double x = andX, y = andY;

				// Create and position the AND gate control
				var gateControl = CreateGateControl(gate.Type, gate.Output, gate.Inputs);
				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[gate.Output] = new Point(x + 40, y + 25);

				// Draw connections from inputs to the current gate
				foreach (var input in gate.Inputs)
				{
					if (gatePositions.ContainsKey(input))
					{
						DrawConnection(gatePositions[input], gatePositions[gate.Output]);
					}
				}

				andY += ySpacing;
			}

			// Draw OR gate
			foreach (var gate in gates.Where(g => g.Type == "OR"))
			{
				double x = orX, y = orY;

				// Create and position the OR gate control
				var gateControl = CreateGateControl(gate.Type, gate.Output, gate.Inputs);
				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[gate.Output] = new Point(x + 40, y + 25);

				// Draw connections from inputs to the current gate
				foreach (var input in gate.Inputs)
				{
					if (gatePositions.ContainsKey(input))
					{
						DrawConnection(gatePositions[input], gatePositions[gate.Output]);
					}
				}

				orY += ySpacing;
			}

			// Draw output
			UIElement outputControl = CreateGateControl("Output", "Output", new List<string>{ gates.Last().Output }, "Output");
			Canvas.SetLeft(outputControl, outputX);
			Canvas.SetTop(outputControl, outputY);

			CircuitCanvas.Children.Add(outputControl);
			gatePositions["Output"] = new Point(outputX + 40, outputY + 25);

			// Connect last OR gate to output
			if (gatePositions.ContainsKey(gates.Last().Output))
			{
				DrawConnection(gatePositions[gates.Last().Output], gatePositions["Output"]);
			}
		}



		private void DrawConnection(Point start, Point end)
		{
			PathFigure pathFigure = new PathFigure { StartPoint = start };
			pathFigure.Segments.Add(new LineSegment(end, true));

			PathGeometry geometry = new PathGeometry();
			geometry.Figures.Add(pathFigure);

			Path path = new Path
			{
				Stroke = Brushes.Black,
				StrokeThickness = 2,
				Data = geometry
			};

			CircuitCanvas.Children.Add(path);
		}

		private List<Gate> ParseBooleanExpression(string expression)
		{
			var gates = new List<Gate>();
			var terms = expression.Split('+').Select(term => term.Trim());

			foreach (var term in terms)
			{
				var inputs = new List<string>();
				foreach (var variable in term)
				{
					if (variable == '\'')
					{
						var lastVar = inputs.Last();
						inputs.Remove(lastVar);
						var negatedVar = lastVar + "'";
						gates.Add(new Gate { Type = "Inverter", Inputs = new List<string> { lastVar }, Output = negatedVar });
						inputs.Add(negatedVar);
					}
					else
					{
						inputs.Add(variable.ToString());
					}
				}

				string andOutput = string.Join("", inputs);
				gates.Add(new Gate { Type = "AND", Inputs = inputs, Output = andOutput });
			}

			var finalOutput = "Output";
			var andOutputs = gates.Where(g => g.Type == "AND").Select(g => g.Output).ToList();
			gates.Add(new Gate { Type = "OR", Inputs = andOutputs, Output = finalOutput });

			return gates;
		}

		// Helper method to identify memory modules from the gates
		private List<Gate> IdentifyMemoryModules(List<Gate> gates)
		{
			var memoryModules = new List<Gate>();

			foreach (var gate in gates)
			{
				if (gate.Type == "Memory") // Example criterion for memory detection
				{
					memoryModules.Add(new Gate
					{
						Inputs = gate.Inputs, // Assume first input is the memory input
						Output = gate.Output
					});
				}
			}

			return memoryModules;
		}
	}
}