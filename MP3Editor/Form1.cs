using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using TagLib;

namespace WindowsFormsApplication1
{

    public partial class MainFrame : Form
    {
        public MainFrame()
        {
            InitializeComponent();
        }

        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            } catch (Exception ex) { }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            } catch (Exception ex) { }
            return objectOut;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Navigate to your folder with MP3 files you would like to change.";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBoxPath.Text = fbd.SelectedPath;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (!validLocation()) return;
            if (dataTable.Rows.Count <= 1) updateTable();
            this.Enabled = false;
            string[] filePaths = Directory.GetFiles(textBoxPath.Text, "*.mp3", SearchOption.TopDirectoryOnly);
            foreach (string path in filePaths) {
                TagLib.File file = TagLib.File.Create(@path);
                string fileName = @path.Substring(@path.LastIndexOf(@"\") + 1);
                string jpgPath = null;
                try  {
                    jpgPath = path.Substring(0, @path.LastIndexOf(".")) + ".jpg";
                    file.Tag.Pictures = new TagLib.IPicture[]
                    {
                    new TagLib.Picture(new TagLib.ByteVector((byte[])new System.Drawing.ImageConverter().ConvertTo(System.Drawing.Image.FromFile(jpgPath), typeof(byte[]))))
                    };
                } catch (Exception error) { }
                if (@fileName.Contains("-"))
                    {
                        try
                        {
                            string[] artistTitle = fileName.Split('-');
                            string[] artists = new string[1];
                            artists[0] = new CultureInfo("en-US").TextInfo.ToTitleCase(artistTitle[0]);
                            file.Tag.Performers = artists;
                            string title = artistTitle[1].Substring(0, artistTitle[1].LastIndexOf(".")).Trim();
                            file.Tag.Title = new CultureInfo("en-US").TextInfo.ToTitleCase(title);
                        }
                        catch (Exception er) { }
                    }
                    file.Save();

                    if (checkBox.Checked)
                    {
                        string targetPath = @path.Substring(0, @path.LastIndexOf("."));
                        if (!System.IO.Directory.Exists(targetPath))
                        {
                            System.IO.Directory.CreateDirectory(targetPath);
                        }
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(path, destFile, true);
                    }
                    // System.IO.File.Delete(path);
            } 
            this.Enabled = true;
            MessageBox.Show("All done!");
        }

        private void MainFrame_Load(object sender, EventArgs e)
        {
            dataTable.MouseClick += new MouseEventHandler(dataTable_CellContentClick);
            FormClosed += MyClosedHandler;
            string path = @System.IO.Directory.GetCurrentDirectory() + @"\path.bin";
            if (System.IO.File.Exists(@path)) textBoxPath.Text = DeSerializeObject<string>(@path);
        }

        private void dataTable_CellContentClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    int position_xy_mouse_row = dataTable.HitTest(e.X, e.Y).RowIndex;
                    DataGridViewRow selectedRow;
                    try { selectedRow = dataTable.Rows[position_xy_mouse_row]; } catch (Exception OutOfRangeError) { return; }
                    selectedRow = dataTable.Rows[position_xy_mouse_row];
                    string fileName = Convert.ToString(selectedRow.Cells[0].Value);
                    TagLib.File file = TagLib.File.Create(@textBoxPath.Text + @"\" + fileName + ".mp3");
                    System.Diagnostics.Debug.Write(file.Tag.Title);
                    Globals.setFile(file);
                    textBoxTitle.Text = file.Tag.Title;
                    try { textBoxArtist.Text = file.Tag.Performers[0]; }
                    catch (Exception OutOfBoundsError) { textBoxArtist.Text = ""; }
                    textBoxAlbum.Text = file.Tag.Album;
                    if (file.Tag.Year != 0) textBoxYear.Text = file.Tag.Year.ToString();
                    else textBoxYear.Text = "";
                    try { textBoxGenre.Text = file.Tag.Genres[0]; }
                    catch (Exception OutOfBoundsError) { textBoxGenre.Text = ""; }
                    try
                    {
                        MemoryStream ms = new MemoryStream(file.Tag.Pictures[0].Data.Data);
                        System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                        imageBox.Image = image;
                    }
                    catch (Exception error) { imageBox.Image = null; }
                }
            }
            catch (Exception er) { }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void updateTable()
        {
            if (!validLocation()) return;
            string[] filePaths = Directory.GetFiles(textBoxPath.Text, "*.mp3", SearchOption.TopDirectoryOnly);
            foreach (string mp3Path in filePaths)
            {
                string fileName = @mp3Path.Substring(@mp3Path.LastIndexOf(@"\") + 1);
                if (fileName.Contains(".")) fileName = fileName.Substring(0, fileName.LastIndexOf("."));
                dataTable.Rows.Add(fileName);
            }
        }

        private Boolean validLocation()
        {
            Boolean valid = false;
            try
            {
                string[] filePaths = Directory.GetFiles(textBoxPath.Text, "*.mp3", SearchOption.TopDirectoryOnly);
                if (filePaths.Length > 0) valid = true;
            }
            catch (Exception e) { }
            return valid;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            TagLib.File file = Globals.getFile(); // Grab selected audio file
            if (file == null) return; // If no file is selected, exit method
            file.Tag.Title = textBoxTitle.Text; // Save title to audio file
            string[] performer = new string[1]; //<--- Save artist to audio file
            performer[0] = textBoxArtist.Text; // ---- 
            file.Tag.Performers = performer;  //  ---> 
            file.Tag.Album = textBoxAlbum.Text; // Save album to audio file
            try
            {
                if (textBoxYear.Text.Equals("")) file.Tag.Year = 0; // If year textbox is empty, change to zero
                else file.Tag.Year = uint.Parse(textBoxYear.Text); // If not empty, parse number from text and save as year to audio file
            } catch (Exception ParseError) { file.Tag.Year = 0; }
            string[] genre = new string[1]; // <--- Save genre to audio file
            genre[0] = textBoxGenre.Text;   //  ----
            file.Tag.Genres = genre;        //  ----> 
            file.Save();                    // Submit changes to audio file
            MessageBox.Show("Your changes have been saved!");
        }

        private void textBoxTitle_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            dataTable.Rows.Clear();
            updateTable();
        }

        private void dataTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void buttonImageLocation_Click(object sender, EventArgs e)
        {
            TagLib.File file = Globals.getFile();
            if (file == null) return;
            OpenFileDialog obd = new OpenFileDialog();
            obd.Filter = "JPG Files (.jpg)|*.jpg|All Files (*.*)|*.*";
            obd.FilterIndex = 1;
            if (obd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string jpgPath = obd.FileName;
                file.Tag.Pictures = new TagLib.IPicture[]
                {
                    new TagLib.Picture(new TagLib.ByteVector((byte[])new System.Drawing.ImageConverter().ConvertTo(System.Drawing.Image.FromFile(jpgPath), typeof(byte[]))))
                };
                MemoryStream ms = new MemoryStream(file.Tag.Pictures[0].Data.Data);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                imageBox.Image = image;
            }
        }

        private void imageBox_Click(object sender, EventArgs e)
        {
        }

        protected void MyClosedHandler(object sender, EventArgs e)
        {
            string path = @System.IO.Directory.GetCurrentDirectory() + @"\path.bin";
            SerializeObject<string>(textBoxPath.Text, @path);
            System.Diagnostics.Debug.Write("path.bin created!");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}

public class Globals
{
    private static string path;
    private static TagLib.File file;
    public static string getPath()
        {
            return path;
        }
    public static void setPath(string value)
        {
        path = value;
        }

    public static TagLib.File getFile()
    {
        return file;
    }
    public static void setFile(TagLib.File value)
    {
        file = value;
    }
}