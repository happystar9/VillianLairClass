using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;
using VillainLairManager.Services;
using VillainLairManager.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace VillainLairManager.Forms
{
    /// <summary>
    /// Main dashboard form with navigation and statistics
    /// Contains business logic in UI layer (anti-pattern)
    /// </summary>
    public partial class MainForm : Form
    {
        private SchemeService _schemeService;
        private MinionService _minionService;
        private BaseService _baseService;
        private EquipmentService _equipmentService;
        private readonly IServiceProvider _serviceProvider;

        public MainForm(
            SchemeService schemeService,
            MinionService minionService,
            BaseService baseService,
            EquipmentService equipmentService,
            IServiceProvider serviceProvider)
        {
            _schemeService = schemeService;
            _minionService = minionService;
            _baseService = baseService;
            _equipmentService = equipmentService;
            _serviceProvider = serviceProvider;

            InitializeComponent();
            LoadStatistics();
        }

        private void btnMinions_Click(object sender, EventArgs e)
        {
            var form = _serviceProvider.GetRequiredService<MinionManagementForm>();
            OpenForm(form);
        }

        private void btnSchemes_Click(object sender, EventArgs e)
        {
            var form = _serviceProvider.GetRequiredService<SchemeManagementForm>();
            OpenForm(form);
        }

        private void btnBases_Click(object sender, EventArgs e)
        {
            var form = _serviceProvider.GetRequiredService<BaseManagementForm>();
            OpenForm(form);
        }

        private void btnEquipment_Click(object sender, EventArgs e)
        {
            var form = _serviceProvider.GetRequiredService<EquipmentInventoryForm>();
            OpenForm(form);
        }

        private void OpenForm(Form form)
        {
            form.ShowDialog();
            LoadStatistics(); // Refresh after closing child form
        }

        // Business logic in UI layer (anti-pattern)
        // This calculation is duplicated from models
        private void LoadStatistics()
        {
            var minions = _minionService.GetAllMinions().ToList();
            var schemes = _schemeService.GetAllSchemes().ToList();
            var bases = _baseService.GetAllBases().ToList();
            var equipment = _equipmentService.GetAllEquipment().ToList();

            var moodCounts = _minionService.GetMinionMoodCounts();
            lblMinionStats.Text = $"Minions: {minions.Count} total | Happy: {moodCounts["Happy"]} | Grumpy: {moodCounts["Grumpy"]} | Plotting Betrayal: {moodCounts["Betrayal"]}";

            var activeSchemes = schemes.Where(s => s.Status == "Active").ToList();
            double avgSuccess = 0;
            if (activeSchemes.Any())
            {
                foreach (var scheme in activeSchemes)
                {
                    int success = _schemeService.CalculateSuccessLikelihood(scheme);
                    avgSuccess += success;
                }
                avgSuccess /= activeSchemes.Count;
            }

            lblSchemeStats.Text = $"Evil Schemes: {schemes.Count} total | Active: {activeSchemes.Count} | Avg Success Likelihood: {avgSuccess:F1}%";

            decimal totalMinionSalaries = _minionService.CalculateTotalSalaryCosts();
            decimal totalBaseCosts = _baseService.CalculateTotalMaintenanceCosts();
            decimal totalEquipmentCosts = _equipmentService.CalculateTotalMaintenanceCosts();
            decimal totalMonthlyCost = totalMinionSalaries + totalBaseCosts + totalEquipmentCosts;

            lblCostStats.Text = $"Monthly Costs: Minions: ${totalMinionSalaries:N0} | Bases: ${totalBaseCosts:N0} | Equipment: ${totalEquipmentCosts:N0} | TOTAL: ${totalMonthlyCost:N0}";

            var alerts = "";

            var lowLoyaltyMinions = _minionService.GetLowLoyaltyMinions().Count();
            if (lowLoyaltyMinions > 0)
            {
                alerts += $"⚠ Warning: {lowLoyaltyMinions} minions have low loyalty and may betray you! ";
            }

            var brokenEquipment = _equipmentService.GetBrokenEquipment().Count();
            if (brokenEquipment > 0)
            {
                alerts += $"⚠ {brokenEquipment} equipment items are broken! ";
            }

            var overBudgetSchemes = _schemeService.GetOverBudgetSchemes().Count();
            if (overBudgetSchemes > 0)
            {
                alerts += $"⚠ {overBudgetSchemes} schemes are over budget! ";
            }

            lblAlerts.Text = string.IsNullOrEmpty(alerts) ? "✓ All systems operational" : alerts;
        }
    }
}
