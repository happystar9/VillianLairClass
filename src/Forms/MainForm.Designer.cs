using System;
using System.Drawing;
using System.Windows.Forms;

namespace VillainLairManager.Forms
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
            this.btnMinions = new System.Windows.Forms.Button();
            this.btnSchemes = new System.Windows.Forms.Button();
            this.btnBases = new System.Windows.Forms.Button();
            this.btnEquipment = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlStats = new System.Windows.Forms.Panel();
            this.lblMinionStats = new System.Windows.Forms.Label();
            this.lblSchemeStats = new System.Windows.Forms.Label();
            this.lblCostStats = new System.Windows.Forms.Label();
            this.pnlAlerts = new System.Windows.Forms.Panel();
            this.lblAlerts = new System.Windows.Forms.Label();
            this.pnlStats.SuspendLayout();
            this.pnlAlerts.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMinions
            // 
            this.btnMinions.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnMinions.Location = new System.Drawing.Point(100, 100);
            this.btnMinions.Name = "btnMinions";
            this.btnMinions.Size = new System.Drawing.Size(250, 80);
            this.btnMinions.TabIndex = 0;
            this.btnMinions.Text = "Manage Minions";
            this.btnMinions.UseVisualStyleBackColor = true;
            this.btnMinions.Click += new System.EventHandler(this.btnMinions_Click);
            // 
            // btnSchemes
            // 
            this.btnSchemes.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSchemes.Location = new System.Drawing.Point(450, 100);
            this.btnSchemes.Name = "btnSchemes";
            this.btnSchemes.Size = new System.Drawing.Size(250, 80);
            this.btnSchemes.TabIndex = 1;
            this.btnSchemes.Text = "Manage Evil Schemes";
            this.btnSchemes.UseVisualStyleBackColor = true;
            this.btnSchemes.Click += new System.EventHandler(this.btnSchemes_Click);
            // 
            // btnBases
            // 
            this.btnBases.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnBases.Location = new System.Drawing.Point(100, 200);
            this.btnBases.Name = "btnBases";
            this.btnBases.Size = new System.Drawing.Size(250, 80);
            this.btnBases.TabIndex = 2;
            this.btnBases.Text = "Manage Secret Bases";
            this.btnBases.UseVisualStyleBackColor = true;
            this.btnBases.Click += new System.EventHandler(this.btnBases_Click);
            // 
            // btnEquipment
            // 
            this.btnEquipment.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnEquipment.Location = new System.Drawing.Point(450, 200);
            this.btnEquipment.Name = "btnEquipment";
            this.btnEquipment.Size = new System.Drawing.Size(250, 80);
            this.btnEquipment.TabIndex = 3;
            this.btnEquipment.Text = "Equipment Inventory";
            this.btnEquipment.UseVisualStyleBackColor = true;
            this.btnEquipment.Click += new System.EventHandler(this.btnEquipment_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.Location = new System.Drawing.Point(150, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(500, 40);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "?? Super Villain Lair Management System ??";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStats
            // 
            this.pnlStats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlStats.Controls.Add(this.lblMinionStats);
            this.pnlStats.Controls.Add(this.lblSchemeStats);
            this.pnlStats.Controls.Add(this.lblCostStats);
            this.pnlStats.Location = new System.Drawing.Point(50, 320);
            this.pnlStats.Name = "pnlStats";
            this.pnlStats.Size = new System.Drawing.Size(700, 150);
            this.pnlStats.TabIndex = 5;
            // 
            // lblMinionStats
            // 
            this.lblMinionStats.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMinionStats.Location = new System.Drawing.Point(10, 10);
            this.lblMinionStats.Name = "lblMinionStats";
            this.lblMinionStats.Size = new System.Drawing.Size(680, 30);
            this.lblMinionStats.TabIndex = 0;
            // 
            // lblSchemeStats
            // 
            this.lblSchemeStats.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSchemeStats.Location = new System.Drawing.Point(10, 50);
            this.lblSchemeStats.Name = "lblSchemeStats";
            this.lblSchemeStats.Size = new System.Drawing.Size(680, 30);
            this.lblSchemeStats.TabIndex = 1;
            // 
            // lblCostStats
            // 
            this.lblCostStats.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCostStats.Location = new System.Drawing.Point(10, 90);
            this.lblCostStats.Name = "lblCostStats";
            this.lblCostStats.Size = new System.Drawing.Size(680, 30);
            this.lblCostStats.TabIndex = 2;
            // 
            // pnlAlerts
            // 
            this.pnlAlerts.BackColor = System.Drawing.Color.LightYellow;
            this.pnlAlerts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAlerts.Controls.Add(this.lblAlerts);
            this.pnlAlerts.Location = new System.Drawing.Point(50, 480);
            this.pnlAlerts.Name = "pnlAlerts";
            this.pnlAlerts.Size = new System.Drawing.Size(700, 80);
            this.pnlAlerts.TabIndex = 6;
            // 
            // lblAlerts
            // 
            this.lblAlerts.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblAlerts.ForeColor = System.Drawing.Color.DarkRed;
            this.lblAlerts.Location = new System.Drawing.Point(10, 10);
            this.lblAlerts.Name = "lblAlerts";
            this.lblAlerts.Size = new System.Drawing.Size(680, 60);
            this.lblAlerts.TabIndex = 0;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.pnlAlerts);
            this.Controls.Add(this.pnlStats);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnEquipment);
            this.Controls.Add(this.btnBases);
            this.Controls.Add(this.btnSchemes);
            this.Controls.Add(this.btnMinions);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Super Villain Lair Management System";
            this.pnlStats.ResumeLayout(false);
            this.pnlAlerts.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Button btnMinions;
        private Button btnSchemes;
        private Button btnBases;
        private Button btnEquipment;
        private Label lblTitle;
        private Panel pnlStats;
        private Label lblMinionStats;
        private Label lblSchemeStats;
        private Label lblCostStats;
        private Panel pnlAlerts;
        private Label lblAlerts;
    }
}
