using AsyncCircuitVisualizer.Models;
using AsyncCircuitVisualizer.Views;
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
		List<Gate> _MemoryModuls;

		public MainWindow()
        {
            InitializeComponent();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Calculate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Assuming input is a comma-separated list of minterms
				string input = InputTextBox.Text; // e.g., "1,3,5,7,8,11,12,17,19,21,23,24,28"
				var minterms = input.Split(',').Select(int.Parse).ToList();
				int variableCount = CountVariables(minterms); // Calculate how many variables (based on the highest minterm)

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

		private void NewCircuit_Click(object sender, RoutedEventArgs e)
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="minterms"></param>
		/// <returns></returns>
		private int CountVariables(List<int> minterms)
		{
			// Get the number of bits required to represent the largest minterm
			return minterms.Max() == 0 ? 1 : (int)Math.Floor(Math.Log2(minterms.Max())) + 1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		private List<char> GenerateVariableList(int count)
		{
			// Generate a list of variables A, B, C, ..., based on the number of variables
			return Enumerable.Range(0, count).Select(i => (char)('A' + i)).ToList();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="minterms"></param>
		/// <param name="variableCount"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="binaryTerms"></param>
		/// <param name="variables"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="term1"></param>
		/// <param name="term2"></param>
		/// <returns></returns>
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
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="output"></param>
		/// <param name="inputs"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		private UIElement CreateGateControl(string type, string output, List<string> inputs, string inputName = null)
		{
			UserControl gateControl;

			switch (type)
			{
				case "AND":
					gateControl = new ANDgate();
					((ANDgate)gateControl).ConfigureGate("AND", inputs.Count);
					_Gates.Add(gateControl);
					break;

				case "OR":
					gateControl = new ORgate();
					((ORgate)gateControl).ConfigureGate("OR", inputs.Count);
					_Gates.Add(gateControl);
					break;

				case "Inverter":
					gateControl = new Inverter();
					((Inverter)gateControl).ConfigureGate("1", 1);
					_Gates.Add(gateControl);
					break;

				case "Input":
					gateControl = new Input();
					((Input)gateControl).ConfigureGate(inputName, 1);
					_Gates.Add(gateControl);
					break;

				default:
					throw new ArgumentException("Unsupported gate type");
			}

			

			return gateControl;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
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
						// Handle NOT gate
						var lastVar = inputs.Last();
						inputs.Remove(lastVar);
						var negatedVar = lastVar + "'";

                        if (!gates.Exists(gate => gate.Output == negatedVar))
                        {
                            gates.Add(new Gate { Type = "Inverter", Inputs = new List<string> { lastVar }, Output = negatedVar });
                            inputs.Add(negatedVar);
                        }


					}
					else
					{
						inputs.Add(variable.ToString());
					}
				}

				// Create AND gate for the term
				string andOutput = string.Join("", inputs);
				gates.Add(new Gate { Type = "AND", Inputs = inputs, Output = andOutput });
			}

			// Create OR gate for the final expression
			var finalOutput = "Output";
			var andOutputs = gates.Where(g => g.Type == "AND").Select(g => g.Output).ToList();
			gates.Add(new Gate { Type = "OR", Inputs = andOutputs, Output = finalOutput });

			return gates;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="booleanExpression"></param>
		private void DrawCircuit(string booleanExpression)
		{
			CircuitCanvas.Children.Clear();

			var gates = ParseBooleanExpression(booleanExpression);
			double x = 50, y = 50;
			double xSpacing = 150, ySpacing = 100;

			var gatePositions = new Dictionary<string, Point>();

			foreach (var input in _InputVariables) 
			{
				UIElement gateControl = new Input();
				((Input)gateControl).ConfigureGate(input.ToString(), 1);

				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions["input"] = new Point(x + 40, y + 25); // connection point

				y += ySpacing;
				if (y > CircuitCanvas.Height - ySpacing)
				{
					y = 50;
					x += xSpacing;
				}
			}

			_Inverters = gates.Where(x=>x.Type == "Inverter").ToList();
			_Ands = gates.Where(x => x.Type == "AND").ToList();
			_Or = gates.Where(x => x.Type == "OR").ToList();

			y = 50;
			x += xSpacing;

			for (int i = 0; i < _InputVariables.Count; i++)
			{

                UIElement gateControl = new MemoryModul();
                ((MemoryModul)gateControl).ConfigureGate("MemoryModule", 1);

                Canvas.SetLeft(gateControl, x);
                Canvas.SetTop(gateControl, y);

                CircuitCanvas.Children.Add(gateControl);
                gatePositions["MemoryModule"] = new Point(x + 40, y + 25); // connection point

                y += ySpacing;
                if (y > CircuitCanvas.Height - ySpacing)
                {
                    y = 50;
                    x += xSpacing;
                }

            }

			y = 50;
			x += xSpacing;

			foreach (var inverter in _Inverters)
			{
				var gateControl = CreateGateControl(inverter.Type, inverter.Output, inverter.Inputs);
				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[inverter.Output] = new Point(x + 80, y + 25); // connection point

                y += ySpacing;
				//x += xSpacing;
				if (y > CircuitCanvas.Height - ySpacing)
				{
					y = 50;
					x += xSpacing;
				}
			}

			y = 50;
			x += xSpacing;

			foreach (var gate in _Ands)
			{


				var gateControl = CreateGateControl(gate.Type, gate.Output, gate.Inputs);
				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[gate.Output] = new Point(x + 40, y + 25); // connection point

                y += ySpacing;
				//x += xSpacing;
				if (y > CircuitCanvas.Height - ySpacing)
				{
					y = 50;
					x += xSpacing;
				}
			}

			y = 50;
			x += xSpacing;

			foreach (var gate in _Or)
			{


				var gateControl = CreateGateControl(gate.Type, gate.Output, gate.Inputs);
				Canvas.SetLeft(gateControl, x);
				Canvas.SetTop(gateControl, y);

				CircuitCanvas.Children.Add(gateControl);
				gatePositions[gate.Output] = new Point(x + 40, y + 25); // connection point

                y += ySpacing;
				//x += xSpacing;
				if (y > CircuitCanvas.Height - ySpacing)
				{
					y = 50;
					x += xSpacing;
				}
			}

			y = 50;
			x += xSpacing;

			UIElement gateUI = new MemoryModul();
			((MemoryModul)gateUI).ConfigureGate("MemoryModule", 1);

			Canvas.SetLeft(gateUI, x);
			Canvas.SetTop(gateUI, y);

			CircuitCanvas.Children.Add(gateUI);
			gatePositions["MemoryModule"] = new Point(x + 40, y + 25); // connection point

            y += ySpacing;
			if (y > CircuitCanvas.Height - ySpacing)
			{
				y = 50;
				x += xSpacing;
			}

			y = 50;
			x += xSpacing;

			gateUI = new Output();
			((Output)gateUI).ConfigureGate("Output", 1);

			Canvas.SetLeft(gateUI, x);
			Canvas.SetTop(gateUI, y);

			CircuitCanvas.Children.Add(gateUI);
			gatePositions["MemoryModule"] = new Point(x + 40, y + 25); // connection point

            y += ySpacing;
			if (y > CircuitCanvas.Height - ySpacing)
			{
				y = 50;
				x += xSpacing;
			}

			foreach (var gate in gates)
			{
				foreach (var input in gate.Inputs)
				{
					if (gatePositions.ContainsKey(input) && gatePositions.ContainsKey(gate.Output))
					{
						DrawConnection(gatePositions[input], gatePositions[gate.Output]);
					}
				}
			}
		}
	}
}