using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThaiNationalIDCard;
using PCSC;
using System.IO;
using System.Drawing.Imaging;

namespace testPersonalCard
{
    public partial class Form1 : Form
    {
      

        public Form1()
        {   
            InitializeComponent();
        }
        public void LogLine(string text = "")
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(new MethodInvoker(delegate { textBox1.AppendText(text + Environment.NewLine); }));
            }
            else
            {
                textBox1.AppendText(text + Environment.NewLine);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            textBox1.Text = String.Empty;
            textBox1.Refresh();
            LogLine("Loading...");
            ReadCard();
        }
        public void ReadCard()
        {

            const string path_thaicardpic = "D:\\grgbanking\\ecat\\thainationalcard\\pic\\";
            if (!Directory.Exists(path_thaicardpic))
            {
                Directory.CreateDirectory(path_thaicardpic);
            }
            ThaiIDCard idcard = new ThaiIDCard();
            Personal personal = idcard.readAllPhoto();
            textBox1.Text = String.Empty;
            textBox1.Refresh();
            var contextFactory = ContextFactory.Instance;
            using (var ctx = contextFactory.Establish(SCardScope.System))
            {
                var readerNames = ctx.GetReaders();
                
                if (IsEmpty(readerNames))
                {

                    LogLine("No reader connected.");
                    return;
                }
                textBox1.Text = String.Empty;
                textBox1.Refresh();
                var readerStates = ctx.GetReaderStatus(readerNames);
                foreach (var state in readerStates)
                {
                    var atr = BitConverter.ToString(state.Atr ?? new byte[0]);

                    if (state.ReaderName.Substring(0, 12) == "ACS CCID USB")
                    {
                        LogLine("Device Name : " + state.ReaderName);//reader name :: ACS CCID USB Reader
                        LogLine("State Device : " + state.EventState);//event name such as present card, empty card
                        LogLine("CurrentState : " + state.CurrentState);
                        LogLine("CurrentStateValue : " + state.CurrentStateValue);
                        LogLine("EventStateValue : " + state.EventStateValue);
                        LogLine("atr : " + atr);
                    }
                    else
                    {
                        //LogLine("Device Name : " + state.ReaderName);
                        //LogLine("State Device : " + state.EventState);
                        //LogLine("CurrentState : " + state.CurrentState);
                        //LogLine("CurrentStateValue : " + state.CurrentStateValue);
                        //LogLine("EventStateValue : " + state.EventStateValue);
                        //LogLine("atr : " + atr);
                    }


                    // free system resources (required for each returned state)
                    state.Dispose();
                }
            }
            if (personal != null)
                {
                //idcard.eventPhotoProgress += new handlePhotoProgress(photoProgress);
                LogLine("Citizenid : " + personal.Citizenid);
                LogLine("Birthday : " + personal.Birthday.ToString("dd/MM/yyyy"));
                LogLine("THAI_Name : " + personal.Th_Prefix+" "+personal.Th_Firstname+" "+personal.Th_Lastname);
                LogLine("ENG_Name : " + personal.En_Prefix + " " + personal.En_Firstname + " " + personal.En_Lastname);
                LogLine("Issue : " + personal.Issue.ToString("dd/MM/yyyy")); // วันออกบัตร
                LogLine("Expire : " + personal.Expire.ToString("dd/MM/yyyy")); // วันหมดอายุ
                LogLine("Address : " + personal.Address);
                LogLine("HouseNo : " + personal.addrHouseNo); // บ้านเลขที่
                LogLine("VillageNo : " + personal.addrVillageNo); // หมู่ที่
                LogLine("Lane : " + personal.addrLane); // ซอย
                LogLine("Road : " + personal.addrRoad); // ถนน
                LogLine("Subdistrict : " + personal.addrTambol);
                LogLine("District : " + personal.addrAmphur);
                LogLine("Province : " + personal.addrProvince);
                LogLine("Issuer : " + personal.Issuer);
                // LogLine("Photo : " + personal.PhotoBitmap.ToString());
                //LogLine("Image : " + string.Join(",", personal.PhotoRaw));
                //check
                string photo = string.Join(",", personal.PhotoRaw);
                Console.WriteLine(photo);
                pictureBox1.Image = personal.PhotoBitmap;
                using (Image img = Image.FromStream(new MemoryStream(personal.PhotoRaw)))
                {
                    string path = Path.GetFullPath(path_thaicardpic + personal.Citizenid+"_"+DateTime.Now.ToString("yyyymmdd_hhmmss")+".jpg");
                    img.Save(path, ImageFormat.Jpeg);
                }
            }
            else if (idcard.ErrorCode() > 0)
            {
                LogLine(idcard.Error()); // tell something when card or reader have errors.
            }
        }
        #region
        private static bool IsEmpty(ICollection<string> readerNames) =>
          readerNames == null || readerNames.Count < 1;
       
        #endregion
    }
}
