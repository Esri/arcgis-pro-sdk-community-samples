/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
namespace DivideLines
{
  partial class frmDivideBy
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
      this.rbParts = new System.Windows.Forms.RadioButton();
      this.rbDistance = new System.Windows.Forms.RadioButton();
      this.tbValue = new System.Windows.Forms.TextBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // rbParts
      // 
      this.rbParts.AutoSize = true;
      this.rbParts.Location = new System.Drawing.Point(12, 12);
      this.rbParts.Name = "rbParts";
      this.rbParts.Size = new System.Drawing.Size(100, 17);
      this.rbParts.TabIndex = 0;
      this.rbParts.Text = "Number of parts";
      this.rbParts.UseVisualStyleBackColor = true;
      // 
      // rbDistance
      // 
      this.rbDistance.AutoSize = true;
      this.rbDistance.Checked = true;
      this.rbDistance.Location = new System.Drawing.Point(12, 35);
      this.rbDistance.Name = "rbDistance";
      this.rbDistance.Size = new System.Drawing.Size(67, 17);
      this.rbDistance.TabIndex = 1;
      this.rbDistance.TabStop = true;
      this.rbDistance.Text = "Distance";
      this.rbDistance.UseVisualStyleBackColor = true;
      // 
      // tbValue
      // 
      this.tbValue.Location = new System.Drawing.Point(12, 58);
      this.tbValue.Name = "tbValue";
      this.tbValue.Size = new System.Drawing.Size(100, 20);
      this.tbValue.TabIndex = 3;
      this.tbValue.Text = "100";
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(6, 93);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 4;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(87, 93);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 5;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // frmDivideBy
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(179, 133);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.tbValue);
      this.Controls.Add(this.rbDistance);
      this.Controls.Add(this.rbParts);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmDivideBy";
      this.Text = "Divide Lines";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.RadioButton rbParts;
    private System.Windows.Forms.RadioButton rbDistance;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    public System.Windows.Forms.TextBox tbValue;
  }
}