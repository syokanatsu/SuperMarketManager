using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SuperMarketManager
{
    public partial class SaleRecordForm : Form
    {
        public SaleRecordForm()
        {
            InitializeComponent();
        }

        public int InitLvSaleRecord()
        {
            listView1.Items.Clear();
            int n = 0;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string mysqlstr =
@"SELECT
  *, COUNT(`money`) AS `pinleishu`, SUM(`money`) AS `total`
FROM
  (SELECT
    `sale_billnum`,
    `sale_datetime`,
    `mem_username`,
    `admin_username`,
    (`sale_amount` * `goods_price`) AS 'money'
  FROM
    `tb_sale` a,
    `tb_goodsinfo` b,
    `tb_admin` c,
    `tb_member` d
  WHERE
    `a`.`admin_id` = `c`.`admin_id` AND `a`.`goods_id` = `b`.`goods_id` AND `a`.`mem_id` = `d`.`mem_id`) AS subtb
GROUP BY
  `sale_billnum`;";

                MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = (++n).ToString();
                    item.SubItems.Add(reader.GetString(0));
                    var date = reader.GetMySqlDateTime(1);
                    item.SubItems.Add(string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second));
                    item.SubItems.Add(reader.GetString(2));
                    item.SubItems.Add(reader.GetString(3));
                    item.SubItems.Add(reader.GetString(5));
                    item.SubItems.Add(reader.GetString(6));
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

        private void SaleRecordForm_Load(object sender, EventArgs e)
        {
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            string billnum = listView1.SelectedItems[0].SubItems[1].Text;
            SaleDetailForm window = new SaleDetailForm(billnum);
            if (window.IsDisposed) return;
            window.ShowDialog();
            window.Dispose();
        }
    }
}
