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
    public partial class InventoryForm : Form
    {
        public InventoryForm()
        {
            InitializeComponent();
        }

        private class GoodsIDRepeatException : Exception
        {
            public GoodsIDRepeatException(string msg) : base(msg)
            {

            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string id = txtId.Text;
            double inprice = Convert.ToDouble(numInprice.Value);
            double price = Convert.ToDouble(numPrice.Value);
            int inventory = Convert.ToInt32(numInventory.Value);
            string kind = txtKind.Text;
            if (name.Length != 0 && id.Length != 0 && price != 0 && kind.Length != 0)
            {
                MySqlConnection conn = MysqlConnector.GetInstance();
                try
                {
                    conn.Open();
                    string mysqlstr = string.Format("select goods_id from tb_goodsinfo where goods_num='{0}';", id);
                    MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                    object reo = comm.ExecuteScalar();
                    if (reo != null)
                    {
                        MessageBox.Show(this, "商品条码已存在");
                    }
                    else
                    {
                        string sqlstr = string.Format("insert into tb_goodsinfo(goods_name,goods_num,goods_inprice,goods_price,goods_left,goods_kind) value('{0}','{1}',{2},{3},{4},'{5}');", name, id, inprice, price, inventory, kind);
                        MySqlCommand comm1 = new MySqlCommand(sqlstr, conn);
                        int re = comm1.ExecuteNonQuery();
                        if(re == 1)
                        {
                            ListViewItem item = new ListViewItem();
                            item.Text = ((listView1.Items.Count + 1).ToString());
                            item.SubItems.Add(name);
                            item.SubItems.Add(id);
                            item.SubItems.Add(inprice.ToString());
                            item.SubItems.Add(price.ToString());
                            item.SubItems.Add(inventory.ToString());
                            item.SubItems.Add(kind);
                            listView1.Items.Add(item);
                            MessageBox.Show(this, "添加成功");
                        }
                        else
                        {
                            MessageBox.Show(this, "添加失败");
                        }
                    }
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
            else
            {
                MessageBox.Show(this, "数据不允许为空或0");
            }
        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            //InitLvGoods();
        }

        public int InitLvGoods()
        {
            listView1.Items.Clear();
            int n = 0;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string sqlstr = "select * from tb_goodsinfo;";
                MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = ((++n).ToString());
                    item.SubItems.Add(reader.GetString(1));
                    item.SubItems.Add(reader.GetString(2));
                    item.SubItems.Add(reader.GetDouble(3).ToString());
                    item.SubItems.Add(reader.GetDouble(4).ToString());
                    item.SubItems.Add(reader.GetInt32(5).ToString());
                    item.SubItems.Add(reader.GetString(6));
                    listView1.Items.Add(item);
                }
                reader.Close();
            }
            catch(Exception e)
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
            txtName.Text = listView1.SelectedItems[0].SubItems[1].Text;
            txtId.Text = listView1.SelectedItems[0].SubItems[2].Text;
            numInprice.Value = Convert.ToDecimal(listView1.SelectedItems[0].SubItems[3].Text);
            numPrice.Value = Convert.ToDecimal(listView1.SelectedItems[0].SubItems[4].Text);
            numInventory.Value = Convert.ToDecimal(listView1.SelectedItems[0].SubItems[5].Text);
            txtKind.Text = listView1.SelectedItems[0].SubItems[6].Text;
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            string name = txtName.Text;
            string id = txtId.Text;
            double inprice = Convert.ToDouble(numInprice.Value);
            double price = Convert.ToDouble(numPrice.Value);
            int inventory = Convert.ToInt32(numInventory.Value);
            string kind = txtKind.Text;
            string oldid = listView1.SelectedItems[0].SubItems[2].Text;

            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                if (!id.Equals(oldid))
                {
                    string sqlstr = string.Format("select * from tb_goodsinfo where goods_num='{0}';", id);
                    MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                    object re = comm.ExecuteScalar();
                    if (re != null)
                    {
                        throw new GoodsIDRepeatException("商品信息重复");
                    }
                }
                string mysqlstr = string.Format("update tb_goodsinfo set goods_name='{0}',goods_num='{1}',goods_inprice={2},goods_price={3},goods_left={4},goods_kind='{5}' where goods_num='{6}';",
                    name, id, inprice, price, inventory, kind, oldid);
                MySqlCommand comm1 = new MySqlCommand(mysqlstr, conn);
                int res = comm1.ExecuteNonQuery();
                if (res == 1)
                {
                    listView1.SelectedItems[0].SubItems[1].Text = name;
                    listView1.SelectedItems[0].SubItems[2].Text = id;
                    listView1.SelectedItems[0].SubItems[3].Text = inprice.ToString();
                    listView1.SelectedItems[0].SubItems[4].Text = price.ToString();
                    listView1.SelectedItems[0].SubItems[5].Text = inventory.ToString();
                    listView1.SelectedItems[0].SubItems[6].Text = kind;
                    MessageBox.Show(this, "修改成功");
                }
                else
                {
                    MessageBox.Show(this, "修改失败");
                }
            }
            catch(GoodsIDRepeatException e1)
            {
                MessageBox.Show(this, e1.Message);
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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            int n = listView1.SelectedItems[0].Index;
            string id = listView1.SelectedItems[0].SubItems[2].Text;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string sqlstr = string.Format("delete from tb_goodsinfo where goods_num='{0}';", id);
                MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                int re = comm.ExecuteNonQuery();
                if (re == 1)
                {
                    RemoveItemAndUpdate(n);
                    MessageBox.Show(this, "删除成功");
                }
                else
                {
                    MessageBox.Show(this, "删除失败");
                }
            }
            catch (MySqlException e1)
            {
                MessageBox.Show(this, "无法删除此商品，因为此商品有销售或进货记录，不可删除！");
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
        private void RemoveItemAndUpdate(int index)
        {
            listView1.Items.Remove(listView1.Items[index]);
            int n = listView1.Items.Count;
            for (int i = index; i < n; i++)
            {
                listView1.Items[i].Text = (i + 1).ToString();
            }
        }
    }
}
