using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace O2Yolo
{
    public partial class Launch : Form
    {
        private ClientManager ClientMGR;
        public Launch()
        {
            InitializeComponent();
            //Console.SetOut(new ControlWriter(textBox1));
        }

        public void Launch_Load(object sender, EventArgs e)
        {
            Console.WriteLine("O2Yolo-Alpha v0.1 Booting up!");
            ClientMGR = new ClientManager(15010);

            //ws = new Server("127.0.0.1", 15010);
            //ws.OnUpdateStatus += this.OnBuffer;
        }

        public class ControlWriter : TextWriter
        {
            private Control textbox;
            delegate void SetTextCallBack(string text);
            public ControlWriter(Control textbox)
            {
                this.textbox = textbox;
            }

            public override void Write(char value)
            {
                if (textbox.InvokeRequired)
                {
                    SetTextCallBack d = new SetTextCallBack(Write);
                    textbox.Invoke(d, new object[] { value.ToString() });
                } else
                {
                    textbox.Text += value;
                }
            }

            public override void Write(string value)
            {
                if (textbox.InvokeRequired)
                {
                    SetTextCallBack d = new SetTextCallBack(Write);
                    textbox.Invoke(d, new object[] { value.ToString() });
                }
                else
                {
                    textbox.Text += value;
                }
            }

            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }
    }
}
