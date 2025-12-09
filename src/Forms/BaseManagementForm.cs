using System;
using System.Windows.Forms;
using VillainLairManager.Services;

namespace VillainLairManager.Forms
{
    public partial class BaseManagementForm : Form
    {
        private readonly BaseService _baseService;
        private readonly MinionService _minionService;
        private readonly EquipmentService _equipmentService;

        public BaseManagementForm(
            BaseService baseService,
            MinionService minionService,
            EquipmentService equipmentService)
        {
            _baseService = baseService;
            _minionService = minionService;
            _equipmentService = equipmentService;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Secret Base Management";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblStub = new Label
            {
                Text = "TODO: Implement Base Management Form",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(800, 400),
                Font = new System.Drawing.Font("Arial", 10)
            };
            this.Controls.Add(lblStub);
        }
    }
}
