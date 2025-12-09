using System;
using System.Windows.Forms;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class EquipmentInventoryForm : Form
    {
        private readonly EquipmentService _equipmentService;
        private readonly SchemeService _schemeService;

        public EquipmentInventoryForm(
            EquipmentService equipmentService,
            SchemeService schemeService)
        {
            _equipmentService = equipmentService;
            _schemeService = schemeService;
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Equipment Inventory";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Equipment Inventory Form",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
