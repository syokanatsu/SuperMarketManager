using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperMarketManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog(this);
            if(loginForm.DialogResult == DialogResult.OK)
            {
                loginForm.Dispose();
                Form saleform = new SaleForm();
                saleform.TopLevel = false;
                tabControl1.TabPages[0].Controls.Add(saleform);
                saleform.Show();
                Form purchaseform = new PurchaseForm();
                purchaseform.TopLevel = false;
                tabControl1.TabPages[1].Controls.Add(purchaseform);
                purchaseform.Show();
                Form inventoryform = new InventoryForm();
                inventoryform.TopLevel = false;
                tabControl1.TabPages[2].Controls.Add(inventoryform);
                inventoryform.Show();
                Form memberform = new MemberForm();
                memberform.TopLevel = false;
                tabControl1.TabPages[3].Controls.Add(memberform);
                memberform.Show();
                Form salerecordform = new SaleRecordForm();
                salerecordform.TopLevel = false;
                tabControl1.TabPages[4].Controls.Add(salerecordform);
                salerecordform.Show();
                Form purchaserecordform = new PurchaseRecordForm();
                purchaserecordform.TopLevel = false;
                tabControl1.TabPages[5].Controls.Add(purchaserecordform);
                purchaserecordform.Show();
                Form settingform = new SettingForm();
                settingform.TopLevel = false;
                tabControl1.TabPages[6].Controls.Add(settingform);
                settingform.Show();
            }
            else
            {
                loginForm.Dispose();
                this.Close();
            }
        }

        private void tabPage5_Enter(object sender, EventArgs e)
        {
            ((SaleRecordForm)((TabPage)sender).Controls[0]).InitLvSaleRecord();
        }

        private void tabPage6_Enter(object sender, EventArgs e)
        {
            ((PurchaseRecordForm)((TabPage)sender).Controls[0]).InitLvPurRecord();
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            ((InventoryForm)((TabPage)sender).Controls[0]).InitLvGoods();
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            ((MemberForm)((TabPage)sender).Controls[0]).InitLvMembers();
        }
    }
}
