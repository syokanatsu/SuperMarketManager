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
    public partial class PurchaseForm : Form
    {
        public PurchaseForm()
        {
            InitializeComponent();
        }

        public class PurBillnumRepeat : Exception
        {
            public PurBillnumRepeat(string msg) : base(msg)
            {

            }
        }

        public class NoneGoodsNum : Exception
        {
            public NoneGoodsNum(string msg) : base(msg)
            {

            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            bool update = false;
            string id = txtGoodsid.Text;
            int n = Convert.ToInt32(numAmount.Value);
            if (id.Length == 0)
            {
                MessageBox.Show(this, "商品id不能为空");
                return;
            }
            if (n == 0)
            {
                MessageBox.Show(this, "数量不得为0");
                return;
            }
            int i = listView1.Items.Count;  // list中的项数
            int m = 0;
            for(m = 0; m < i; m++)
            {
                if (listView1.Items[m].SubItems[2].Text.Equals(id))
                    break;
            }
            if (m == i)
            {
                MySqlConnection conn = MysqlConnector.GetInstance();
                try
                {
                    conn.Open();
                    string sqlstr = string.Format("select goods_name,goods_num,goods_inprice from tb_goodsinfo where goods_num='{0}';", id);
                    MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                    MySqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = (i + 1).ToString();
                        item.SubItems.Add(reader.GetString(0));
                        item.SubItems.Add(reader.GetString(1));
                        item.SubItems.Add(reader.GetDouble(2).ToString());
                        item.SubItems.Add(n.ToString());
                        item.SubItems.Add((reader.GetDouble(2) * n).ToString());
                        listView1.Items.Add(item);
                        update = true;
                    }
                    else
                    {
                        MessageBox.Show(this, "未查询到商品信息，请先添加商品信息");
                    }
                }
                catch (Exception e1)
                {
                    MessageBox.Show(this, string.Format("Exception: {0}", e1));
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                int oldn = Convert.ToInt32(listView1.Items[m].SubItems[4].Text);
                double inprice = Convert.ToDouble(listView1.Items[m].SubItems[3].Text);
                double money = (oldn + n) * inprice;
                listView1.Items[m].SubItems[4].Text = (oldn + n).ToString();
                listView1.Items[m].SubItems[5].Text = money.ToString();
                update = true;
            }
            if (update)
            {
                SumMoneyAndUpdate();
            }
        }

        private void SumMoneyAndUpdate()
        {
            double sum = 0;
            foreach(ListViewItem item in listView1.Items)
            {
                double money = Convert.ToDouble(item.SubItems[5].Text);
                sum += money;
            }
            txtSum.Text = sum.ToString();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            int n = listView1.SelectedItems[0].Index;
            RemoveItemAndUpdate(n);
            SumMoneyAndUpdate();
        }

        private void RemoveItemAndUpdate(int index)
        {
            listView1.Items.Remove(listView1.Items[index]);
            int n = listView1.Items.Count;
            for (int i = index; i < n; i++)
            {
                listView1.Items[i].Text = (i + 1).ToString();
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string billnum = txtBillnum.Text;
            string date = dtTime.Text;
            if (billnum.Length == 0)
            {
                MessageBox.Show(this, "流水号不能为空");
                return;
            }
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string sqlstr = string.Format("select pur_id from tb_purchase where pur_billnum='{0}';", billnum);
                MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                object re = comm.ExecuteScalar();
                if (re != null)
                {
                    throw new PurBillnumRepeat("流水号重复");
                }
                string mysqlstr = "";
                foreach (ListViewItem item in listView1.Items)
                {
                    int goodsid = GetGoodsId(item.SubItems[2].Text, conn);
                    if (goodsid == 0)
                    {
                        throw new NoneGoodsNum(string.Format("商品编号：{0}，不存在", item.SubItems[2].Text));
                    }
                    int goodsleft = GetGoodsInventory(item.SubItems[2].Text, conn);
                    mysqlstr += string.Format("insert into tb_purchase(pur_billnum,goods_id,pur_amount,admin_id,pur_date) value('{0}',{1},{2},{3},'{4}');",
                        billnum, goodsid, item.SubItems[4].Text, GlobalVar.login.Id, date);
                    mysqlstr += string.Format("update tb_goodsinfo set goods_left={0} where goods_id='{1}';", goodsleft + Convert.ToInt32(item.SubItems[4].Text), goodsid);
                }

                MySqlCommand comm1 = new MySqlCommand(mysqlstr, conn);
                int res = comm1.ExecuteNonQuery();
                if (res != 0)
                {
                    MessageBox.Show(this, "提交成功");
                    listView1.Items.Clear();
                    txtBillnum.Text = "";
                    txtGoodsid.Text = "";
                    numAmount.Value = 0;
                    txtSum.Text = "0";
                }
                else
                {
                    MessageBox.Show(this, "提交失败");
                }
            }
            catch(PurBillnumRepeat e1)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e1.Message));
            }
            catch(NoneGoodsNum e1)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e1.Message));
            }
            catch(Exception e1)
            {
                MessageBox.Show(this, string.Format("Exception: {0}", e1));
            }
            finally
            {
                conn.Close();
            }
        }

        private int GetGoodsId(string num, MySqlConnection conn)
        {
            string str = string.Format("select goods_id from tb_goodsinfo where goods_num='{0}';", num);
            MySqlCommand comm = new MySqlCommand(str, conn);
            object re = comm.ExecuteScalar();
            if (re == null)
            {
                return 0;
            }
            else
            {
                return (int)re;
            }
        }

        private int GetGoodsInventory(string num, MySqlConnection conn)
        {
            string str = string.Format("select goods_left from tb_goodsinfo where goods_num='{0}';", num);
            MySqlCommand comm = new MySqlCommand(str, conn);
            object re = comm.ExecuteScalar();
            if (re == null)
            {
                return -1;
            }
            else
            {
                return Convert.ToInt32(re);
            }
        }
    }
}
