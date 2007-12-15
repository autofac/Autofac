using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Calculator
{
    partial class CalculatorForm : Form
    {
        Calculator _calculator;

        protected CalculatorForm()
        {
            InitializeComponent();
        }

        public CalculatorForm(Calculator calculator)
            : this()
        {
            if (calculator == null)
                throw new ArgumentNullException("calculator");

            _calculator = calculator;

            foreach (var op in _calculator.AvailableOperators)
                _operatorComboBox.Items.Add(op);

            if (_operatorComboBox.Items.Count == 0)
                throw new ArgumentException("Calculator doesn't contain any operations.");
            else
                _operatorComboBox.SelectedIndex = 0;
        }

        private void _calculateButton_Click(object sender, EventArgs e)
        {
            double result = _calculator.ApplyOperator(
                (string)_operatorComboBox.SelectedItem,
                (double)_lhsNumericUpDown.Value,
                (double)_rhsNumericUpDown.Value);

            _resultTextBox.Text = result.ToString();
        }
    }
}
