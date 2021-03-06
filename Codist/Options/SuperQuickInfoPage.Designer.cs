﻿namespace Codist.Options
{
	partial class SuperQuickInfoPage
	{
		/// <summary> 
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region 组件设计器生成的代码

		/// <summary> 
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent() {
			this._SelectionQuickInfoBox = new System.Windows.Forms.CheckBox();
			this._ControlQuickInfoBox = new System.Windows.Forms.CheckBox();
			this._SuperQuickInfoTabs = new System.Windows.Forms.TabControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this._ColorQuickInfoBox = new System.Windows.Forms.CheckBox();
			this._SuperQuickInfoTabs.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// _SelectionQuickInfoBox
			// 
			this._SelectionQuickInfoBox.AutoSize = true;
			this._SelectionQuickInfoBox.Location = new System.Drawing.Point(15, 31);
			this._SelectionQuickInfoBox.Name = "_SelectionQuickInfoBox";
			this._SelectionQuickInfoBox.Size = new System.Drawing.Size(285, 19);
			this._SelectionQuickInfoBox.TabIndex = 2;
			this._SelectionQuickInfoBox.Text = "Show info about selection length";
			this._SelectionQuickInfoBox.UseVisualStyleBackColor = true;
			// 
			// _ControlQuickInfoBox
			// 
			this._ControlQuickInfoBox.AutoSize = true;
			this._ControlQuickInfoBox.Location = new System.Drawing.Point(15, 6);
			this._ControlQuickInfoBox.Name = "_ControlQuickInfoBox";
			this._ControlQuickInfoBox.Size = new System.Drawing.Size(333, 19);
			this._ControlQuickInfoBox.TabIndex = 1;
			this._ControlQuickInfoBox.Text = "Hide quick info until Shift is pressed";
			this._ControlQuickInfoBox.UseVisualStyleBackColor = true;
			// 
			// _SuperQuickInfoTabs
			// 
			this._SuperQuickInfoTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._SuperQuickInfoTabs.Controls.Add(this.tabPage2);
			this._SuperQuickInfoTabs.Location = new System.Drawing.Point(3, 3);
			this._SuperQuickInfoTabs.Name = "_SuperQuickInfoTabs";
			this._SuperQuickInfoTabs.SelectedIndex = 0;
			this._SuperQuickInfoTabs.Size = new System.Drawing.Size(569, 322);
			this._SuperQuickInfoTabs.TabIndex = 3;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this._ColorQuickInfoBox);
			this.tabPage2.Controls.Add(this._ControlQuickInfoBox);
			this.tabPage2.Controls.Add(this._SelectionQuickInfoBox);
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(561, 293);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "General";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// _ColorQuickInfoBox
			// 
			this._ColorQuickInfoBox.AutoSize = true;
			this._ColorQuickInfoBox.Location = new System.Drawing.Point(15, 56);
			this._ColorQuickInfoBox.Name = "_ColorQuickInfoBox";
			this._ColorQuickInfoBox.Size = new System.Drawing.Size(197, 19);
			this._ColorQuickInfoBox.TabIndex = 3;
			this._ColorQuickInfoBox.Text = "Show info about color";
			this._ColorQuickInfoBox.UseVisualStyleBackColor = true;
			// 
			// SuperQuickInfoPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._SuperQuickInfoTabs);
			this.Name = "SuperQuickInfoPage";
			this.Size = new System.Drawing.Size(575, 328);
			this.Load += new System.EventHandler(this.SuperQuickInfoPage_Load);
			this._SuperQuickInfoTabs.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.CheckBox _ControlQuickInfoBox;
		private System.Windows.Forms.CheckBox _SelectionQuickInfoBox;
		private System.Windows.Forms.TabControl _SuperQuickInfoTabs;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.CheckBox _ColorQuickInfoBox;
	}
}
