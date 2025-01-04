using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCircuitVisualizer.Models
{
	public class Gate
	{
		public string Type { get; set; } // AND, OR, Inverter
		public List<string>? Inputs { get; set; } = new List<string>();
		public string Output { get; set; }
	}
}
