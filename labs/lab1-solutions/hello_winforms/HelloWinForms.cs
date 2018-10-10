using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace HelloWinforms
{
	/// <summary>
	/// FormArranque class: when instantiated shows a windos with a message. 
	/// </summary>
	public class FormArranque : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// FormArranque default constructor. 
		/// </summary>
		public FormArranque()
		{
			//
			// Required for Windows Form Designer support.
			// The component initialization is in the region marked
			// "Windows Form Designer generated code".
			//
			InitializeComponent();            
		}

		/// <summary>
		/// Resource cleanup.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // FormArranque
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(128, 33);
            this.Name = "FormArranque";
            this.Text = "Olá!";
            this.Shown += new System.EventHandler(this.FormArranque_Load);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The Main static method, the application's entry point.
		/// </summary>
		static void Main() 
		{
			
			Application.Run(new FormArranque());
		}

		private void FormArranque_Load(object sender, System.EventArgs e) {
			// The MessageBox class's static method Show displays a
			// Windows message box after the application starts. If this 
			// were in 
			// FormArranque constructor it would appear before the main application window.
            System.Windows.Forms.MessageBox.Show("Hello World! Olá Mundo!");
		}
	}
}
