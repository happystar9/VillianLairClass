using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using VillainLairManager.Models;

namespace VillainLairManager.Forms;
/// <summary>
/// Minion management form - STUB for students to implement
/// Should contain CRUD operations with business logic in event handlers
/// </summary>
public partial class MinionManagementForm : Form
{
    public MinionManagementForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Minion Management";
        this.Size = new System.Drawing.Size(900, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Text = "TODO: Implement Minion Management Form\n\n" +
        //        "Requirements:\n" +
        //        "- DataGridView showing all minions\n" +
        //        "- Text boxes for: Name, Specialty, Skill Level, Salary\n" +
        //        "- ComboBox for Base assignment\n" +
        //        "- ComboBox for Scheme assignment\n" +
        //        "- Buttons: Add, Update, Delete, Refresh\n" +
        //        "- All validation logic in button click handlers (anti-pattern)\n" +
        //        "- Direct database calls from event handlers (anti-pattern)\n" +
        //        "- Loyalty calculation duplicated here (anti-pattern)",

        var minionsDataGridView = new DataGridView
        {
            Location = new Point(20, 50),
            Size = new Size(800, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = false
        };

        minionsDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name" });
        minionsDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Specialty", HeaderText = "Specialty", DataPropertyName = "Specialty" });
        minionsDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "SkillLevel", HeaderText = "Skill Level", DataPropertyName = "SkillLevel" });
        minionsDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "SalaryDemand", HeaderText = "Salary", DataPropertyName = "SalaryDemand" });
        minionsDataGridView.Columns.Add(new DataGridViewComboBoxColumn { Name = "BaseAssignment", HeaderText = "Base", DataPropertyName = "BaseAssignment" });
        minionsDataGridView.Columns.Add(new DataGridViewComboBoxColumn { Name = "SchemeAssignment", HeaderText = "Scheme", DataPropertyName = "SchemeAssignment" });



        var minions = DatabaseHelper.GetAllMinions();
        var binding = new BindingList<Minion>(minions);
        minionsDataGridView.DataSource = binding;

        this.Controls.Add(minionsDataGridView);
        Button addButton = new Button
        {
            Text = "Add",
            Location = new Point(20, 470),
            Size = new Size(100, 30)
        };
        addButton.Click += (sender, e) =>
        {
            var newMinion = new Minion
            {
            Name = "New Minion",
            Specialty = "Unknown",
            SkillLevel = 1,
            SalaryDemand = 1000
            };

            try
            {

            // Update bound list / DataGridView
            binding.Add(newMinion);

            // Select and scroll to the newly added row
            minionsDataGridView.ClearSelection();
            var index = binding.IndexOf(newMinion);
            if (index >= 0)
            {
                minionsDataGridView.Rows[index].Selected = true;
                minionsDataGridView.FirstDisplayedScrollingRowIndex = index;
            }
            }
            catch (Exception ex)
            {
            MessageBox.Show("Failed to add minion: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        this.Controls.Add(addButton);

        Button updateButton = new Button
        {
            Text = "Update",
            Location = new Point(140, 470),
            Size = new Size(100, 30)
        };
        updateButton.Click += (sender, e) =>
        {
            // TODO: Implement update minion logic
        };
        this.Controls.Add(updateButton);

        Button deleteButton = new Button
        {
            Text = "Delete",
            Location = new Point(260, 470),
            Size = new Size(100, 30)
        };
        deleteButton.Click += (sender, e) =>
        {
            // TODO: Implement delete minion logic
        };
        this.Controls.Add(deleteButton);

        Button refreshButton = new Button
        {
            Text = "Refresh",
            Location = new Point(380, 470),
            Size = new Size(100, 30)
        };
        refreshButton.Click += (sender, e) =>
        {
            // TODO: Implement refresh minion logic
        };
        this.Controls.Add(refreshButton);
    }
}
