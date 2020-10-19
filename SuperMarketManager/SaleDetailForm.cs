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
    public partial class SaleDetailForm : Form
    {
        private string billnum;
        public string Billnum { get => billnum; set => billnum = value; }

        private class NoneBillNumFoundException : Exception
        {
            public NoneBillNumFoundException(string message) : base(message)
            {

            }
        }

        private SaleDetailForm()
        {
            InitializeComponent();
        }

        public SaleDetailForm(string billnum) : this()
        {
            Billnum = billnum;
            if (InitTitle())
            {
                InitLvDetails();
            }
            else
            {
                Close();
            }
        }

        private bool InitTitle()
        {
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string mysqlstr = string.Format(
@"SELECT DISTINCT
  `sale_billnum`, `admin_username`, `mem_username`, `sale_datetime`
FROM
  `tb_sale` a,
  `tb_admin` b,
  `tb_member` c
WHERE
  `a`.`admin_id` = `b`.`admin_id` AND `a`.`mem_id` = `c`.`mem_id` AND `sale_billnum` = '{0}';", Billnum);

                MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    txtBillnum.Text = reader.GetString(0);
                    txtAdmin.Text = reader.GetString(1);
                    txtMember.Text = reader.GetString(2);
                    var date = reader.GetMySqlDateTime(3);
                    txtDate.Text = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                    return true;
                }
                else
                {
                    throw new NoneBillNumFoundException("未找到进货单号");
                }
            }
            catch (NoneBillNumFoundException e)
            {
                MessageBox.Show(this, e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e));
            }
            finally
            {
                conn.Close();
            }
            return false;
        }

        private int InitLvDetails()
        {
            listView1.Items.Clear();
            int n = 0;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string mysqlstr = string.Format(
@"SELECT
  `goods_name`,
  `goods_num`,
  `goods_price`,
  `sale_amount`,
  (`sale_amount` * `goods_price`) AS 'money'
FROM
  `tb_sale` a,
  `tb_admin` b,
  `tb_goodsinfo` c
WHERE
  `a`.`admin_id` = `b`.`admin_id` AND `a`.`goods_id` = `c`.`goods_id` AND `sale_billnum` = '{0}'", Billnum);

                MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = (++n).ToString();
                    item.SubItems.Add(reader.GetString(0));
                    item.SubItems.Add(reader.GetString(1));
                    item.SubItems.Add(reader.GetString(2));
                    item.SubItems.Add(reader.GetString(3));
                    item.SubItems.Add(reader.GetString(4));
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
            return 0;
        }
    }
}
