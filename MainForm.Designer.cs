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
            this.buttonReadPropertyTest = new System.Windows.Forms.Button();
            this.timerHeartbeatWhoIs = new System.Windows.Forms.Timer(this.components);
            this.buttonSendReadProperty = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.mycontextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.whoIsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readPropertyObjectListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControlLogs = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonWhoIsLongMAC = new System.Windows.Forms.Button();
            this.buttonSendIAm = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.radioButtonBAC1 = new System.Windows.Forms.RadioButton();
            this.radioButtonBAC0 = new System.Windows.Forms.RadioButton();
            this.radioButtonFDT = new System.Windows.Forms.RadioButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxDeviceInstance = new System.Windows.Forms.TextBox();
            this.tabPageErrors = new System.Windows.Forms.TabPage();
            this.textBoxPanics = new System.Windows.Forms.TextBox();
            this.tabPageProtocol = new System.Windows.Forms.TabPage();
            this.buttonClearProtocol = new System.Windows.Forms.Button();
            this.textBoxProtocol = new System.Windows.Forms.TextBox();
            this.buttonPrepNewTests = new System.Windows.Forms.Button();
            this.buttonStartTests = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.contextMenuStripForRouter = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.readRouterTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripForObject = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.readPresentValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readObjectNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorkerDiagnosticManager = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStripForDiagnostic = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.runDiagnosticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diagnosticDescriptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonDiscover = new System.Windows.Forms.Button();
            this.toolStrip1.SuspendLayout();
            this.mycontextMenuStrip.SuspendLayout();
            this.tabControlLogs.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPageErrors.SuspendLayout();
            this.tabPageProtocol.SuspendLayout();
            this.contextMenuStripForRouter.SuspendLayout();
            this.contextMenuStripForObject.SuspendLayout();
            this.contextMenuStripForDiagnostic.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeViewDevices
            // 
            this.TreeViewDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.TreeViewDevices.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeViewDevices.HideSelection = false;
            this.TreeViewDevices.Location = new System.Drawing.Point(12, 45);
            this.TreeViewDevices.Name = "TreeViewDevices";
            this.TreeViewDevices.Size = new System.Drawing.Size(556, 470);
            this.TreeViewDevices.TabIndex = 2;
            this.TreeViewDevices.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.BACnetInternetworkTreeView_NodeMouseClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(589, 202);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(73, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Expand All";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ExpandAllButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(589, 231);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(73, 23);
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
            this.toolStrip1.Size = new System.Drawing.Size(1064, 25);
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
            this.buttonSendWhoIs.Location = new System.Drawing.Point(191, 89);
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
            this.Quit.Location = new System.Drawing.Point(589, 411);
            this.Quit.Name = "Quit";
            this.Quit.Size = new System.Drawing.Size(73, 23);
            this.Quit.TabIndex = 11;
            this.Quit.Text = "Quit";
            this.Quit.UseVisualStyleBackColor = true;
            this.Quit.Click += new System.EventHandler(this.Quit_Click);
            // 
            // whoisrouterbtn
            // 
            this.whoisrouterbtn.Location = new System.Drawing.Point(191, 119);
            this.whoisrouterbtn.Name = "whoisrouterbtn";
            this.whoisrouterbtn.Size = new System.Drawing.Size(127, 23);
            this.whoisrouterbtn.TabIndex = 12;
            this.whoisrouterbtn.Text = "Send Who-Is-Router";
            this.whoisrouterbtn.UseVisualStyleBackColor = true;
            this.whoisrouterbtn.Click += new System.EventHandler(this.whoisrouterbtn_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(191, 148);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(127, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "Query Routing Tables";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Initialize_Routing_Table_Click);
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
            this.buttonSendReadProperty.Location = new System.Drawing.Point(191, 176);
            this.buttonSendReadProperty.Name = "buttonSendReadProperty";
            this.buttonSendReadProperty.Size = new System.Drawing.Size(127, 23);
            this.buttonSendReadProperty.TabIndex = 15;
            this.buttonSendReadProperty.Text = "Send ReadProperty";
            this.buttonSendReadProperty.UseVisualStyleBackColor = true;
            this.buttonSendReadProperty.Visible = false;
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
            // tabControlLogs
            // 
            this.tabControlLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlLogs.Controls.Add(this.tabPage1);
            this.tabControlLogs.Controls.Add(this.tabPage2);
            this.tabControlLogs.Controls.Add(this.tabPage3);
            this.tabControlLogs.Controls.Add(this.tabPageErrors);
            this.tabControlLogs.Controls.Add(this.tabPageProtocol);
            this.tabControlLogs.Location = new System.Drawing.Point(695, 45);
            this.tabControlLogs.Name = "tabControlLogs";
            this.tabControlLogs.SelectedIndex = 0;
            this.tabControlLogs.Size = new System.Drawing.Size(334, 470);
            this.tabControlLogs.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonWhoIsLongMAC);
            this.tabPage1.Controls.Add(this.buttonSendIAm);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.button4);
            this.tabPage1.Controls.Add(this.whoisrouterbtn);
            this.tabPage1.Controls.Add(this.buttonSendReadProperty);
            this.tabPage1.Controls.Add(this.buttonSendWhoIs);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(326, 444);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Test Buttons";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // buttonWhoIsLongMAC
            // 
            this.buttonWhoIsLongMAC.Location = new System.Drawing.Point(191, 235);
            this.buttonWhoIsLongMAC.Name = "buttonWhoIsLongMAC";
            this.buttonWhoIsLongMAC.Size = new System.Drawing.Size(127, 23);
            this.buttonWhoIsLongMAC.TabIndex = 24;
            this.buttonWhoIsLongMAC.Text = "Who-Is with long MAC";
            this.buttonWhoIsLongMAC.UseVisualStyleBackColor = true;
            this.buttonWhoIsLongMAC.Click += new System.EventHandler(this.buttonWhoIsLongMAC_Click);
            // 
            // buttonSendIAm
            // 
            this.buttonSendIAm.Location = new System.Drawing.Point(191, 206);
            this.buttonSendIAm.Name = "buttonSendIAm";
            this.buttonSendIAm.Size = new System.Drawing.Size(127, 23);
            this.buttonSendIAm.TabIndex = 23;
            this.buttonSendIAm.Text = "Send I-Am";
            this.buttonSendIAm.UseVisualStyleBackColor = true;
            this.buttonSendIAm.Click += new System.EventHandler(this.buttonSendIAm_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(68, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(182, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "unless you know what you are doing.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(68, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(182, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Diagnostic buttons. Do not use these";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(326, 444);
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
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.textBoxDeviceInstance);
            this.tabPage3.Controls.Add(this.checkBox1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(326, 444);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Settings";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(156, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Device Instance for this Device";
            // 
            // textBoxDeviceInstance
            // 
            this.textBoxDeviceInstance.Location = new System.Drawing.Point(36, 103);
            this.textBoxDeviceInstance.Name = "textBoxDeviceInstance";
            this.textBoxDeviceInstance.Size = new System.Drawing.Size(100, 20);
            this.textBoxDeviceInstance.TabIndex = 17;
            this.textBoxDeviceInstance.Text = "12346";
            // 
            // tabPageErrors
            // 
            this.tabPageErrors.Controls.Add(this.textBoxPanics);
            this.tabPageErrors.Location = new System.Drawing.Point(4, 22);
            this.tabPageErrors.Name = "tabPageErrors";
            this.tabPageErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageErrors.Size = new System.Drawing.Size(326, 444);
            this.tabPageErrors.TabIndex = 4;
            this.tabPageErrors.Text = "Errors";
            this.tabPageErrors.UseVisualStyleBackColor = true;
            // 
            // textBoxPanics
            // 
            this.textBoxPanics.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPanics.Location = new System.Drawing.Point(0, 0);
            this.textBoxPanics.Multiline = true;
            this.textBoxPanics.Name = "textBoxPanics";
            this.textBoxPanics.Size = new System.Drawing.Size(326, 388);
            this.textBoxPanics.TabIndex = 0;
            // 
            // tabPageProtocol
            // 
            this.tabPageProtocol.Controls.Add(this.buttonClearProtocol);
            this.tabPageProtocol.Controls.Add(this.textBoxProtocol);
            this.tabPageProtocol.Location = new System.Drawing.Point(4, 22);
            this.tabPageProtocol.Name = "tabPageProtocol";
            this.tabPageProtocol.Size = new System.Drawing.Size(326, 444);
            this.tabPageProtocol.TabIndex = 3;
            this.tabPageProtocol.Text = "Protocol";
            this.tabPageProtocol.UseVisualStyleBackColor = true;
            // 
            // buttonClearProtocol
            // 
            this.buttonClearProtocol.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonClearProtocol.Location = new System.Drawing.Point(129, 408);
            this.buttonClearProtocol.Name = "buttonClearProtocol";
            this.buttonClearProtocol.Size = new System.Drawing.Size(75, 23);
            this.buttonClearProtocol.TabIndex = 1;
            this.buttonClearProtocol.Text = "Clear";
            this.buttonClearProtocol.UseVisualStyleBackColor = true;
            this.buttonClearProtocol.Click += new System.EventHandler(this.buttonClearProtocol_Click);
            // 
            // textBoxProtocol
            // 
            this.textBoxProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProtocol.Location = new System.Drawing.Point(0, 0);
            this.textBoxProtocol.Multiline = true;
            this.textBoxProtocol.Name = "textBoxProtocol";
            this.textBoxProtocol.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxProtocol.Size = new System.Drawing.Size(326, 388);
            this.textBoxProtocol.TabIndex = 0;
            // 
            // buttonPrepNewTests
            // 
            this.buttonPrepNewTests.Location = new System.Drawing.Point(589, 104);
            this.buttonPrepNewTests.Name = "buttonPrepNewTests";
            this.buttonPrepNewTests.Size = new System.Drawing.Size(89, 23);
            this.buttonPrepNewTests.TabIndex = 25;
            this.buttonPrepNewTests.Text = "Prep Tests";
            this.buttonPrepNewTests.UseVisualStyleBackColor = true;
            this.buttonPrepNewTests.Click += new System.EventHandler(this.buttonPrepNewTests_Click);
            // 
            // buttonStartTests
            // 
            this.buttonStartTests.Location = new System.Drawing.Point(589, 132);
            this.buttonStartTests.Name = "buttonStartTests";
            this.buttonStartTests.Size = new System.Drawing.Size(89, 23);
            this.buttonStartTests.TabIndex = 16;
            this.buttonStartTests.Text = "Start All Tests";
            this.buttonStartTests.UseVisualStyleBackColor = true;
            this.buttonStartTests.Click += new System.EventHandler(this.buttonAllTests_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(589, 261);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(73, 23);
            this.buttonClear.TabIndex = 18;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
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
            // contextMenuStripForObject
            // 
            this.contextMenuStripForObject.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.readPresentValueToolStripMenuItem,
            this.readObjectNameToolStripMenuItem});
            this.contextMenuStripForObject.Name = "contextMenuStripForObject";
            this.contextMenuStripForObject.Size = new System.Drawing.Size(180, 48);
            // 
            // readPresentValueToolStripMenuItem
            // 
            this.readPresentValueToolStripMenuItem.Name = "readPresentValueToolStripMenuItem";
            this.readPresentValueToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.readPresentValueToolStripMenuItem.Text = "Read Present Value";
            // 
            // readObjectNameToolStripMenuItem
            // 
            this.readObjectNameToolStripMenuItem.Name = "readObjectNameToolStripMenuItem";
            this.readObjectNameToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.readObjectNameToolStripMenuItem.Text = "Read Object Name";
            // 
            // backgroundWorkerDiagnosticManager
            // 
            this.backgroundWorkerDiagnosticManager.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerDiagnosticManager_DoWork);
            this.backgroundWorkerDiagnosticManager.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerDiagnosticManager_RunWorkerCompleted);
            // 
            // contextMenuStripForDiagnostic
            // 
            this.contextMenuStripForDiagnostic.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runDiagnosticToolStripMenuItem,
            this.diagnosticDescriptionToolStripMenuItem,
            this.cancelToolStripMenuItem});
            this.contextMenuStripForDiagnostic.Name = "contextMenuStripForDiagnostic";
            this.contextMenuStripForDiagnostic.Size = new System.Drawing.Size(204, 70);
            this.contextMenuStripForDiagnostic.Click += new System.EventHandler(this.contextMenuStripForDiagnostic_Click);
            this.contextMenuStripForDiagnostic.MouseClick += new System.Windows.Forms.MouseEventHandler(this.contextMenuStripForDiagnostic_MouseClick);
            // 
            // runDiagnosticToolStripMenuItem
            // 
            this.runDiagnosticToolStripMenuItem.Name = "runDiagnosticToolStripMenuItem";
            this.runDiagnosticToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.runDiagnosticToolStripMenuItem.Text = "Run Diagnostic";
            // 
            // diagnosticDescriptionToolStripMenuItem
            // 
            this.diagnosticDescriptionToolStripMenuItem.Name = "diagnosticDescriptionToolStripMenuItem";
            this.diagnosticDescriptionToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.diagnosticDescriptionToolStripMenuItem.Text = "Description of Diagnostic";
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.cancelToolStripMenuItem.Text = "Cancel";
            // 
            // buttonDiscover
            // 
            this.buttonDiscover.Location = new System.Drawing.Point(589, 75);
            this.buttonDiscover.Name = "buttonDiscover";
            this.buttonDiscover.Size = new System.Drawing.Size(89, 23);
            this.buttonDiscover.TabIndex = 19;
            this.buttonDiscover.Text = "Discover";
            this.buttonDiscover.UseVisualStyleBackColor = true;
            this.buttonDiscover.Click += new System.EventHandler(this.buttonStartScan_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1064, 536);
            this.Controls.Add(this.buttonPrepNewTests);
            this.Controls.Add(this.buttonDiscover);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.TreeViewDevices);
            this.Controls.Add(this.tabControlLogs);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.Quit);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.buttonStartTests);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(522, 465);
            this.Name = "MainForm";
            this.Text = "BACnet Browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainform_closing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.mycontextMenuStrip.ResumeLayout(false);
            this.tabControlLogs.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPageErrors.ResumeLayout(false);
            this.tabPageErrors.PerformLayout();
            this.tabPageProtocol.ResumeLayout(false);
            this.tabPageProtocol.PerformLayout();
            this.contextMenuStripForRouter.ResumeLayout(false);
            this.contextMenuStripForObject.ResumeLayout(false);
            this.contextMenuStripForDiagnostic.ResumeLayout(false);
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
        private System.Windows.Forms.Timer timerHeartbeatWhoIs;
        private System.Windows.Forms.Button buttonSendReadProperty;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip mycontextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem whoIsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readPropertyObjectListToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControlLogs;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RadioButton radioButtonBAC1;
        private System.Windows.Forms.RadioButton radioButtonBAC0;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radioButtonFDT;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForRouter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem readRouterTableToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForObject;
        private System.Windows.Forms.ToolStripMenuItem readPresentValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readObjectNameToolStripMenuItem;
        private System.Windows.Forms.Button buttonStartTests;
        private System.ComponentModel.BackgroundWorker backgroundWorkerDiagnosticManager;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripForDiagnostic;
        private System.Windows.Forms.TabPage tabPageProtocol;
        private System.Windows.Forms.TextBox textBoxProtocol;
        private System.Windows.Forms.Button buttonClearProtocol;
        private System.Windows.Forms.Button buttonDiscover;
        private System.Windows.Forms.ToolStripMenuItem runDiagnosticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem;
        internal System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSendIAm;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxDeviceInstance;
        private System.Windows.Forms.Button buttonWhoIsLongMAC;
        private System.Windows.Forms.TabPage tabPageErrors;
        private System.Windows.Forms.TextBox textBoxPanics;
        private System.Windows.Forms.Button buttonPrepNewTests;
        private System.Windows.Forms.ToolStripMenuItem diagnosticDescriptionToolStripMenuItem;
    }
}

