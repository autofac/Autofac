namespace Calculator
{
    partial class CalculatorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._operatorComboBox = new System.Windows.Forms.ComboBox();
            this._lhsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._rhsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this._resultTextBox = new System.Windows.Forms.TextBox();
            this._calculateButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._lhsNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._rhsNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // _operatorComboBox
            // 
            this._operatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._operatorComboBox.FormattingEnabled = true;
            this._operatorComboBox.Location = new System.Drawing.Point(84, 13);
            this._operatorComboBox.Name = "_operatorComboBox";
            this._operatorComboBox.Size = new System.Drawing.Size(90, 21);
            this._operatorComboBox.TabIndex = 0;
            // 
            // _lhsNumericUpDown
            // 
            this._lhsNumericUpDown.Location = new System.Drawing.Point(12, 13);
            this._lhsNumericUpDown.Name = "_lhsNumericUpDown";
            this._lhsNumericUpDown.Size = new System.Drawing.Size(66, 20);
            this._lhsNumericUpDown.TabIndex = 1;
            // 
            // _rhsNumericUpDown
            // 
            this._rhsNumericUpDown.Location = new System.Drawing.Point(180, 13);
            this._rhsNumericUpDown.Name = "_rhsNumericUpDown";
            this._rhsNumericUpDown.Size = new System.Drawing.Size(66, 20);
            this._rhsNumericUpDown.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(252, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "=";
            // 
            // _resultTextBox
            // 
            this._resultTextBox.Location = new System.Drawing.Point(271, 13);
            this._resultTextBox.Name = "_resultTextBox";
            this._resultTextBox.ReadOnly = true;
            this._resultTextBox.Size = new System.Drawing.Size(76, 20);
            this._resultTextBox.TabIndex = 4;
            // 
            // _calculateButton
            // 
            this._calculateButton.Location = new System.Drawing.Point(272, 49);
            this._calculateButton.Name = "_calculateButton";
            this._calculateButton.Size = new System.Drawing.Size(75, 23);
            this._calculateButton.TabIndex = 5;
            this._calculateButton.Text = "Calculate!";
            this._calculateButton.UseVisualStyleBackColor = true;
            this._calculateButton.Click += new System.EventHandler(this._calculateButton_Click);
            // 
            // CalculatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 84);
            this.Controls.Add(this._calculateButton);
            this.Controls.Add(this._resultTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._rhsNumericUpDown);
            this.Controls.Add(this._lhsNumericUpDown);
            this.Controls.Add(this._operatorComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CalculatorForm";
            this.Text = "Caraway Calculator";
            ((System.ComponentModel.ISupportInitialize)(this._lhsNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._rhsNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _operatorComboBox;
        private System.Windows.Forms.NumericUpDown _lhsNumericUpDown;
        private System.Windows.Forms.NumericUpDown _rhsNumericUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _resultTextBox;
        private System.Windows.Forms.Button _calculateButton;
    }
}

