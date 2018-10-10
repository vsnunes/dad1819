using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using ChatRemotingInterfaces;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;

namespace Chat {
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class FormChatClient : System.Windows.Forms.Form {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox tb_Conversation;
        private System.Windows.Forms.TextBox tb_Message;
        private System.Windows.Forms.Button bt_Send;
        private System.Windows.Forms.TextBox tb_Port;
        private System.Windows.Forms.Label lb_Port;
        private System.Windows.Forms.Button bt_Connect;
        private System.Windows.Forms.Label lb_Name;
        private System.Windows.Forms.TextBox tb_Name;


        private IChatServer server;




        public FormChatClient() {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tb_Conversation = new System.Windows.Forms.TextBox();
            this.tb_Message = new System.Windows.Forms.TextBox();
            this.bt_Send = new System.Windows.Forms.Button();
            this.tb_Port = new System.Windows.Forms.TextBox();
            this.lb_Port = new System.Windows.Forms.Label();
            this.bt_Connect = new System.Windows.Forms.Button();
            this.lb_Name = new System.Windows.Forms.Label();
            this.tb_Name = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxConversa
            // 
            this.tb_Conversation.AcceptsReturn = true;
            this.tb_Conversation.AcceptsTab = true;
            this.tb_Conversation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_Conversation.Location = new System.Drawing.Point(120, 8);
            this.tb_Conversation.Multiline = true;
            this.tb_Conversation.Name = "textBoxConversa";
            this.tb_Conversation.ReadOnly = true;
            this.tb_Conversation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Conversation.Size = new System.Drawing.Size(215, 232);
            this.tb_Conversation.TabIndex = 0;
            // 
            // textBoxMensagem
            // 
            this.tb_Message.Location = new System.Drawing.Point(8, 245);
            this.tb_Message.Name = "textBoxMensagem";
            this.tb_Message.Size = new System.Drawing.Size(273, 20);
            this.tb_Message.TabIndex = 1;
            // 
            // buttonEnvio
            // 
            this.bt_Send.Location = new System.Drawing.Point(287, 245);
            this.bt_Send.Name = "buttonEnvio";
            this.bt_Send.Size = new System.Drawing.Size(48, 23);
            this.bt_Send.TabIndex = 2;
            this.bt_Send.Text = "send";
            this.bt_Send.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxPorto
            // 
            this.tb_Port.Location = new System.Drawing.Point(8, 24);
            this.tb_Port.Name = "textBoxPorto";
            this.tb_Port.Size = new System.Drawing.Size(40, 20);
            this.tb_Port.TabIndex = 3;
            // 
            // labelPorto
            // 
            this.lb_Port.Location = new System.Drawing.Point(25, 5);
            this.lb_Port.Name = "labelPorto";
            this.lb_Port.Size = new System.Drawing.Size(63, 16);
            this.lb_Port.TabIndex = 4;
            this.lb_Port.Text = "Client Port";
            this.lb_Port.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonLigar
            // 
            this.bt_Connect.Location = new System.Drawing.Point(56, 24);
            this.bt_Connect.Name = "buttonLigar";
            this.bt_Connect.Size = new System.Drawing.Size(56, 23);
            this.bt_Connect.TabIndex = 5;
            this.bt_Connect.Text = "connect";
            this.bt_Connect.Click += new System.EventHandler(this.button2_Click);
            // 
            // labelNome
            // 
            this.lb_Name.Location = new System.Drawing.Point(8, 182);
            this.lb_Name.Name = "labelNome";
            this.lb_Name.Size = new System.Drawing.Size(104, 18);
            this.lb_Name.TabIndex = 6;
            this.lb_Name.Text = "Nickname";
            this.lb_Name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxNome
            // 
            this.tb_Name.Location = new System.Drawing.Point(8, 208);
            this.tb_Name.Name = "textBoxNome";
            this.tb_Name.Size = new System.Drawing.Size(100, 20);
            this.tb_Name.TabIndex = 7;
            // 
            // FormClienteChat
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(352, 285);
            this.Controls.Add(this.tb_Name);
            this.Controls.Add(this.lb_Name);
            this.Controls.Add(this.bt_Connect);
            this.Controls.Add(this.lb_Port);
            this.Controls.Add(this.tb_Port);
            this.Controls.Add(this.bt_Send);
            this.Controls.Add(this.tb_Message);
            this.Controls.Add(this.tb_Conversation);
            this.Name = "FormClienteChat";
            this.Text = "PADI CHAT";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.Run(new FormChatClient());
        }

        private void button1_Click(object sender, System.EventArgs e) {
            this.server.SendMsg(this.tb_Name.Text + " : " +
                                        this.tb_Message.Text);
        }

        public void AddMsg(string s) {
            this.tb_Conversation.AppendText("\r\n" + s);
        }


        private void button2_Click(object sender, System.EventArgs e) {
            ChatClientServices.form = this;
            int port = Int32.Parse(tb_Port.Text);
            TcpChannel chan = new TcpChannel(port);
            ChannelServices.RegisterChannel(chan, false);

            // Alternative 1 for service activation
            ChatClientServices servicos = new ChatClientServices();
            RemotingServices.Marshal(servicos, "ChatClient",
                typeof(ChatClientServices));

            // Alternative 2 for service activation
            //RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(ChatClientServices), "ChatClient",
            //    WellKnownObjectMode.Singleton);

            IChatServer server = (IChatServer)Activator.GetObject(typeof(IChatServer), "tcp://localhost:8086/ChatServer");
            List<string> messages = server.RegisterClient(port.ToString());
            this.server = server;
            foreach (object o in messages) {
                AddMsg((string)o);
            }
        }
    }


    delegate void DelAddMsg(string mensagem);


    public class ChatClientServices : MarshalByRefObject, IChatClient {
        public static FormChatClient form;

        public ChatClientServices() {
        }

        public void MsgToClient(string mensagem) {
            // thread-safe access to form
            form.Invoke(new DelAddMsg(form.AddMsg), mensagem);
        }
    }
}
