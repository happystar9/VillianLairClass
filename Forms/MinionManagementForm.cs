using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
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
            Location = new Point(20, 60),
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

        var nameLabel = new Label { Text = "Name:", Location = new Point(20, 12), AutoSize = true };
        var nameTextBox = new TextBox { Location = new Point(80, 10), Size = new Size(160, 23) };

        var specialtyLabel = new Label { Text = "Specialty:", Location = new Point(250, 12), AutoSize = true };
        var specialtyTextBox = new TextBox { Location = new Point(330, 10), Size = new Size(120, 23) };

        var moodLabel = new Label { Text = "Mood:", Location = new Point(460, 12), AutoSize = true };
        var moodTextBox = new TextBox { Location = new Point(510, 10), Size = new Size(220, 23) };

        var skillLabel = new Label { Text = "Skill:", Location = new Point(460, 35), AutoSize = true };
        var skillUpDown = new NumericUpDown { Location = new Point(510, 33), Size = new Size(60, 23), Minimum = 1, Maximum = 10, Value = 1 };

        var salaryLabel = new Label { Text = "Salary:", Location = new Point(580, 12), AutoSize = true };
        var salaryUpDown = new NumericUpDown { Location = new Point(630, 33), Size = new Size(100, 23), Minimum = 0, Maximum = 1000000, DecimalPlaces = 0, Increment = 100 };

        var baseLabel = new Label { Text = "Base:", Location = new Point(20, 35), AutoSize = true };
        var baseCombo = new ComboBox { Location = new Point(80, 33), Size = new Size(160, 23), DropDownStyle = ComboBoxStyle.DropDownList };

        var schemeLabel = new Label { Text = "Scheme:", Location = new Point(250, 35), AutoSize = true };
        var schemeCombo = new ComboBox { Location = new Point(330, 33), Size = new Size(120, 23), DropDownStyle = ComboBoxStyle.DropDownList };

        var bases = DatabaseHelper.GetAllBases();
        baseCombo.DataSource = bases;
        baseCombo.DisplayMember = "Name";
        baseCombo.ValueMember = "BaseId";
        baseCombo.SelectedIndex = -1;

        var schemes = DatabaseHelper.GetAllSchemes();
        schemeCombo.DataSource = schemes;
        schemeCombo.DisplayMember = "Name";
        schemeCombo.ValueMember = "SchemeId";
        schemeCombo.SelectedIndex = -1;

        this.Controls.Add(nameLabel);
        this.Controls.Add(nameTextBox);
        this.Controls.Add(specialtyLabel);
        this.Controls.Add(specialtyTextBox);
        this.Controls.Add(skillLabel);
        this.Controls.Add(skillUpDown);
        this.Controls.Add(salaryLabel);
        this.Controls.Add(salaryUpDown);
        this.Controls.Add(baseLabel);
        this.Controls.Add(baseCombo);
        this.Controls.Add(schemeLabel);
        this.Controls.Add(schemeCombo);
        this.Controls.Add(moodLabel);
        this.Controls.Add(moodTextBox);
        Button addButton = new Button
        {
            Text = "Add",
            Location = new Point(775, 16),
            Size = new Size(100, 30)
        };
        addButton.Click += (sender, e) =>
        {
            try
            {
                var name = string.IsNullOrWhiteSpace(nameTextBox.Text) ? "New Minion" : nameTextBox.Text.Trim();
                var specialty = string.IsNullOrWhiteSpace(specialtyTextBox.Text) ? "Unknown" : specialtyTextBox.Text.Trim();
                var skill = (int)skillUpDown.Value;
                var salary = (decimal)salaryUpDown.Value;
                var mood = moodTextBox.Text?.Trim();

                int? baseId = null;
                if (baseCombo.SelectedIndex >= 0 && baseCombo.SelectedValue != null)
                    baseId = Convert.ToInt32(baseCombo.SelectedValue);

                int? schemeId = null;
                if (schemeCombo.SelectedIndex >= 0 && schemeCombo.SelectedValue != null)
                    schemeId = Convert.ToInt32(schemeCombo.SelectedValue);

                var newMinion = new Minion
                {
                    MinionId = 0,
                    Name = name,
                    Specialty = specialty,
                    SkillLevel = skill,
                    SalaryDemand = salary,
                    LoyaltyScore = 50,
                    CurrentBaseId = baseId,
                    CurrentSchemeId = schemeId,
                    MoodStatus = string.IsNullOrWhiteSpace(mood) ? "Neutral" : mood,
                    LastMoodUpdate = DateTime.Now
                };

                binding.Add(newMinion);

                // Clear inputs
                nameTextBox.Text = string.Empty;
                specialtyTextBox.Text = string.Empty;
                skillUpDown.Value = 1;
                salaryUpDown.Value = 0;
                baseCombo.SelectedIndex = -1;
                schemeCombo.SelectedIndex = -1;
                moodTextBox.Text = string.Empty;
                nameTextBox.Focus();


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
            try
            {
                var all = binding.ToList();

                foreach (var minion in all)
                {
                    if (minion.MinionId > 0)
                    {
                        DatabaseHelper.UpdateMinion(minion);
                    }
                    else
                    {
                        DatabaseHelper.InsertMinion(minion);
                    }
                }

                MessageBox.Show("All minions saved to database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save minions: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                var toDelete = new List<Minion>();
                foreach (DataGridViewRow row in minionsDataGridView.SelectedRows)
                {
                    if (row?.DataBoundItem is Minion m)
                        toDelete.Add(m);
                }

                if (toDelete.Count == 0)
                    return;

                var result = MessageBox.Show($"Are you sure you want to delete the selected minion(s) This cannot be undone.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return;

                foreach (var minion in toDelete)
                {
                    if (minion.MinionId > 0)
                    {
                        DatabaseHelper.DeleteMinion(minion.MinionId);
                    }

                    binding.Remove(minion);
                }
            
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
            try
            {
                var refreshed = DatabaseHelper.GetAllMinions();
                binding = new BindingList<Minion>(refreshed);
                minionsDataGridView.DataSource = binding;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh minions: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        this.Controls.Add(refreshButton);
    }
}
