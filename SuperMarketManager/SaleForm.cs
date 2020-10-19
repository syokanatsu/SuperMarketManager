using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperMarketManager
{
    public partial class SaleForm : Form
    {
        public SaleForm()
        {
            InitializeComponent();
        }

        private delegate void OnInventoryNotEnough();
        private OnInventoryNotEnough inventoryNotEnoughEvent;

        private Dictionary<string, int> inventory = new Dictionary<string, int>();

        private void SaleForm_Load(object sender, EventArgs e)
        {
            inventoryNotEnoughEvent += ThrowNotEnough;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private class InventoryNotEnoughException : Exception
        {
            public InventoryNotEnoughException(string msg) : base(msg)
            {

            }
        }

        private class SaleBillnumRepeat : Exception
        {
            public SaleBillnumRepeat(string msg) : base(msg)
            {

            }
        }

        private class NoneGoodsNum : Exception
        {
            public NoneGoodsNum(string msg) : base(msg)
            {

            }
        }

        private class NoneMemberNum : Exception
        {
            public NoneMemberNum(string msg) : base(msg)
            {

            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            bool update = false;
            string goodsnum = txtGoodsid.Text;
            int amount = Convert.ToInt32(numAmount.Value);
            if (goodsnum.Length == 0)
            {
                MessageBox.Show(this, "商品条码不能为空");
                return;
            }
            if (amount == 0)
            {
                MessageBox.Show(this, "数量不能为0");
                return;
            }

            int i = listvGoods.Items.Count;  // list中的项数
            int m = 0;
            for (m = 0; m < i; m++)
            {
                if (listvGoods.Items[m].SubItems[2].Text.Equals(goodsnum))
                    break;
            }
            if (m == i)
            {
                MySqlConnection conn = MysqlConnector.GetInstance();
                try
                {
                    conn.Open();
                    string sqlstr = string.Format("select goods_name,goods_num,goods_price,goods_left from tb_goodsinfo where goods_num='{0}';", goodsnum);
                    MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                    MySqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {
                        inventory.Add(goodsnum, reader.GetInt32(3));
                        if (amount > inventory[goodsnum])
                        {
                            reader.Close();
                            inventoryNotEnoughEvent();
                        }
                        MakeBillNum();
                        ListViewItem item = new ListViewItem();
                        item.Text = (i + 1).ToString();
                        item.SubItems.Add(reader.GetString(0));
                        item.SubItems.Add(reader.GetString(1));
                        item.SubItems.Add(reader.GetDouble(2).ToString());
                        item.SubItems.Add(amount.ToString());
                        item.SubItems.Add((reader.GetDouble(2) * amount).ToString());
                        listvGoods.Items.Add(item);
                        update = true;
                    }
                    else
                    {
                        MessageBox.Show(this, "未查询到商品信息，请先添加商品信息");
                    }
                }
                catch(InventoryNotEnoughException e1)
                {
                    MessageBox.Show(this, e1.Message);
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
                int oldn = Convert.ToInt32(listvGoods.Items[m].SubItems[4].Text);
                if((oldn + amount) > inventory[goodsnum])
                {
                    MessageBox.Show(this, "库存不足");
                    return;
                }
                double price = Convert.ToDouble(listvGoods.Items[m].SubItems[3].Text);
                double money = (oldn + amount) * price;
                listvGoods.Items[m].SubItems[4].Text = (oldn + amount).ToString();
                listvGoods.Items[m].SubItems[5].Text = money.ToString();
                update = true;
            }
            if (update)
            {
                SumMoneyAndUpdate();
            }
        }

        private void ThrowNotEnough()
        {
            throw new InventoryNotEnoughException("库存不足");
        }

        private void SumMoneyAndUpdate()
        {
            double sum = 0;
            foreach (ListViewItem item in listvGoods.Items)
            {
                double money = Convert.ToDouble(item.SubItems[5].Text);
                sum += money;
            }
            txtSum.Text = sum.ToString();
        }

        private void MakeBillNum()
        {
            if (listvGoods.Items.Count == 0)
            {
                DateTime dt = DateTime.Now;
                string billnum = string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                txtBillnum.Text = billnum;
            }
        }

        private void numMoney_ValueChanged(object sender, EventArgs e)
        {
            SumChange();
        }

        private void SumChange()
        {
            if (numMoney.Value > Convert.ToInt32(txtSum.Text))
            {
                txtChange.Text = (numMoney.Value - Convert.ToInt32(txtSum.Text)).ToString();
            }
            else
            {
                txtChange.Text = "0";
            }
        }

        private void listvGoods_ControlAdded(object sender, ControlEventArgs e)
        {
        }

        private void listvGoods_ControlRemoved(object sender, ControlEventArgs e)
        {
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listvGoods.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            int n = listvGoods.SelectedItems[0].Index;
            RemoveItemAndUpdate(n);
            SumMoneyAndUpdate();
            if (listvGoods.Items.Count == 0)
            {
                txtBillnum.Text = "";
            }
        }

        private void RemoveItemAndUpdate(int index)
        {
            listvGoods.Items.Remove(listvGoods.Items[index]);
            int n = listvGoods.Items.Count;
            for (int i = index; i < n; i++)
            {
                listvGoods.Items[i].Text = (i + 1).ToString();
            }
        }

        private void btnSum_Click(object sender, EventArgs e)
        {
            string billnum = txtBillnum.Text;
            string mem_num = txtMemberId.Text;
            string money = numMoney.Value.ToString();
            string sum = txtSum.Text;
            if (Convert.ToDouble(money) < Convert.ToDouble(sum))
            {
                MessageBox.Show(this, "实付款不得小于总金额");
                return;
            }
            if (mem_num.Length == 0)
            {
                MessageBox.Show(this, "会员卡号不能为空");
                return;
            }

            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string sqlstr = string.Format("select sale_id from tb_sale where sale_billnum='{0}';", billnum);
                MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                object re = comm.ExecuteScalar();
                if (re != null)
                {
                    throw new SaleBillnumRepeat("流水号重复");
                }
                int memid = GetMemberId(mem_num, conn);
                if (memid == 0)
                {
                    throw new NoneGoodsNum("会员卡号不存在");
                }
                string mysqlstr = "";
                foreach (ListViewItem item in listvGoods.Items)
                {
                    int goodsid = GetGoodsId(item.SubItems[2].Text, conn);
                    if (goodsid == 0)
                    {
                        throw new NoneGoodsNum(string.Format("商品编号：{0}，不存在", item.SubItems[2].Text));
                    }
                    mysqlstr += string.Format("insert into tb_sale(mem_id,goods_id,sale_amount, sale_billnum, sale_money,admin_id,sale_datetime) value({0},{1},{2},'{3}',{4},'{5}','{6}-{7}-{8} {9}:{10}:{11}');",
                        memid, goodsid, item.SubItems[4].Text, billnum, money, GlobalVar.login.Id,
                        billnum.Substring(0, 4),
                        billnum.Substring(4, 2),
                        billnum.Substring(6, 2),
                        billnum.Substring(8, 2),
                        billnum.Substring(10, 2),
                        billnum.Substring(12, 2));
                    mysqlstr += string.Format("update tb_goodsinfo set goods_left=goods_left-{0} where goods_id='{1}';", item.SubItems[4].Text, goodsid);
                }
                mysqlstr += string.Format("update tb_member set mem_points=mem_points+{0} where mem_id='{1}';", sum, memid);
                MySqlCommand comm1 = new MySqlCommand(mysqlstr, conn);
                int res = comm1.ExecuteNonQuery();
                if (res != 0)
                {
                    MessageBox.Show(this, "结算成功");
                    inventory.Clear();
                    listvGoods.Items.Clear();
                    txtBillnum.Text = "";
                    txtGoodsid.Text = "";
                    txtMemberId.Text = "";
                    numAmount.Value = 1;
                    numMoney.Value = 0;
                    txtSum.Text = "0";
                }
                else
                {
                    MessageBox.Show(this, "结算失败");
                }
            }
            catch (SaleBillnumRepeat e1)
            {
                MessageBox.Show(this, e1.Message);
            }
            catch(NoneMemberNum e1)
            {
                MessageBox.Show(this, e1.Message);
            }
            catch (NoneGoodsNum e1)
            {
                MessageBox.Show(this, e1.Message);
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
        private int GetMemberId(string num, MySqlConnection conn)
        {
            string str = string.Format("select mem_id from tb_member where mem_username='{0}';", num);
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
    }
}
