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
    public partial class Confirm : Form
    {
        public Random random = new Random();
        DataDataContext db = new DataDataContext();

        List<Ticket> Ds_Ve = new List<Ticket>();
        public Confirm()
        {
            InitializeComponent();
        }

        void XuLyDL()
        {
            this.CenterToScreen();
            List<DataGridViewRow> DongMoi = new List<DataGridViewRow>();
            DongMoi = Searching.dong_grv;


            DataGridViewRow Dong1 = new DataGridViewRow();
            DataGridViewRow Dong2 = new DataGridViewRow();


            Dong1 = DongMoi[0];

            string cost1 = Dong1.Cells[5].Value.ToString();
            int cost = int.Parse(cost1.Replace("$", "").Replace(",", ""));

            if (Searching.checkOneWay == false)
            {
                Dong2 = DongMoi[1];
                string cost2 = Dong2.Cells[5].Value.ToString();
                cost += int.Parse(cost2.Replace("$", "").Replace(",", ""));
            }

            lable_total_amount.Text = (cost * Searching.SoHanhKhach).ToString("C0");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            XuLyDL();
           
        }

        
        public char LayKTNgauNhien()
        {
            string Chuoi = "";

            for (char i = 'A'; i <= 'Z'; i++) Chuoi += i;
             
            for (char i = '1'; i <= '9'; i++) Chuoi += i;
 
            int NumberRandom = random.Next(0, Chuoi.Length - 1);

            char KiTu = Chuoi[NumberRandom];

            return KiTu;
        }

        public string Tham_Chieu_Dat_Ve()
        {
            string ChuoiNgauNhien = "";

            for (int i = 0; i < 6; i++)
                ChuoiNgauNhien += LayKTNgauNhien();

            return ChuoiNgauNhien;
        }

       

        private void btn_Issue_tickets_Click(object sender, EventArgs e)
        {
            var db = new DataDataContext();
            string ThamChieuDat = "";
            
            while (true) // ve da ton tai hay chưa
            {
                ThamChieuDat = Tham_Chieu_Dat_Ve();

                var Number_count = db.Tickets.Where(p => p.BookingReference == ThamChieuDat).ToList();

                if (Number_count.Count == 0)
                    break;
            }

            List<DataGridViewRow> ds_HanhKhach = new List<DataGridViewRow>();
            ds_HanhKhach = Booking.Khach_Dat_ve_grvr;

            List<DataGridViewRow> ds_ChuyeBay = new List<DataGridViewRow>();
            ds_ChuyeBay = Searching.dong_grv;

            var Chi_Tiet_chuyen_bay = ds_ChuyeBay[0];

            string Date_time = Chi_Tiet_chuyen_bay.Cells[2].Value.ToString();

            DateTime ngay = DateTime.ParseExact(Date_time, "MM/dd/yyyy", null);
            
            //lay ra so hieu cua chuyen bay
            string[] trung_gian = Chi_Tiet_chuyen_bay.Cells[4].Value.ToString().Split('-');
            foreach (var item in trung_gian)
            {
                Them_Ve(ThamChieuDat, ds_HanhKhach, ngay, item);
            }    

            if (Searching.checkOneWay == false)
            {
                var x1 = ds_ChuyeBay[1];

                string[] xx1 = x1.Cells[4].Value.ToString().Split('-');

                foreach (var xi in xx1)
                {
                    Them_Ve(ThamChieuDat, ds_HanhKhach, ngay, xi);
                }   
            }
            db.Tickets.InsertAllOnSubmit(Ds_Ve);
            db.SubmitChanges();
            MessageBox.Show("Đặt vé thành công" , "Messages");
        }

        void Them_Ve(string ThamChieuDat, List<DataGridViewRow> list_Passenger, DateTime date_time, string flight_Number)
        {
           
            for (int i = 0; i < list_Passenger.Count; i++)
            {
                Ticket New_Ticket = new Ticket();
                New_Ticket.UserID = 1;

                //lay so hieu chuyen bay
                New_Ticket.ScheduleID =
                    db.Schedules.Where(p => p.Date >= date_time && p.FlightNumber == flight_Number).Select(p => p.ID).FirstOrDefault();
                New_Ticket.CabinTypeID = 
                    db.CabinTypes.Where(p => p.Name == Searching.cabinn).Select(p => p.ID).FirstOrDefault();
                New_Ticket.BookingReference = ThamChieuDat;
                New_Ticket.Email = null;
                New_Ticket.Firstname = list_Passenger[i].Cells[0].Value.ToString();
                New_Ticket.Lastname = list_Passenger[i].Cells[1].Value.ToString();
                string tmp1 = list_Passenger[i].Cells[4].Value.ToString();
                New_Ticket.PassportCountryID = db.Countries.Where(p => p.Name == tmp1).Select(p => p.ID).FirstOrDefault();
                New_Ticket.Phone = list_Passenger[i].Cells[5].Value.ToString();
                New_Ticket.PassportNumber = list_Passenger[i].Cells[3].Value.ToString();
                New_Ticket.Confirmed = true;
                Ds_Ve.Add(New_Ticket);
            }
        }
    }
}
