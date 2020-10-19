using MySql.Data.MySqlClient;
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
    public partial class PurchaseRecordForm : Form
    {
        public PurchaseRecordForm()
        {
            InitializeComponent();
        }

        private void PurchaseRecordForm_Load(object sender, EventArgs e)
        {
            
        }

        public int InitLvPurRecord()
        {
            listView1.Items.Clear();
            int n = 0;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string mysqlstr =
@"SELECT
  *, COUNT(`money`) AS 'pinleishu', SUM(`money`) AS 'total'
FROM
  (SELECT
    `pur_billnum`, `pur_date`, `admin_username`, (`pur_amount` * `goods_inprice`) AS 'money'
  FROM
    `tb_purchase` a,
    `tb_goodsinfo` b,
    `tb_admin` c
  WHERE
    `a`.`admin_id` = `c`.`admin_id` AND `a`.`goods_id` = `b`.`goods_id`) AS subtable
GROUP BY
  `pur_billnum`;";

                MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = (++n).ToString();
                    item.SubItems.Add(reader.GetString(0));
                    var date = reader.GetMySqlDateTime(1);
                    item.SubItems.Add(string.Format("{0:D4}-{1:D2}-{2:D2}", date.Year, date.Month, date.Day));
                    item.SubItems.Add(reader.GetString(2));
                    item.SubItems.Add(reader.GetString(4));
                    item.SubItems.Add(reader.GetString(5));
                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e));
            }
            finally
            {
                conn.Close();
            }
            return n;
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            string billnum = listView1.SelectedItems[0].SubItems[1].Text;
            PurchaseDetailForm window = new PurchaseDetailForm(billnum);
            if (window.IsDisposed) return;
            window.ShowDialog();
            window.Dispose();
        }
    }
}
