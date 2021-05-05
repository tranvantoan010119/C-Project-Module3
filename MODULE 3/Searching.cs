﻿using System;
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
    public partial class Searching : Form
    {
        DataDataContext db = new DataDataContext();
        public static int SoHanhKhach;
        public static int number_ticket = 0;


        public List<int>[] Duong_di;
        public static List<DataGridViewRow> dong_grv = new List<DataGridViewRow>();
        public Searching()
        {
            InitializeComponent();
            Tao_Duong_Di();

            XuLy_DL(dgv_out);
            XuLy_DL(dgv_return);

            XuLy_Combobox();
        }

        public void XuLy_DL(DataGridView data_Grid)
        {
            dgv_out.RowHeadersVisible = false;
            dgv_return.RowHeadersVisible = false;

            data_Grid.Rows.Clear();


            var Lich_trinh = from Schedule in db.Schedules
                       select new
                       {
                           Schedule.ID,  Schedule.Date, Schedule.Time, Schedule.AircraftID,
                           Schedule.RouteID, Schedule.EconomyPrice, Schedule.Confirmed,
                           Schedule.FlightNumber
                       };
            foreach (var item in Lich_trinh)
            {
                int route_ID = item.RouteID;
                int airID1 = db.Routes.Where(p => p.ID == route_ID).Select(p => p.DepartureAirportID).FirstOrDefault();
                DataGridViewRow dongMoi = (DataGridViewRow)data_Grid.Rows[0].Clone();

                dongMoi.Cells[0].Value = db.Airports.Where(p => p.ID == airID1).Select(p => p.IATACode).FirstOrDefault();

                int airID2 = db.Routes.Where(p => p.ID == route_ID).Select(p => p.ArrivalAirportID).FirstOrDefault();

                dongMoi.Cells[1].Value = db.Airports.Where(p => p.ID == airID2).Select(p => p.IATACode).FirstOrDefault();

                dongMoi.Cells[2].Value = item.Date.ToString("dd/MM/yyyy");
                dongMoi.Cells[3].Value = item.Time.ToString();
                dongMoi.Cells[4].Value = item.FlightNumber;
                int idCabinType = db.CabinTypes.Where(p => p.Name == cb_cabinType.Text).Select(p => p.ID).FirstOrDefault();

                double Gia_ve = double.Parse(item.EconomyPrice.ToString());

                if (idCabinType == 2)
                {
                    Gia_ve = Gia_ve + (Gia_ve * 0.35);
                }   
                else if (idCabinType == 3)
                {
                    Gia_ve = Gia_ve + (Gia_ve + (Gia_ve * 0.35)) * 0.3;
                }   

                dongMoi.Cells[5].Value = Convert.ToDecimal(Gia_ve).ToString("C0");

                dongMoi.Cells[6].Value = 0;

                dongMoi.Cells[7].Value = item.ID;

                data_Grid.Rows.Add(dongMoi);
            }
        }

        public void XuLy_Combobox()
        {
            var airport = (from x in db.Airports select x).ToList();

            cb_from.ValueMember = "IATACode";
            cb_from.DataSource = airport;

            var airport1 = (from x in db.Airports select x).ToList();

            cb_to.ValueMember = "IATACode";
            cb_to.DataSource = airport1;

            var cabin = (from x in db.CabinTypes select x).ToList();
            cb_cabinType.DataSource = cabin;
            cb_cabinType.ValueMember = "name";
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            DialogResult dlr = MessageBox.Show("Xác nhận thoát?", "Messages", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dlr == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void rd_oneWay_CheckedChanged(object sender, EventArgs e)
        {
            if (rd_oneWay.Checked == true)
            {
                dgv_return.Hide();
                cb_return.Hide();
                label6.Hide();
            }
            else
            {
                dgv_return.Show();
                cb_return.Show();
                label6.Show();
            }
        }

        public void Tao_Duong_Di() //xay dung do thi de tim kiem
        {
            using (var db = new DataDataContext())
            {
                int n = db.Airports.Max(p => p.ID) + 1;

                Duong_di = new List<int>[n];

                for (int i = 0; i < n; i++)
                    Duong_di[i] = new List<int>();
                var select = from s in db.Routes select s;
                foreach (var data in select)
                {
                    int index = data.DepartureAirportID;
                    int val = data.ArrivalAirportID;
                    Duong_di[index].Add(val);
                }
                for (int i = 0; i < n; i++)
                    Duong_di[i] = new HashSet<int>(Duong_di[i]).ToList<int>();
            }
        }

        List<int> onePath = new List<int>();
        List<List<int>> listPath = new List<List<int>>();

        // create intermediate list 
        public List<int> addPath(List<int> localpath)
        {
            var DuongTrungGian = new List<int>();
            foreach (var item in localpath)
            {
                DuongTrungGian.Add(item);
            }   

            return DuongTrungGian;
        }

        // Thuat toan tim kiem theo chieu sau tren do thi de tim tat ca cac duong di
        public void dfs(int u, int end)
        {
            if (u == end)
            {
                listPath.Add(addPath(onePath));
                return;
            }
            foreach (var item in Duong_di[u])
            {
                if (!onePath.Contains(item))
                {
                    onePath.Add(item);
                    dfs(item, end);
                    onePath.Remove(item);
                }
            }   
            return;
        }

        // get infor path to tho datagrid
        public int Lay_So_Hieu(int x, int y)
        {
            using (var db = new DataDataContext())
            {
                var ds_SoHieu =
                    db.Routes.Where(p => p.DepartureAirportID == x && p.ArrivalAirportID == y).Select(p => p.ID).ToList();
                for (int i = 0; i < ds_SoHieu.Count; i++)
                {
                    int ii = ds_SoHieu[i];
                    if (db.Schedules.Any(p => p.RouteID == ii))
                        return ds_SoHieu[i];
                }
                return 0;
            }
        }


        private DataGridViewRow get1Path(List<int> path, int id, ComboBox cb1, ComboBox cb2)
        {
            DataGridViewRow dongmoi = (DataGridViewRow)dgv_out.Rows[0].Clone();
            var db = new DataDataContext();
            var row = db.Schedules.Where(p => p.ID == id);
            double cost = 0;
            dongmoi.Cells[0].Value = cb1.Text;
            dongmoi.Cells[1].Value = cb2.Text;
            dongmoi.Cells[2].Value = row.Select(p => p.Date).FirstOrDefault().ToString("dd/MM/yyyy");
            DateTime dt = new DateTime();
            dt = DateTime.ParseExact(dongmoi.Cells[2].Value.ToString(), "dd/MM/yyyy", null);
            dongmoi.Cells[3].Value = row.Select(p => p.Time).FirstOrDefault().ToString();
            dongmoi.Cells[4].Value += row.Select(p => p.FlightNumber).FirstOrDefault().ToString();
            cost += double.Parse(row.Select(p => p.EconomyPrice).FirstOrDefault().ToString());
            for (int i = 1; i < path.Count - 1; i++)
            {
                int idRoute = Lay_So_Hieu(path[i], path[i + 1]);
                id = db.Schedules.Where(p => (p.Date >= dt && p.RouteID == idRoute)).Select(p => p.ID).FirstOrDefault();
                if (id == 0)
                    return null;
                dt = db.Schedules.Where(p => p.ID == id).Select(p => p.Date).FirstOrDefault();
                dongmoi.Cells[4].Value += "-" + db.Schedules.Where(p => p.ID == id).Select(p => p.FlightNumber).FirstOrDefault();
                cost += double.Parse(db.Schedules.Where(p => p.ID == id).Select(p => p.EconomyPrice).FirstOrDefault().ToString());
            }
            int idCabinType = db.CabinTypes.Where(p => p.Name == cb_cabinType.Text).Select(p => p.ID).FirstOrDefault();
            if (idCabinType == 2)
                cost = cost + (cost * 0.35);
            else if (idCabinType == 3)
                cost = cost + (cost + (cost * 0.35)) * 0.3;
            dongmoi.Cells[5].Value = Convert.ToDecimal(cost).ToString("C0");
            string cnt = dongmoi.Cells[4].Value.ToString();
            int tmp = 0;
            for (int i = 0; i < cnt.Length; i++)
                if (cnt[i] == '-')
                    tmp++;
            dongmoi.Cells[6].Value = tmp;
            //
            dongmoi.Cells[7].Value = id;
            return dongmoi;
        }

        public void getPath(List<int> path, string dt, DataGridView dg, ComboBox cb1, ComboBox cb2, int chose)
        {
            var db = new DataDataContext();
            DateTime cmp = DateTime.ParseExact(dt, "dd/MM/yyyy", null);
            int idrou = Lay_So_Hieu(path[0], path[1]);
            List<int> listIdSchedules = new List<int>();
            if (chose == 1)
                listIdSchedules = db.Schedules.Where(p => p.Date >= cmp && p.RouteID == idrou).Select(p => p.ID).ToList();
            else
                listIdSchedules = db.Schedules.Where(p => p.Date == cmp && p.RouteID == idrou).Select(p => p.ID).ToList();
            foreach (int id in listIdSchedules)
            {
                DataGridViewRow newItem = get1Path(path, id, cb1, cb2);
                if (newItem != null)
                    dg.Rows.Add(newItem);
            }
        }

        public void dayDL(DataGridView dg, string date, ComboBox cb1, ComboBox cb2, int chose)
        {
            int begin = 0, end = 0;
            using (var db = new DataDataContext())
            {
                begin = db.Airports.Where(p => p.IATACode == cb1.Text).Select(p => p.ID).FirstOrDefault();
                end = db.Airports.Where(p => p.IATACode == cb2.Text).Select(p => p.ID).FirstOrDefault();
            }
            listPath.Clear();
            onePath.Clear();
            onePath.Add(begin);
            dfs(begin, end);
            dg.Rows.Clear();
            foreach (var path in listPath)
                getPath(path, date, dg, cb1, cb2, chose);
        }
        public bool checkPassNum()
        {
            if (txt_Passengers.Text == "")
                return false;
            var isNumberic = int.TryParse(txt_Passengers.Text, out _);
            if (!isNumberic)
                return false;
            var db = new DataDataContext();
            int passNum = int.Parse(txt_Passengers.Text);
            string CabinType = cb_cabinType.Text;

            var seatNum1 = db.Aircrafts.Where(p => p.ID == 1);
            var seatNum2 = db.Aircrafts.Where(p => p.ID == 1);

            int seatNum11 = 0;
            int seatNum21 = 0;
            if (CabinType == "Economy")
            {
                seatNum11 = seatNum1.Select(p => p.EconomySeats).FirstOrDefault();
                seatNum21 = seatNum2.Select(p => p.EconomySeats).FirstOrDefault();
            }
            else if (CabinType == "Business")
            {
                seatNum11 = seatNum1.Select(p => p.BusinessSeats).FirstOrDefault();
                seatNum21 = seatNum2.Select(p => p.BusinessSeats).FirstOrDefault();
            }
            else
            {
                seatNum11 = seatNum1.Select(p => p.TotalSeats - p.EconomySeats - p.BusinessSeats).FirstOrDefault();
                seatNum21 = seatNum2.Select(p => p.TotalSeats - p.EconomySeats - p.BusinessSeats).FirstOrDefault();
            }
            return passNum <= Math.Min(seatNum11, seatNum21) && 0 < passNum;
        }
        public bool checkDataRoute()
        {
            int cnt1 = dgv_out.Rows.Count;
            int cnt2 = dgv_return.Rows.Count;
            if (cnt1 == 1 || (cnt2 == 1 && rd_return.Checked))
                return false;
            int in1 = dgv_out.CurrentRow.Index.Equals(null) ? -1 : dgv_out.CurrentRow.Index;
            int in2 = dgv_return.CurrentRow.Index.Equals(null) ? -1 : dgv_return.CurrentRow.Index;
            if (in1 == -1 || (in2 == -1 && rd_return.Checked == true))
                return false;

            var x01 = dgv_out.Rows[in1];
            var x02 = dgv_return.Rows[in2];
            var t1 = DateTime.ParseExact(x01.Cells[2].Value.ToString(), "dd/MM/yyyy", null);
            var t2 = DateTime.ParseExact(x02.Cells[2].Value.ToString(), "dd/MM/yyyy", null);
            return t1 <= t2;
        }

        public bool checkTime()
        {
            if (rd_oneWay.Checked)
                return true;
            var t1 = DateTime.ParseExact(dateTime_out.Text, "dd/MM/yyyy", null);
            var t2 = DateTime.ParseExact(dateTime_return.Text, "dd/MM/yyyy", null);
            return t1 <= t2;
        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            if (!check())
            {
                MessageBox.Show("Thông tin nhập chưa đúng", "Cảnh báo!");
                return;
            }
            if (!checkTime())
            {
                MessageBox.Show("Ngày đi ngày về nhập chưa đúng", "Cảnh báo!");
                return;
            }
            dayDL(dgv_out, dateTime_out.Text, cb_from, cb_to, 0);
            if (rd_return.Checked == true)
                dayDL(dgv_return, dateTime_return.Text, cb_to, cb_from, 0);
        }
        public bool check()
        {
            if (cb_from.Text == "")
                return false;
            if (cb_to.Text == "")
                return false;
            if (dateTime_out.Text == "")
                return false;
            if (rd_return.Checked == true && dateTime_out.Text == "")
                return false;
            if (cb_from.Text == cb_to.Text)
                return false;

            return true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void cb_out_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_out.Checked)
                tickCheckBox(dgv_out, dateTime_out, cb_from, cb_to);
            else
            {
                if (!check())
                {
                    MessageBox.Show("Thông tin nhập chưa đúng", "Cảnh báo!");
                    return;
                }
                dayDL(dgv_out, dateTime_out.Text, cb_from, cb_to, 0);
            }
        }


        public void tickCheckBox(DataGridView dg, DateTimePicker dp, ComboBox c1, ComboBox c2)
        {
            if (!check())
            {
                MessageBox.Show("Thông tin nhập chưa đúng", "Cảnh báo!");
                return;
            }
            TimeSpan tp = new TimeSpan();
            tp = TimeSpan.FromDays(3);
            DateTime dt = DateTime.ParseExact(dp.Text, "dd/MM/yyyy", null);
            DateTime dt0 = dt.Subtract(tp);
            dayDL(dg, dt0.ToString("dd/MM/yyy"), c1, c2, 1);
            removeByDate(dg, dt);
        }
        public void removeByDate(DataGridView dg, DateTime date)
        {
            TimeSpan tp = new TimeSpan();
            tp = TimeSpan.FromDays(3);
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                DataGridViewRow x = new DataGridViewRow();
                x = (DataGridViewRow)dg.Rows[i];
                if (x.Cells[2].Value == null)
                    return;
                DateTime d = DateTime.ParseExact(x.Cells[2].Value.ToString(), "dd/MM/yyyy", null);
                if (date < d.Subtract(tp))
                {
                    dg.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void cb_return_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_return.Checked)
                tickCheckBox(dgv_return, dateTime_return, cb_to, cb_from);
            else
            {
                if (!check())
                {
                    MessageBox.Show("Thông tin nhập chưa đúng", "Cảnh báo!");
                    return;
                }
                dayDL(dgv_return, dateTime_return.Text, cb_to, cb_from, 0);
            }
        }

        public static DataGridView dgv1Next, dgv2Next;
        public static string cabinn;
        public static bool checkOneWay;
      
      
        private void btn_book_Click(object sender, EventArgs e)
        {
            if (!checkPassNum())
            {
                MessageBox.Show("Số lượng vé chưa đúng", "Cảnh báo!");
                return;
            }

            if (rd_return.Checked == true && !checkDataRoute())
            {
                MessageBox.Show("Thời gian của chuyến bay không khả dụng", "Cảnh báo!");
                return;
            }

            if (checkTime() == false)
            {
                MessageBox.Show("Thời gian đặt chuyến chưa hợp lệ", "Cảnh báo!");
                return;
            }

            dong_grv.Clear();
            cabinn = cb_cabinType.Text;// gan cabinn 
            int in1 = dgv_out.CurrentRow.Index; // lay dong hien tai
            int in2 = !rd_oneWay.Checked ? dgv_return.CurrentRow.Index : 0;
            dong_grv.Add(dgv_out.Rows[in1]);// add dong hien tai vao listrow
            dong_grv.Add(dgv_return.Rows[in2]);
            SoHanhKhach = int.Parse(txt_Passengers.Text); // gan so luong dat ve
            Booking myForm2 = new Booking(); //chuyen form2
            Confirm myForm3 = new Confirm();
            myForm2.Tag = dong_grv;
            myForm3.Tag = dong_grv;
            checkOneWay = rd_oneWay.Checked;
            myForm2.ShowDialog(); // hien thi
            return;
        }



        private void dgv_out_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string id = dgv_out.Rows[e.RowIndex].Cells[0].Value.ToString();
            //Lấy sản phẩm muốn xóa hoặc sửa
            Schedule spXoaSua = db.Schedules.SingleOrDefault(Schedule => Schedule.Aircraft.Name == id);

            Booking mySuaSanPham = new Booking();
            mySuaSanPham.Tag = spXoaSua;
        }

        private void dgv_out_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
           
        }

        private void dgv_out_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void dgv_return_SelectionChanged(object sender, EventArgs e)
        {
        }
    }
}
