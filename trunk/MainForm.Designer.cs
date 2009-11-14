namespace BACnetInteropApp
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.TreeViewDevices = new System.Windows.Forms.TreeView();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.buttonSendWhoIs = new System.Windows.Forms.Button();
            this.timerUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.Quit = new System.Windows.Forms.Button();
            this.whoisrouterbtn = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.buttonReadPropertyTest = new System.Windows.Forms.Button();
            this.timerHeartbeatWhoIs = new System.Windows.Forms.Timer(this.components);
            this.buttonSendReadProperty = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.mycontextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.whoIsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readPropertyObjectListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.radioButtonBAC1 = new System.Windows.Forms.RadioButton();
            this.radioButtonBAC0 = new System.Windows.Forms.RadioButton();
            this.radioButtonFDT = new System.Windows.Forms.RadioButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textBoxDiagnosticLog = new System.Windows.Forms.TextBox();
            this.labelLog = new System.Windows.Forms.Label();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonMinimize = new System.Windows.Forms.Button();
            this.contextMenuStripForRouter = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.readRouterTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.mycontextMenuStrip.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.contextMenuStripForRouter.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeViewDevices
            // 
            this.TreeViewDevices.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeViewDevices.HideSelection = false;
            this.TreeViewDevices.Location = new System.Drawing.Point(12, 45);
            this.TreeViewDevices.Name = "TreeViewDevices";
            this.TreeViewDevices.Size = new System.Drawing.Size(364, 400);
            this.TreeViewDevices.TabIndex = 2;
            this.TreeViewDevices.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.BACnetInternetworkTreeView_NodeMouseClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(402, 214);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Expand All";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ExpandAllButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(402, 243);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Collapse All";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.CollapseAllButton_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.helpToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(944, 25);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("helpToolStripButton.Image")));
            this.helpToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.helpToolStripButton.Name = "helpToolStripButton";
            this.helpToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.helpToolStripButton.Text = "He&lp";
            this.helpToolStripButton.Click += new System.EventHandler(this.helpToolStripButton_Click);
            // 
            // buttonSendWhoIs
            // 
            this.buttonSendWhoIs.Location = new System.Drawing.Point(402, 57);
            this.buttonSendWhoIs.Name = "buttonSendWhoIs";
            this.buttonSendWhoIs.Size = new System.Drawing.Size(127, 23);
            this.buttonSendWhoIs.TabIndex = 10;
            this.buttonSendWhoIs.Text = "Send Who-Is";
            this.buttonSendWhoIs.UseVisualStyleBackColor = true;
            this.buttonSendWhoIs.Click += new System.EventHandler(this.SendWhoIsButton);
            // 
            // timerUpdateUI
            // 
            this.timerUpdateUI.Enabled = true;
            this.timerUpdateUI.Tick += new System.EventHandler(this.timerUpdateUItick);
            // 
            // Quit
            // 
            this.Quit.Location = new System.Drawing.Point(402, 423);
            this.Quit.Name = "Quit";
            this.Quit.Size = new System.Drawing.Size(75, 23);
            this.Quit.TabIndex = 11;
            this.Quit.Text = "Quit";
            this.Quit.UseVisualStyleBackColor = true;
            this.Quit.Click += new System.EventHandler(this.Quit_Click);
            // 
            // whoisrouterbtn
            // 
            this.whoisrouterbtn.Location = new System.Drawing.Point(191, 36);
            this.whoisrouterbtn.Name = "whoisrouterbtn";
            this.whoisrouterbtn.Size = new System.Drawing.Size(127, 23);
            this.whoisrouterbtn.TabIndex = 12;
            this.whoisrouterbtn.Text = "Send Who-Is-Router";
            this.whoisrouterbtn.UseVisualStyleBackColor = true;
            this.whoisrouterbtn.Click += new System.EventHandler(this.whoisrouterbtn_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(191, 65);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(127, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "Query Routing Tables";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.Initialize_Routing_Table_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(43, 35);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(127, 23);
            this.button5.TabIndex = 14;
            this.button5.Text = "Send Short Who-Is ";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // buttonReadPropertyTest
            // 
            this.buttonReadPropertyTest.Location = new System.Drawing.Point(0, 0);
            this.buttonReadPropertyTest.Name = "buttonReadPropertyTest";
            this.buttonReadPropertyTest.Size = new System.Drawing.Size(75, 23);
            this.buttonReadPropertyTest.TabIndex = 0;
            // 
            // timerHeartbeatWhoIs
            // 
            this.timerHeartbeatWhoIs.Interval = 20000;
            this.timerHeartbeatWhoIs.Tick += new System.EventHandler(this.SendWhoIs_Tick);
            // 
            // buttonSendReadProperty
            // 
            this.buttonSendReadProperty.Location = new System.Drawing.Point(43, 171);
            this.buttonSendReadProperty.Name = "buttonSendReadProperty";
            this.buttonSendReadProperty.Size = new System.Drawing.Size(127, 23);
            this.buttonSendReadProperty.TabIndex = 15;
            this.buttonSendReadProperty.Text = "Send ReadProperty";
            this.buttonSendReadProperty.UseVisualStyleBackColor = true;
            this.buttonSendReadProperty.Click += new System.EventHandler(this.buttonSendReadProperty_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(36, 38);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(110, 17);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "Who-Is Heartbeat";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // mycontextMenuStrip
            // 
            this.mycontextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.whoIsToolStripMenuItem,
            this.readPropertyObjectListToolStripMenuItem});
            this.mycontextMenuStrip.Name = "contextMenuStrip1";
            this.mycontextMenuStrip.Size = new System.Drawing.Size(217, 48);
            this.mycontextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.mycontextMenuStrip_Opening);
            // 
            // whoIsToolStripMenuItem
            // 
            this.whoIsToolStripMenuItem.Name = "whoIsToolStripMenuItem";
            this.whoIsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.whoIsToolStripMenuItem.Text = "Who-Is";
            this.whoIsToolStripMenuItem.Click += new System.EventHandler(this.whoIsToolStripMenuItem_Click);
            // 
            // readPropertyObjectListToolStripMenuItem
            // 
            this.readPropertyObjectListToolStripMenuItem.Name = "readPropertyObjectListToolStripMenuItem";
            this.readPropertyObjectListToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.readPropertyObjectListToolStripMenuItem.Text = "Read Property - Object List";
            this.readPropertyObjectListToolStripMenuItem.Click += new System.EventHandler(this.readPropertyObjectListToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(556, 46);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(349, 262);
            this.tabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button5);
            this.tabPage1.Controls.Add(this.button4);
            this.tabPage1.Controls.Add(this.whoisrouterbtn);
            this.tabPage1.Controls.Add(this.buttonSendReadProperty);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(341, 236);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Test Buttons";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(341, 236);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Connection";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.radioButtonBAC1);
            this.groupBox1.Controls.Add(this.radioButtonBAC0);
            this.groupBox1.Controls.Add(this.radioButtonFDT);
            this.groupBox1.Location = new System.Drawing.Point(50, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(261, 164);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "BACnet Port";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(84, 113);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // radioButtonBAC1
            // 
            this.radioButtonBAC1.AutoSize = true;
            this.radioButtonBAC1.Location = new System.Drawing.Point(23, 72);
            this.radioButtonBAC1.Name = "radioButtonBAC1";
            this.radioButtonBAC1.Size = new System.Drawing.Size(104, 17);
            this.radioButtonBAC1.TabIndex = 1;
            this.radioButtonBAC1.Text = "0xBAC1 / 47809";
            this.radioButtonBAC1.UseVisualStyleBackColor = true;
            this.radioButtonBAC1.Click += new System.EventHandler(this.radioButtonBAC1_Click);
            // 
            // radioButtonBAC0
            // 
            this.radioButtonBAC0.AutoSize = true;
            this.radioButtonBAC0.Checked = true;
            this.radioButtonBAC0.Location = new System.Drawing.Point(23, 36);
            this.radioButtonBAC0.Name = "radioButtonBAC0";
            this.radioButtonBAC0.Size = new System.Drawing.Size(104, 17);
            this.radioButtonBAC0.TabIndex = 0;
            this.radioButtonBAC0.TabStop = true;
            this.radioButtonBAC0.Text = "0xBAC0 / 47808";
            this.radioButtonBAC0.UseVisualStyleBackColor = true;
            this.radioButtonBAC0.Click += new System.EventHandler(this.radioButtonBAC0_Click);
            // 
            // radioButtonFDT
            // 
            this.radioButtonFDT.Location = new System.Drawing.Point(23, 110);
            this.radioButtonFDT.Name = "radioButtonFDT";
            this.radioButtonFDT.Size = new System.Drawing.Size(104, 24);
            this.radioButtonFDT.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.checkBox1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(341, 236);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Settings";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textBoxDiagnosticLog
            // 
            this.textBoxDiagnosticLog.Location = new System.Drawing.Point(556, 340);
            this.textBoxDiagnosticLog.Multiline = true;
            this.textBoxDiagnosticLog.Name = "textBoxDiagnosticLog";
            this.textBoxDiagnosticLog.Size = new System.Drawing.Size(345, 316);
            this.textBoxDiagnosticLog.TabIndex = 15;
            // 
            // labelLog
            // 
            this.labelLog.AutoSize = true;
            this.labelLog.Location = new System.Drawing.Point(557, 324);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(25, 13);
            this.labelLog.TabIndex = 16;
            this.labelLog.Text = "Log";
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(402, 273);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 23);
            this.buttonClear.TabIndex = 18;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonMinimize
            // 
            this.buttonMinimize.Location = new System.Drawing.Point(402, 394);
            this.buttonMinimize.Name = "buttonMinimize";
            this.buttonMinimize.Size = new System.Drawing.Size(75, 23);
            this.buttonMinimize.TabIndex = 19;
            this.buttonMinimize.Text = "Minimize";
            this.buttonMinimize.UseVisualStyleBackColor = true;
            this.buttonMinimize.Click += new System.EventHandler(this.buttonMinimize_Click);
            // 
            // contextMenuStripForRouter
            // 
            this.contextMenuStripForRouter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.readRouterTableToolStripMenuItem});
            this.contextMenuStripForRouter.Name = "contextMenuStrip1";
            this.contextMenuStripForRouter.Size = new System.Drawing.Size(217, 70);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(216, 22);
            this.toolStripMenuItem1.Text = "Who-Is";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.whoIsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(216, 22);
            this.toolStripMenuItem2.Text = "Read Property - Object List";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.readPropertyObjectListToolStripMenuItem_Click);
            // 
            // readRouterTableToolStripMenuItem
            // 
            this.readRouterTableToolStripMenuItem.Name = "readRouterTableToolStripMenuItem";
            this.readRouterTableToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.readRouterTableToolStripMenuItem.Text = "Read Router Table";
            this.readRouterTableToolStripMenuItem.Click += new System.EventHandler(this.readRouterTableToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 668);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonSendWhoIs);
            this.Controls.Add(this.labelLog);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.Quit);
            this.Controls.Add(this.TreeViewDevices);
            this.Controls.Add(this.textBoxDiagnosticLog);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(522, 465);
            this.Name = "MainForm";
            this.Text = "BACnet Browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainform_closing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mycontextMenuStrip.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.contextMenuStripForRouter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView TreeViewDevices;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton helpToolStripButton;
        private System.Windows.Forms.Button buttonSendWhoIs;
        private System.Windows.Forms.Timer timerUpdateUI;
        private System.Windows.Forms.Button Quit;
        private System.Windows.Forms.Button whoisrouterbtn;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button buttonReadPropertyTest;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Timer timerHeartbeatWhoIs;
        private System.Windows.Forms.Button buttonSendReadProperty;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip mycontextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem whoIsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readPropertyObjectListToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RadioButton radioButtonBAC1;
        private System.Windows.Forms.RadioButton radioButtonBAC0;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radioButtonFDT;
        private System.Windows.Forms.TextBox textBoxDiagnosticLog;
        private System.Windows.Forms.Label labelLog;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonMinimize;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForRouter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem readRouterTableToolStripMenuItem;
    }
}

