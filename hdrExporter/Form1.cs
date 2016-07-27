using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hdrExporter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public byte[] ReadAllBytes(string fileName)
        {
            byte[] buffer = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
            }
            return buffer;
        }
        public byte[] belliYeriOku(string dosyaAdi,int boyut,int offset)
        {
            byte[] tmp = null;
            using(FileStream stream = new FileStream(dosyaAdi,FileMode.Open,FileAccess.Read))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                tmp = new byte[boyut];
                stream.Read(tmp, 0, boyut);
            }
            return tmp;
        }
        public void olusturYaz(string yeniDosyaAdi,byte[] veri,int boyut)
        {
            using (FileStream fs =  File.Create(yeniDosyaAdi))
            {
                fs.Write(veri, 0, boyut);  
            }
        }
        public void Cikart(string Klasor,string hedefSrc,string dosyaAdi,int adres,int boyut)
        {
            string[] parcala = hedefSrc.Split('.');
            string srcAdres = Klasor + "\\" + parcala[0] + ".src";
            byte[] okunan = belliYeriOku(srcAdres, boyut, adres);
            string[] str = dosyaAdi.Split('\\');
            string exporterKlasor = Klasor + "\\hdrExporter\\";
            if (!Directory.Exists(exporterKlasor))
            {
                Directory.CreateDirectory(exporterKlasor);
            }
            if (str.Length == 1)
            {
                exporterKlasor += dosyaAdi;
            }
            else 
            {
                for (int i = 0; i < str.Length -1 ; i++)
                {
                    exporterKlasor += str[i] + "\\";
                }
                if (!Directory.Exists(exporterKlasor))
                {
                    Directory.CreateDirectory(exporterKlasor);
                }
                exporterKlasor += str[str.Length - 1];
            }

            //Console.WriteLine(Klasor + "\\hdrExporter\\" + dosyaAdi);
            olusturYaz(exporterKlasor, okunan, boyut);
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            toolStripLabel1.Text = "Hazir";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "HDR files|*.hdr";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            int i = 0;
            int index = 0;
            byte[] pDatas = null;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pDatas = ReadAllBytes(openFileDialog1.FileName);
                    int toplamKayit = BitConverter.ToInt32(pDatas, index); index += 4;
                    while (toplamKayit > i)
                    {
                        int dosyaIsimUzunluk = BitConverter.ToInt32(pDatas, index); index += 4;
                        byte[] dosya = new byte[dosyaIsimUzunluk];
                        Array.Copy(pDatas, index, dosya, 0, dosyaIsimUzunluk);
                        string dosyaAdi = Encoding.UTF8.GetString(dosya, 0, dosyaIsimUzunluk); index += dosyaIsimUzunluk;
                        int adres = BitConverter.ToInt32(pDatas, index); index += 4;
                        int boyut = BitConverter.ToInt32(pDatas, index); index += 4;
                        string adr = String.Format("{0:X}", adres);
                        string tmp = "0x" +  string.Concat(Enumerable.Repeat("0", 8 - adr.Length)) + adr;
                        i++;
                        Cikart(Path.GetDirectoryName(openFileDialog1.FileName),openFileDialog1.SafeFileName, dosyaAdi, adres,boyut);
                        itemEkle(Convert.ToString(i),  tmp, Convert.ToString(boyut) + " bytes", dosyaAdi);
                    }
                    toolStripLabel1.Text = Convert.ToString(toplamKayit) + " tane dosya cikartildi";
                    MessageBox.Show("Cikartma İslemi Tamamlandi");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("pDatas.Length : {0}, index : {1}", pDatas.Length, index);
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns.Add("#", 50);
            listView1.Columns.Add("Adres",75);
            listView1.Columns.Add("Dosya Boyutu", 85);
            listView1.Columns.Add("Dosya Adı", 260);
        }
        private void itemEkle(string sira,string adres,string boyut,string dosyaAdi)
        {
            string[] items = {sira,adres,boyut,dosyaAdi };
            ListViewItem itm = new ListViewItem(items);
            listView1.Items.Add(itm);

        }
    }
}
