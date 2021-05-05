using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MODULE_3
{
    public partial class Booking : Form
    {
        DataDataContext db = new DataDataContext();

        public static Schedule schedule = null;
        public bool check;
        public static List<DataGridViewRow> Khach_Dat_ve_grvr = new List<DataGridViewRow>();
        public Booking()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!checkPassDetail())
            {
                MessageBox.Show("Kiểm tra lại thông tin", "Thông báo");
                return;
            }
            DataGridViewRow dongmoi = (DataGridViewRow)dgv_passengersList.Rows[0].Clone();
            dongmoi.Cells[0].Value = txt_firstName.Text;
            dongmoi.Cells[1].Value = txt_lastName.Text;
            dongmoi.Cells[2].Value = dt_birthDate.Text;
            dongmoi.Cells[3].Value = txt_passportNumber.Text;
            dongmoi.Cells[4].Value = cb_passportCountry.Text;
            dongmoi.Cells[5].Value = txt_phone.Text;
            if (dgv_passengersList.Rows.Count - 1 == Searching.SoHanhKhach)
            {
                MessageBox.Show("Bạn đã đặt đủ số lượng vé");
                return;
            }
            for (int i = 0; i < dgv_passengersList.Rows.Count-1; i++)
            {
                if (dongmoi.Cells[3].Value.ToString() == dgv_passengersList.Rows[i].Cells[3].Value.ToString())
                {
                    MessageBox.Show("đã tồn tại passport này !");
                    return;
                }
            }
            dgv_passengersList.Rows.Add(dongmoi);
            return;
        }

        public bool checkPassDetail()
        {
            if (txt_firstName.Text == "")
                return false;
            if (txt_lastName.Text == "")
                return false;
            if (txt_passportNumber.Text == "")
                return false;
            if (txt_phone.Text == "")
                return false;
            if (dt_birthDate.Text == "")
                return false;
            if (cb_passportCountry.Text == "")
                return false;
            return true;
        }
        private void cb_passportCountry_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void LoadCombo()
        {
            var contries = (from x in db.Countries select x).ToList();
            cb_passportCountry.ValueMember = "name";
            cb_passportCountry.DataSource = contries;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadCombo();
            this.CenterToParent();
            List<DataGridViewRow> listRow = new List<DataGridViewRow>();
            listRow = (List<DataGridViewRow>)this.Tag;
            DataGridViewRow data_grv1 = new DataGridViewRow();
            DataGridViewRow data_grv_2 = new DataGridViewRow();

            data_grv1 = listRow[0];
            data_grv_2 = listRow[1];

            lb_from_outbound.Text = data_grv1.Cells[0].Value.ToString();
            lb_to_outbound.Text = data_grv1.Cells[1].Value.ToString();
            lb_cabintype_outbound.Text = Searching.cabinn;
            lb_date_outbound.Text = data_grv1.Cells[2].Value.ToString();
            lb_flightnumber_outbound.Text = data_grv1.Cells[4].Value.ToString();
            gr_return.Visible = false;

            if (Searching.checkOneWay == false)
            {
                lb_from_return.Text = data_grv_2.Cells[0].Value.ToString();
                lb_to_return.Text = data_grv_2.Cells[1].Value.ToString();
                lb_cabintype_return.Text = Searching.cabinn;
                lb_date_return.Text = data_grv_2.Cells[2].Value.ToString();
                lb_flightnumber_return.Text = data_grv_2.Cells[4].Value.ToString();
                gr_return.Visible = true;
            }
        }
        public static List<List<string>> list_passenger_dgv = new List<List<string>>();
        private void button4_Click(object sender, EventArgs e)
        {
            if (dgv_passengersList.Rows.Count - 1 != Searching.SoHanhKhach)
            {
                MessageBox.Show("Bạn đặt "+ Searching.SoHanhKhach +" vé, mời đặt thêm");
                return;
            }
            Khach_Dat_ve_grvr.Clear();
            for (int i = 0; i < dgv_passengersList.Rows.Count - 1; i++)
            {
                Khach_Dat_ve_grvr.Add((DataGridViewRow)dgv_passengersList.Rows[i]);
            }  
            Confirm form3 = new Confirm();
            form3.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            int row_index = dgv_passengersList.CurrentRow.Index;
            dgv_passengersList.Rows.RemoveAt(row_index);
            txt_firstName.Clear();
            txt_lastName.Clear();
            txt_passportNumber.Clear();
            txt_phone.Clear();
        }
    }
}
