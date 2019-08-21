using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace distance_field
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Converts the image in the path from a binary image to a scaled down distance field.
        /// </summary>
        void ConvertImage(string path)
        {
            System.IO.FileStream fs = System.IO.File.Open(path, System.IO.FileMode.Open);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();

            Image image = Tga.Load(bytes);

            image.Channels["A"] = DistanceField.Transform(image.Channels["A"], Int32.Parse(scaleDown.Text), float.Parse(spread.Text));
            image.Channels["R"] = new Channel(image.Width, image.Height, 1.0f);
            image.Channels["G"] = new Channel(image.Width, image.Height, 1.0f);
            image.Channels["B"] = new Channel(image.Width, image.Height, 1.0f);

            byte[] result = Tga.Save(image, Tga.UNCOMPRESSED_TRUECOLOR_IMAGE);

            fs = System.IO.File.Open(path + ".conv.tga", System.IO.FileMode.Create);
            fs.Write(result, 0, result.Length);
            fs.Close();
        }

        void ScaleIntAttribute(XmlNode node, string attribute, int scale)
        {
            node.Attributes[attribute].Value = ( (Int32.Parse(node.Attributes[attribute].Value) + scale/2) / scale).ToString();
        }

        void ScaleDoubleAttribute(XmlNode node, string attribute, int scale)
        {
            node.Attributes[attribute].Value = (Double.Parse(node.Attributes[attribute].Value) / scale).ToString();
        }

        /// <summary>
        /// Converts the specified font xml and all the images referenced by it.
        /// </summary>
        void ConvertFont(string path)
        {
            int scale = Int32.Parse(scaleDown.Text);
            
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode info = doc.SelectSingleNode("/font/info");
            ScaleIntAttribute(info, "size", scale);
            // scale padding

            XmlNode common = doc.SelectSingleNode("/font/common");
            ScaleDoubleAttribute(common, "lineHeight", scale);
            ScaleDoubleAttribute(common, "base", scale);
            ScaleDoubleAttribute(common, "scaleW", scale);
            ScaleDoubleAttribute(common, "scaleH", scale);

            XmlNodeList chars = doc.SelectNodes("/font/chars/char");
            foreach (XmlNode char_node in chars) {
                ScaleDoubleAttribute(char_node, "x", scale);
                ScaleDoubleAttribute(char_node, "y", scale);
                ScaleDoubleAttribute(char_node, "width", scale);
                ScaleDoubleAttribute(char_node, "height", scale);
                ScaleDoubleAttribute(char_node, "xoffset", scale);
                ScaleDoubleAttribute(char_node, "yoffset", scale);
                ScaleDoubleAttribute(char_node, "xadvance", scale);
            }

            XmlNodeList kernings = doc.SelectNodes("/font/kernings/kerning");
            foreach (XmlNode kerning in kernings) {
                ScaleDoubleAttribute(kerning, "amount", scale);
            }

            doc.Save(path + ".conv.fnt");

            XmlNodeList pages = doc.SelectNodes("/font/pages/page");
            foreach (XmlNode page in pages)
            {
                string image = System.IO.Path.GetDirectoryName(path) + "\\" + page.Attributes["file"].Value;
                ConvertImage(image);
            }
        }

        void Convert(object sender, EventArgs args)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Angel Code Fonts (*.fnt)|*.fnt|Images (*.tga)|*.tga";
            ofd.FilterIndex = 1;
            ofd.ShowDialog();
            if (ofd.FileName == "")
                return;

            if (System.IO.Path.GetExtension(ofd.FileName) == ".tga")
                ConvertImage(ofd.FileName);
            else
                ConvertFont(ofd.FileName);
        }

        void Quit(object sender, EventArgs args)
        {
            this.Close();
        }
    }
}
