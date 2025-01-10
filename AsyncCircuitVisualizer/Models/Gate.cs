using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCircuitVisualizer.Models
{
	public class Gate
	{
		private string _type;
		private bool _state;

		public string Type
		{
			get => _type;
			set => SetProperty(ref _type, value);
		}

		public List<string> Inputs { get; set; } = new();
		public string? Output { get; set; }
		//public List<string>? Output { get; set; }

		public bool State
		{
			get => _state;
			set => SetProperty(ref _state, value);
		}

		public Guid Id { get; set; } = Guid.NewGuid();

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(field, value))
			{
				field = value;
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}
	}
}
