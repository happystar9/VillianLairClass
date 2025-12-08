using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Service for dashboard statistics and alerts
    /// Extracted from MainForm.LoadStatistics()
    /// </summary>
    public class DashboardService
    {
        private readonly MinionService _minionService;
        private readonly SchemeService _schemeService;
        private readonly BaseService _baseService;
        private readonly EquipmentService _equipmentService;

        public DashboardService(
            MinionService minionService,
            SchemeService schemeService,
            BaseService baseService,
            EquipmentService equipmentService)
        {
            _minionService = minionService ?? throw new ArgumentNullException(nameof(minionService));
            _schemeService = schemeService ?? throw new ArgumentNullException(nameof(schemeService));
            _baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        }

        /// <summary>
        /// Gets statistics for minions
        /// Extracted from MainForm.LoadStatistics()
        /// </summary>
        public MinionStatistics GetMinionStatistics()
        {
            var moodCounts = _minionService.GetMinionMoodCounts();
            var totalCount = _minionService.GetAllMinions().Count();

            return new MinionStatistics
            {
                TotalCount = totalCount,
                HappyCount = moodCounts["Happy"],
                GrumpyCount = moodCounts["Grumpy"],
                BetrayalCount = moodCounts["Betrayal"]
            };
        }

        /// <summary>
        /// Gets statistics for schemes
        /// Extracted from MainForm.LoadStatistics()
        /// </summary>
        public SchemeStatistics GetSchemeStatistics()
        {
            var allSchemes = _schemeService.GetAllSchemes().ToList();
            var activeSchemes = _schemeService.GetActiveSchemes().ToList();
            var avgSuccess = _schemeService.CalculateAverageSuccessLikelihood();

            return new SchemeStatistics
            {
                TotalCount = allSchemes.Count,
                ActiveCount = activeSchemes.Count,
                AverageSuccessLikelihood = avgSuccess
            };
        }

        /// <summary>
        /// Gets cost statistics
        /// Extracted from MainForm.LoadStatistics()
        /// </summary>
        public CostStatistics GetCostStatistics()
        {
            var minionCosts = _minionService.CalculateTotalSalaryCosts();
            var baseCosts = _baseService.CalculateTotalMaintenanceCosts();
            var equipmentCosts = _equipmentService.CalculateTotalMaintenanceCosts();
            var totalCosts = minionCosts + baseCosts + equipmentCosts;

            return new CostStatistics
            {
                TotalMinionSalaries = minionCosts,
                TotalBaseMaintenance = baseCosts,
                TotalEquipmentMaintenance = equipmentCosts,
                TotalMonthlyCost = totalCosts
            };
        }

        /// <summary>
        /// Gets alerts for various issues
        /// Extracted from MainForm.LoadStatistics()
        /// </summary>
        public List<Alert> GetAlerts()
        {
            var alerts = new List<Alert>();

            // Low loyalty alert
            var lowLoyaltyCount = _minionService.GetLowLoyaltyMinions().Count();
            if (lowLoyaltyCount > 0)
            {
                alerts.Add(new Alert
                {
                    Severity = AlertSeverity.Warning,
                    Message = $"{lowLoyaltyCount} minions have low loyalty and may betray you!"
                });
            }

            // Broken equipment alert
            var brokenEquipmentCount = _equipmentService.GetBrokenEquipment().Count();
            if (brokenEquipmentCount > 0)
            {
                alerts.Add(new Alert
                {
                    Severity = AlertSeverity.Warning,
                    Message = $"{brokenEquipmentCount} equipment items are broken!"
                });
            }

            // Over budget schemes
            var overBudgetCount = _schemeService.GetOverBudgetSchemes().Count();
            if (overBudgetCount > 0)
            {
                alerts.Add(new Alert
                {
                    Severity = AlertSeverity.Warning,
                    Message = $"{overBudgetCount} schemes are over budget!"
                });
            }

            return alerts;
        }
    }

    /// <summary>
    /// Statistics for minions
    /// </summary>
    public class MinionStatistics
    {
        public int TotalCount { get; set; }
        public int HappyCount { get; set; }
        public int GrumpyCount { get; set; }
        public int BetrayalCount { get; set; }
    }

    /// <summary>
    /// Statistics for schemes
    /// </summary>
    public class SchemeStatistics
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public double AverageSuccessLikelihood { get; set; }
    }

    /// <summary>
    /// Cost statistics
    /// </summary>
    public class CostStatistics
    {
        public decimal TotalMinionSalaries { get; set; }
        public decimal TotalBaseMaintenance { get; set; }
        public decimal TotalEquipmentMaintenance { get; set; }
        public decimal TotalMonthlyCost { get; set; }
    }

    /// <summary>
    /// Alert for dashboard
    /// </summary>
    public class Alert
    {
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Alert severity levels
    /// </summary>
    public enum AlertSeverity
    {
        Info,
        Warning,
        Error
    }
}
