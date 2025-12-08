using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VillainLairManager.Models;
using VillainLairManager.Utils;
using VillainLairManager.Services;
using VillainLairManager.Repositories;

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
        
        public MainForm()
        {
            InitializeComponent();
            
            // Initialize services
            var factory = new RepositoryFactory(AppSettings.Instance.DatabasePath);
            _schemeService = new SchemeService(factory.Schemes, factory.Minions, factory.Equipment);
            _minionService = new MinionService(factory.Minions);
            _baseService = new BaseService(factory.Bases, factory.Minions, factory.Equipment);
            _equipmentService = new EquipmentService(factory.Equipment, factory.Schemes);
            
            LoadStatistics(); // Business logic in form load (anti-pattern)
        }

        private void btnMinions_Click(object sender, EventArgs e)
        {
            OpenForm(new MinionManagementForm());
        }

        private void btnSchemes_Click(object sender, EventArgs e)
        {
            OpenForm(new SchemeManagementForm());
        }

        private void btnBases_Click(object sender, EventArgs e)
        {
            OpenForm(new BaseManagementForm());
        }

        private void btnEquipment_Click(object sender, EventArgs e)
        {
            OpenForm(new EquipmentInventoryForm());
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
            // Use services for data retrieval instead of direct database access
            var minions = _minionService.GetAllMinions().ToList();
            var schemes = _schemeService.GetAllSchemes().ToList();
            var bases = _baseService.GetAllBases().ToList();
            var equipment = _equipmentService.GetAllEquipment().ToList();

            // Minion statistics with duplicated mood calculation
            int happyCount = 0, grumpyCount = 0, betrayalCount = 0;
            foreach (var minion in minions)
            {
                // Mood calculation duplicated from Minion.UpdateMood() (anti-pattern)
                if (minion.LoyaltyScore > 70)
                    happyCount++;
                else if (minion.LoyaltyScore < 40)
                    betrayalCount++;
                else
                    grumpyCount++;
            }

            lblMinionStats.Text = $"Minions: {minions.Count} total | Happy: {happyCount} | Grumpy: {grumpyCount} | Plotting Betrayal: {betrayalCount}";

            // Scheme statistics with duplicated success calculation
            var activeSchemes = schemes.Where(s => s.Status == "Active").ToList();
            double avgSuccess = 0;
            if (activeSchemes.Any())
            {
                // Use service for success calculation
                foreach (var scheme in activeSchemes)
                {
                    int success = _schemeService.CalculateSuccessLikelihood(scheme);
                    avgSuccess += success;
                }
                avgSuccess /= activeSchemes.Count;
            }

            lblSchemeStats.Text = $"Evil Schemes: {schemes.Count} total | Active: {activeSchemes.Count} | Avg Success Likelihood: {avgSuccess:F1}%";

            // Cost calculation (business logic in UI)
            decimal totalMinionSalaries = 0;
            foreach (var minion in minions)
            {
                totalMinionSalaries += minion.SalaryDemand;
            }

            decimal totalBaseCosts = 0;
            foreach (var baseObj in bases)
            {
                totalBaseCosts += baseObj.MonthlyMaintenanceCost;
            }

            decimal totalEquipmentCosts = 0;
            foreach (var equip in equipment)
            {
                totalEquipmentCosts += equip.MaintenanceCost;
            }

            decimal totalMonthlyCost = totalMinionSalaries + totalBaseCosts + totalEquipmentCosts;

            lblCostStats.Text = $"Monthly Costs: Minions: ${totalMinionSalaries:N0} | Bases: ${totalBaseCosts:N0} | Equipment: ${totalEquipmentCosts:N0} | TOTAL: ${totalMonthlyCost:N0}";

            // Alerts (more business logic in UI)
            var alerts = "";

            // Low loyalty alert
            var lowLoyaltyMinions = minions.Where(m => m.LoyaltyScore < 40).Count();
            if (lowLoyaltyMinions > 0)
            {
                alerts += $"⚠ Warning: {lowLoyaltyMinions} minions have low loyalty and may betray you! ";
            }

            // Broken equipment alert
            var brokenEquipment = equipment.Where(e => e.Condition < 20).Count();
            if (brokenEquipment > 0)
            {
                alerts += $"⚠ {brokenEquipment} equipment items are broken! ";
            }

            // Over budget schemes
            var overBudgetSchemes = schemes.Where(s => s.CurrentSpending > s.Budget).Count();
            if (overBudgetSchemes > 0)
            {
                alerts += $"⚠ {overBudgetSchemes} schemes are over budget! ";
            }

            lblAlerts.Text = string.IsNullOrEmpty(alerts) ? "✓ All systems operational" : alerts;
        }
    }
}
