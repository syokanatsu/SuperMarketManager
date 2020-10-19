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
    public partial class MemberForm : Form
    {
        public MemberForm()
        {
            InitializeComponent();
        }

        public class MemberUsernameRepeat : Exception
        {
            public MemberUsernameRepeat(string msg) : base(msg)
            {

            }
        }

        private void MemberForm_Load(object sender, EventArgs e)
        {
            //InitLvMembers();
        }
        public int InitLvMembers()
        {
            listView1.Items.Clear();
            int n = 0;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string sqlstr = "select * from tb_member;";
                MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                MySqlDataReader reader = comm.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = ((++n).ToString());
                    item.SubItems.Add(reader.GetString(1));
                    item.SubItems.Add(reader.GetString(2));
                    item.SubItems.Add(reader.GetInt32(3).ToString());
                    item.SubItems.Add(reader.GetInt32(4).ToString());
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            int point = Convert.ToInt32(numPoint.Value);
            int level = Convert.ToInt32(numLevel.Value);
            if (username.Length != 0 && password.Length != 0)
            {
                MySqlConnection conn = MysqlConnector.GetInstance();
                try
                {
                    conn.Open();
                    string mysqlstr = string.Format("select mem_id from tb_member where mem_username='{0}';", username);
                    MySqlCommand comm = new MySqlCommand(mysqlstr, conn);
                    object reo = comm.ExecuteScalar();
                    if (reo != null)
                    {
                        MessageBox.Show(this, "用户名已存在");
                    }
                    else
                    {
                        string sqlstr = string.Format("insert into tb_member(mem_username,mem_password,mem_points,mem_level) value('{0}','{1}',{2},{3});", username, password, point, level);
                        MySqlCommand comm1 = new MySqlCommand(sqlstr, conn);
                        int re = comm1.ExecuteNonQuery();
                        if (re == 1)
                        {
                            ListViewItem item = new ListViewItem();
                            item.Text = ((listView1.Items.Count + 1).ToString());
                            item.SubItems.Add(username);
                            item.SubItems.Add(password);
                            item.SubItems.Add(point.ToString());
                            item.SubItems.Add(level.ToString());
                            listView1.Items.Add(item);
                            MessageBox.Show(this, "添加成功");
                        }
                        else
                        {
                            MessageBox.Show(this, "添加失败");
                        }
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
                MessageBox.Show(this, "用户名和密码不允许为空");
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
            string username = listView1.SelectedItems[0].SubItems[1].Text;
            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                string sqlstr = string.Format("delete from tb_member where mem_username='{0}';", username);
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
                MessageBox.Show(this, "无法删除此会员，因为此管理员有购买记录，不可删除！");
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

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "未选中任何项");
                return;
            }
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            int point = Convert.ToInt32(numPoint.Value);
            int level = Convert.ToInt32(numLevel.Value);
            string oldusername = listView1.SelectedItems[0].SubItems[1].Text;

            MySqlConnection conn = MysqlConnector.GetInstance();
            try
            {
                conn.Open();
                if (!username.Equals(oldusername))
                {
                    string sqlstr = string.Format("select mem_id from tb_member where mem_username='{0}';", username);
                    MySqlCommand comm = new MySqlCommand(sqlstr, conn);
                    object re = comm.ExecuteScalar();
                    if (re != null)
                    {
                        throw new MemberUsernameRepeat("用户名已存在");
                    }
                }
                string mysqlstr = string.Format("update tb_member set mem_username='{0}',mem_password='{1}',mem_points={2},mem_level={3} where mem_username='{4}';",
                    username, password, point, level, oldusername);
                MySqlCommand comm1 = new MySqlCommand(mysqlstr, conn);
                int res = comm1.ExecuteNonQuery();
                if (res != 0)
                {
                    listView1.SelectedItems[0].SubItems[1].Text = username;
                    listView1.SelectedItems[0].SubItems[2].Text = password;
                    listView1.SelectedItems[0].SubItems[3].Text = point.ToString();
                    listView1.SelectedItems[0].SubItems[4].Text = level.ToString();
                    MessageBox.Show(this, "修改成功");
                }
                else
                {
                    MessageBox.Show(this, "修改失败");
                }
            }
            catch (MemberUsernameRepeat e1)
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

        private void listView1_Click(object sender, EventArgs e)
        {
            txtUsername.Text = listView1.SelectedItems[0].SubItems[1].Text;
            txtPassword.Text = listView1.SelectedItems[0].SubItems[2].Text;
            numPoint.Value = Convert.ToDecimal(listView1.SelectedItems[0].SubItems[3].Text);
            numLevel.Value = Convert.ToDecimal(listView1.SelectedItems[0].SubItems[4].Text);
        }
    }
}
