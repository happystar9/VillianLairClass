/*
TODO: Implement Evil Scheme Management Form

    Requirements:
    - DataGridView with color-coded status
    - Success likelihood calculation in UI (anti-pattern)
    - Budget validation in button handler (anti-pattern)
    - Status transition logic in ComboBox event (anti-pattern)
*/

using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using VillainLairManager.Models;
using VillainLairManager.Services;
using VillainLairManager.Repositories;
using System.Linq;

namespace VillainLairManager.Forms;

public partial class SchemeManagementForm : Form
{
    private readonly SchemeService _schemeService;
    private readonly MinionService _minionService;
    private readonly EquipmentService _equipmentService;
    private DataGridView schemesDataGridView;
    private BindingList<EvilScheme> binding;
    private TextBox txtName;
    private TextBox txtDescription;
    private NumericUpDown numBudget;
    private NumericUpDown numCurrentSpending;
    private NumericUpDown numRequiredSkillLevel;
    private TextBox txtRequiredSpecialty;
    private ComboBox cmbStatus;
    private DateTimePicker dtpStartDate;
    private DateTimePicker dtpTargetCompletion;
    private NumericUpDown numDiabolicalRating;
    private Label lblSuccessLikelihood;
    private Button btnLoadSelected;

    public SchemeManagementForm()
    {
        // Initialize services
        var factory = new RepositoryFactory(AppSettings.Instance.DatabasePath);
        _schemeService = new SchemeService(factory.Schemes, factory.Minions, factory.Equipment);
        _minionService = new MinionService(factory.Minions);
        _equipmentService = new EquipmentService(factory.Equipment, factory.Schemes);
        
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Evil Scheme Management";
        this.Size = new System.Drawing.Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;

        // DataGridView
        schemesDataGridView = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(700, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = false
        };

        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Scheme Name", DataPropertyName = "Name", Width = 150 });
        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status", Width = 80 });
        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Budget", HeaderText = "Budget", DataPropertyName = "Budget", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C0" } });
        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentSpending", HeaderText = "Spending", DataPropertyName = "CurrentSpending", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C0" } });
        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "SuccessLikelihood", HeaderText = "Success %", DataPropertyName = "SuccessLikelihood", Width = 80 });
        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiabolicalRating", HeaderText = "Rating", DataPropertyName = "DiabolicalRating", Width = 60 });
        schemesDataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetCompletionDate", HeaderText = "Target Date", DataPropertyName = "TargetCompletionDate", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } });

        // Apply color-coded status formatting (anti-pattern: UI logic for business state)
        schemesDataGridView.CellFormatting += (sender, e) =>
        {
            if (e.ColumnIndex == schemesDataGridView.Columns["Status"].Index && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Active")
                    e.CellStyle.BackColor = Color.LightGreen;
                else if (status == "Planning")
                    e.CellStyle.BackColor = Color.LightYellow;
                else if (status == "Completed")
                    e.CellStyle.BackColor = Color.LightBlue;
                else if (status == "Failed")
                    e.CellStyle.BackColor = Color.LightCoral;
            }

            // Color code over-budget schemes
            if (e.RowIndex >= 0 && schemesDataGridView.Rows[e.RowIndex].DataBoundItem is EvilScheme scheme)
            {
                if (scheme.CurrentSpending > scheme.Budget && e.ColumnIndex == schemesDataGridView.Columns["CurrentSpending"].Index)
                {
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        };

        // Removed automatic SelectionChanged handler - user will use "Load Selected" button instead

        var schemes = _schemeService.GetAllSchemes().ToList();
        binding = new BindingList<EvilScheme>(schemes);
        schemesDataGridView.DataSource = binding;

        this.Controls.Add(schemesDataGridView);

        // Input controls panel
        int inputX = 740;
        int inputY = 20;
        int labelWidth = 120;
        int inputWidth = 200;
        int rowHeight = 30;

        // Name
        this.Controls.Add(new Label { Text = "Scheme Name:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        txtName = new TextBox { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20) };
        this.Controls.Add(txtName);

        inputY += rowHeight;

        // Description
        this.Controls.Add(new Label { Text = "Description:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        txtDescription = new TextBox { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 60), Multiline = true };
        this.Controls.Add(txtDescription);

        inputY += 70;

        // Budget
        this.Controls.Add(new Label { Text = "Budget:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        numBudget = new NumericUpDown { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Maximum = 100000000, DecimalPlaces = 0 };
        this.Controls.Add(numBudget);

        inputY += rowHeight;

        // Current Spending
        this.Controls.Add(new Label { Text = "Current Spending:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        numCurrentSpending = new NumericUpDown { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Maximum = 100000000, DecimalPlaces = 0 };
        this.Controls.Add(numCurrentSpending);

        inputY += rowHeight;

        // Required Skill Level
        this.Controls.Add(new Label { Text = "Req. Skill Level:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        numRequiredSkillLevel = new NumericUpDown { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Minimum = 1, Maximum = 10, Value = 5 };
        this.Controls.Add(numRequiredSkillLevel);

        inputY += rowHeight;

        // Required Specialty
        this.Controls.Add(new Label { Text = "Req. Specialty:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        txtRequiredSpecialty = new TextBox { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20) };
        this.Controls.Add(txtRequiredSpecialty);

        inputY += rowHeight;

        // Status
        this.Controls.Add(new Label { Text = "Status:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        cmbStatus = new ComboBox { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbStatus.Items.AddRange(new object[] { "Planning", "Active", "Completed", "Failed", "On Hold" });
        cmbStatus.SelectedIndex = 0;
        // Status transition logic in ComboBox event (anti-pattern)
        cmbStatus.SelectedIndexChanged += (sender, e) =>
        {
            // Business logic in UI event handler (anti-pattern)
            if (cmbStatus.SelectedItem.ToString() == "Active" && dtpStartDate.Value == DateTime.Today)
            {
                dtpStartDate.Value = DateTime.Now;
            }
        };
        this.Controls.Add(cmbStatus);

        inputY += rowHeight;

        // Start Date
        this.Controls.Add(new Label { Text = "Start Date:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        dtpStartDate = new DateTimePicker { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Format = DateTimePickerFormat.Short };
        this.Controls.Add(dtpStartDate);

        inputY += rowHeight;

        // Target Completion
        this.Controls.Add(new Label { Text = "Target Date:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        dtpTargetCompletion = new DateTimePicker { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Format = DateTimePickerFormat.Short };
        dtpTargetCompletion.Value = DateTime.Now.AddMonths(6);
        this.Controls.Add(dtpTargetCompletion);

        inputY += rowHeight;

        // Diabolical Rating
        this.Controls.Add(new Label { Text = "Diabolical Rating:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        numDiabolicalRating = new NumericUpDown { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Minimum = 1, Maximum = 10, Value = 5 };
        this.Controls.Add(numDiabolicalRating);

        inputY += rowHeight;

        // Success Likelihood (calculated - anti-pattern: calculation in UI)
        this.Controls.Add(new Label { Text = "Success Likelihood:", Location = new Point(inputX, inputY), Size = new Size(labelWidth, 20) });
        lblSuccessLikelihood = new Label { Location = new Point(inputX + labelWidth, inputY), Size = new Size(inputWidth, 20), Text = "0%", Font = new Font("Arial", 10, FontStyle.Bold) };
        this.Controls.Add(lblSuccessLikelihood);

        inputY += rowHeight + 10;

        // Buttons
        Button addButton = new Button
        {
            Text = "Add Scheme",
            Location = new Point(inputX, inputY),
            Size = new Size(150, 30)
        };
        addButton.Click += AddButton_Click;
        this.Controls.Add(addButton);

        btnLoadSelected = new Button
        {
            Text = "Load Selected",
            Location = new Point(inputX + 160, inputY),
            Size = new Size(150, 30),
            BackColor = Color.LightYellow
        };
        btnLoadSelected.Click += LoadSelectedButton_Click;
        this.Controls.Add(btnLoadSelected);

        inputY += 40;

        Button updateButton = new Button
        {
            Text = "Update Scheme",
            Location = new Point(inputX, inputY),
            Size = new Size(150, 30)
        };
        updateButton.Click += UpdateButton_Click;
        this.Controls.Add(updateButton);

        Button clearButton = new Button
        {
            Text = "Clear Form",
            Location = new Point(inputX + 160, inputY),
            Size = new Size(150, 30)
        };
        clearButton.Click += ClearButton_Click;
        this.Controls.Add(clearButton);

        inputY += 40;

        Button deleteButton = new Button
        {
            Text = "Delete Scheme",
            Location = new Point(inputX, inputY),
            Size = new Size(150, 30)
        };
        deleteButton.Click += DeleteButton_Click;
        this.Controls.Add(deleteButton);

        Button refreshButton = new Button
        {
            Text = "Refresh",
            Location = new Point(inputX + 160, inputY),
            Size = new Size(150, 30)
        };
        refreshButton.Click += RefreshButton_Click;
        this.Controls.Add(refreshButton);

        inputY += 40;

        Button calculateButton = new Button
        {
            Text = "Recalculate Success",
            Location = new Point(inputX, inputY),
            Size = new Size(200, 30),
            BackColor = Color.LightBlue
        };
        calculateButton.Click += CalculateButton_Click;
        this.Controls.Add(calculateButton);
    }

    private void LoadSelectedButton_Click(object sender, EventArgs e)
    {
        if (schemesDataGridView.SelectedRows.Count > 0 && schemesDataGridView.SelectedRows[0].DataBoundItem is EvilScheme scheme)
        {
            txtName.Text = scheme.Name;
            txtDescription.Text = scheme.Description;
            numBudget.Value = scheme.Budget;
            numCurrentSpending.Value = scheme.CurrentSpending;
            numRequiredSkillLevel.Value = scheme.RequiredSkillLevel;
            txtRequiredSpecialty.Text = scheme.RequiredSpecialty;
            cmbStatus.SelectedItem = scheme.Status;
            if (scheme.StartDate.HasValue)
                dtpStartDate.Value = scheme.StartDate.Value;
            else
                dtpStartDate.Value = DateTime.Now;
            dtpTargetCompletion.Value = scheme.TargetCompletionDate;
            numDiabolicalRating.Value = scheme.DiabolicalRating;
            lblSuccessLikelihood.Text = $"{scheme.SuccessLikelihood}%";

            // Color code the success likelihood
            if (scheme.SuccessLikelihood >= AppSettings.Instance.SuccessLikelihoodHighThreshold)
                lblSuccessLikelihood.ForeColor = Color.Green;
            else if (scheme.SuccessLikelihood >= AppSettings.Instance.SuccessLikelihoodMediumThreshold)
                lblSuccessLikelihood.ForeColor = Color.Orange;
            else
                lblSuccessLikelihood.ForeColor = Color.Red;
        }
        else
        {
            MessageBox.Show("Please select a scheme from the grid.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearButton_Click(object sender, EventArgs e)
    {
        txtName.Text = string.Empty;
        txtDescription.Text = string.Empty;
        numBudget.Value = 0;
        numCurrentSpending.Value = 0;
        numRequiredSkillLevel.Value = 5;
        txtRequiredSpecialty.Text = string.Empty;
        cmbStatus.SelectedIndex = 0;
        dtpStartDate.Value = DateTime.Now;
        dtpTargetCompletion.Value = DateTime.Now.AddMonths(6);
        numDiabolicalRating.Value = 5;
        lblSuccessLikelihood.Text = "0%";
        lblSuccessLikelihood.ForeColor = Color.Black;
        schemesDataGridView.ClearSelection();
    }

    private void SchemesDataGridView_SelectionChanged(object sender, EventArgs e)
    {
        // This method is no longer used - removed automatic loading behavior
        // Users must click "Load Selected" button to populate the form
    }

    private void AddButton_Click(object sender, EventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Scheme name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtRequiredSpecialty.Text))
        {
            MessageBox.Show("Required specialty is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Validate specialty using service
        if (!_schemeService.IsValidSpecialty(txtRequiredSpecialty.Text))
        {
            MessageBox.Show($"Invalid specialty: {txtRequiredSpecialty.Text}\n\nValid specialties: {string.Join(", ", AppSettings.Instance.ValidSpecialties)}",
                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Validate skill level using service
        if (!_schemeService.IsValidSkillLevel((int)numRequiredSkillLevel.Value))
        {
            MessageBox.Show("Skill level must be between 1 and 10!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (dtpTargetCompletion.Value <= DateTime.Now)
        {
            MessageBox.Show("Target completion date must be in the future!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            var newScheme = new EvilScheme
            {
                Name = txtName.Text,
                Description = txtDescription.Text,
                Budget = numBudget.Value,
                CurrentSpending = numCurrentSpending.Value,
                RequiredSkillLevel = (int)numRequiredSkillLevel.Value,
                RequiredSpecialty = txtRequiredSpecialty.Text,
                Status = cmbStatus.SelectedItem.ToString(),
                StartDate = cmbStatus.SelectedItem.ToString() == "Active" ? (DateTime?)dtpStartDate.Value : null,
                TargetCompletionDate = dtpTargetCompletion.Value,
                DiabolicalRating = (int)numDiabolicalRating.Value,
                SuccessLikelihood = 0 // Will be calculated by service
            };

            // Use service to create scheme
            _schemeService.CreateScheme(newScheme);

            // Calculate success likelihood using service
            newScheme.SuccessLikelihood = _schemeService.CalculateSuccessLikelihood(newScheme);
            _schemeService.UpdateScheme(newScheme);

            // Check if over budget and warn
            if (_schemeService.IsSchemeOverBudget(newScheme))
            {
                MessageBox.Show("Warning: This scheme is over budget! Success likelihood has been reduced.",
                    "Budget Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Reload to get the ID
            var schemes = _schemeService.GetAllSchemes().ToList();
            binding.Clear();
            foreach (var s in schemes)
                binding.Add(s);

            MessageBox.Show("Scheme added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add scheme: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateButton_Click(object sender, EventArgs e)
    {
        if (schemesDataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a scheme to update.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var scheme = schemesDataGridView.SelectedRows[0].DataBoundItem as EvilScheme;
        if (scheme == null)
            return;

        // Validation
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Scheme name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Validate specialty using service
        if (!_schemeService.IsValidSpecialty(txtRequiredSpecialty.Text))
        {
            MessageBox.Show($"Invalid specialty: {txtRequiredSpecialty.Text}\n\nValid specialties: {string.Join(", ", AppSettings.Instance.ValidSpecialties)}",
                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            scheme.Name = txtName.Text;
            scheme.Description = txtDescription.Text;
            scheme.Budget = numBudget.Value;
            scheme.CurrentSpending = numCurrentSpending.Value;
            scheme.RequiredSkillLevel = (int)numRequiredSkillLevel.Value;
            scheme.RequiredSpecialty = txtRequiredSpecialty.Text;
            scheme.Status = cmbStatus.SelectedItem.ToString();
            scheme.StartDate = cmbStatus.SelectedItem.ToString() == "Active" ? (DateTime?)dtpStartDate.Value : null;
            scheme.TargetCompletionDate = dtpTargetCompletion.Value;
            scheme.DiabolicalRating = (int)numDiabolicalRating.Value;
            
            // Calculate success likelihood using service
            scheme.SuccessLikelihood = _schemeService.CalculateSuccessLikelihood(scheme);

            // Use service to update scheme
            _schemeService.UpdateScheme(scheme);

            // Check if over budget and warn
            if (_schemeService.IsSchemeOverBudget(scheme))
            {
                MessageBox.Show("Warning: This scheme is over budget!",
                    "Budget Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            binding.ResetBindings();
            schemesDataGridView.Refresh();

            MessageBox.Show("Scheme updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update scheme: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
        var toDelete = new List<EvilScheme>();
        foreach (DataGridViewRow row in schemesDataGridView.SelectedRows)
        {
            if (row?.DataBoundItem is EvilScheme s)
                toDelete.Add(s);
        }

        if (toDelete.Count == 0)
            return;

        var result = MessageBox.Show($"Are you sure you want to delete {toDelete.Count} scheme(s)? This cannot be undone.", 
            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
            return;

        try
        {
            foreach (var scheme in toDelete)
            {
                if (scheme.SchemeId > 0)
                {
                    _schemeService.DeleteScheme(scheme.SchemeId);
                }
                binding.Remove(scheme);
            }

            MessageBox.Show("Scheme(s) deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete scheme: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshButton_Click(object sender, EventArgs e)
    {
        try
        {
            var schemes = _schemeService.GetAllSchemes().ToList();
            binding.Clear();
            foreach (var scheme in schemes)
            {
                binding.Add(scheme);
            }

            MessageBox.Show("Data refreshed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to refresh data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CalculateButton_Click(object sender, EventArgs e)
    {
        if (schemesDataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a scheme to recalculate.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var scheme = schemesDataGridView.SelectedRows[0].DataBoundItem as EvilScheme;
        if (scheme == null)
            return;

        // Use service to calculate success likelihood
        int newSuccessLikelihood = _schemeService.CalculateSuccessLikelihood(scheme);
        scheme.SuccessLikelihood = newSuccessLikelihood;
        lblSuccessLikelihood.Text = $"{newSuccessLikelihood}%";

        // Color code the success likelihood
        if (newSuccessLikelihood >= AppSettings.Instance.SuccessLikelihoodHighThreshold)
            lblSuccessLikelihood.ForeColor = Color.Green;
        else if (newSuccessLikelihood >= AppSettings.Instance.SuccessLikelihoodMediumThreshold)
            lblSuccessLikelihood.ForeColor = Color.Orange;
        else
            lblSuccessLikelihood.ForeColor = Color.Red;

        binding.ResetBindings();
        schemesDataGridView.Refresh();

        MessageBox.Show($"Success likelihood recalculated: {newSuccessLikelihood}%", "Calculation Complete", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

