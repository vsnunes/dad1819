using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Names
{
	/// <summary>
	/// Form for manipulating a NameListClasse do formulário de manipulação de uma ListaNomes
	/// </summary>
	public class form_nomes : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox tb_insertion;
		private System.Windows.Forms.Button bt_insertion;
		private System.Windows.Forms.TextBox tb_query;
		private System.Windows.Forms.Button bt_empty;

		/// <summary>
		/// Lista de Nomes usada para guardar os nomes (instanciada no construtor)
		/// </summary>
		private INameList lista;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public form_nomes()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			lista = new NameList();
		}

		/// <summary>
		/// Limpeza de recursos que estejam a ser usados.
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
            this.tb_insertion = new System.Windows.Forms.TextBox();
            this.bt_insertion = new System.Windows.Forms.Button();
            this.tb_query = new System.Windows.Forms.TextBox();
            this.bt_empty = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tb_insertion
            // 
            this.tb_insertion.Location = new System.Drawing.Point(27, 28);
            this.tb_insertion.Name = "tb_insertion";
            this.tb_insertion.Size = new System.Drawing.Size(83, 20);
            this.tb_insertion.TabIndex = 0;
            // 
            // bt_insertion
            // 
            this.bt_insertion.Location = new System.Drawing.Point(127, 28);
            this.bt_insertion.Name = "bt_insertion";
            this.bt_insertion.Size = new System.Drawing.Size(62, 20);
            this.bt_insertion.TabIndex = 1;
            this.bt_insertion.Text = "add";
            this.bt_insertion.Click += new System.EventHandler(this.bt_insercao_Click);
            // 
            // tb_query
            // 
            this.tb_query.Location = new System.Drawing.Point(27, 111);
            this.tb_query.Multiline = true;
            this.tb_query.Name = "tb_query";
            this.tb_query.Size = new System.Drawing.Size(140, 208);
            this.tb_query.TabIndex = 2;
            // 
            // bt_empty
            // 
            this.bt_empty.Location = new System.Drawing.Point(60, 69);
            this.bt_empty.Name = "bt_empty";
            this.bt_empty.Size = new System.Drawing.Size(80, 20);
            this.bt_empty.TabIndex = 3;
            this.bt_empty.Text = "empty list";
            this.bt_empty.Click += new System.EventHandler(this.bt_esvaziar_Click);
            // 
            // form_nomes
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(248, 401);
            this.Controls.Add(this.bt_empty);
            this.Controls.Add(this.tb_query);
            this.Controls.Add(this.bt_insertion);
            this.Controls.Add(this.tb_insertion);
            this.Name = "form_nomes";
            this.Text = "names";
            this.Load += new System.EventHandler(this.form_nomes_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// Ponto de entrada da aplicação.
		/// </summary>
		static void Main() 
		{
			Application.Run(new form_nomes());
		}

		/// <summary>
		/// Processamento do evento Click no botão bt_insercao.
		/// </summary>
		private void bt_insercao_Click(object sender, System.EventArgs e) {
			if (tb_insertion.Text != "") {
				lista.Add(tb_insertion.Text);
				// actualização da caixa de texto de consulta da lista
				tb_query.Text = lista.toString();
			}
		}

		/// <summary>
		/// Método executado sempre que o formulário é carregado.
		/// </summary>
		private void form_nomes_Load(object sender, System.EventArgs e) {
			tb_query.Text = lista.toString();
		}

		/// <summary>
		/// Processamento do evento Click no botão bt_esvaziar.
		/// </summary>
		private void bt_esvaziar_Click(object sender, System.EventArgs e) {
			lista.Empty();
			// actualização da caixa de texto de consulta da lista
			tb_query.Text = lista.toString();
		}
	}
}
