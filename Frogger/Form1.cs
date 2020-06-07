using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Frogger
{
    public partial class Form1 : Form
    {

        public Control control;
        BackgroundWorker worker = new BackgroundWorker();
        BackgroundWorker dashtemp = new BackgroundWorker();

        public Random rand = new Random();
        public List<Log> logs = new List<Log>();
        public int logmargin = 10;
        public int logheight = 5;
        public static int lognum = 10; //mnozstvi drev na obrazovce

        public bool[] keyspressed = { false, false, false, false, false, false }; //up, down ,left ,right, dash, timestopped
        public static int[] keylist = { 38, 40, 37, 39, 16, 17 };
        public static int[] keylist2 = { 87, 83, 65, 68, 16, 17 };
        public float posx = 50;
        public int posy = 1;
        public static float playersize = 15;
        public /*static */Color playercol = Color.Black;
        public static int playerspeed = 2;

        public double energy = double.MaxValue;
        public bool dashdelayed = false;
        public int dashlength;
        public static double timeMoveCost = 1.25;

        public bool fullscreen = false;

        public bool end = false;
        public int distance = 0;

        public void EnterFullScreenMode(Form targetForm)
        {
            fullscreen = true;
            targetForm.WindowState = FormWindowState.Normal;
            targetForm.FormBorderStyle = FormBorderStyle.None;
            targetForm.WindowState = FormWindowState.Maximized;
            Form1_ResizeEnd(targetForm, null);
        }
        public void LeaveFullScreenMode(Form targetForm)
        {
            fullscreen = false;
            targetForm.WindowState = FormWindowState.Normal;
            Form1_ResizeEnd(targetForm, null);
        }
        public int formwidth = 0;
        public int formheight = 0;

        public int currlog = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            formheight = this.Height;
            formwidth = this.Width;
            Canvas.Size = new Size(formwidth, formheight);
            CloseButton.Location = new Point(formwidth - 50, 10);
            MaximizeButton.Location = new Point(formwidth - 75, 10);
            logheight = 100 / lognum;
            logmargin = logheight / lognum;
            playersize = (int)(logheight / 1.5);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EnterFullScreenMode(this);
            dashlength = logheight * 2;
            logs.Add(new Log(formwidth, 0, Color.White, 0));
            worker.DoWork += new DoWorkEventHandler(Mainloop);
            worker.WorkerSupportsCancellation = true;
            control = Canvas;
            worker.RunWorkerAsync();
        }
           
        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int offsetTop;
            for (int i = 0; i < logs.Count; i++)
            {
                offsetTop = i * logheight * formheight / 100 + i * logmargin * formheight / 100;
                Rectangle rect = new Rectangle((int)logs[i].posx*formwidth/100, offsetTop, logs[i].width*formwidth/100, logheight*formheight/100);
                g.FillRectangle(new SolidBrush(logs[i].color), rect);
            }
            g.FillEllipse(new SolidBrush(playercol), new Rectangle((int)(posx*formwidth/100 - playersize * formheight / 100 / 2), (int)(posy*formheight/100 - playersize * formheight / 100 / 2), (int)playersize*formheight/100, (int)playersize * formheight / 100));
        }
        public void makeRandLog(int width, float speed, Color col, float posx, int formwidth)
        {
            int thiswidth;
            float thisspeed;
            Color thiscolor;
            float thisposx;
            if (width != 0)
            {
                thiswidth = width;
            }
            else
            {
                thiswidth = rand.Next(10, 20);
            }
            if (!float.IsNaN(speed))
            {
                thisspeed = speed;
            }
            else if (distance < 25)
            {
                thisspeed = (float)(rand.Next(-10, 10) * (Math.Sqrt(distance))/10);
            }
            else
            {
                thisspeed = (float)(rand.Next(-100, 100) * 0.5);
            }
            if (!col.IsEmpty)
            {
                thiscolor = col;
            }
            else
            {
                thiscolor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            }
            if (!float.IsNaN(posx))
            {
                thisposx = posx;
            }
            else
            {
                thisposx = (float)rand.NextDouble() * 100;
            }
            logs.Add(new Log(thiswidth, thisspeed, thiscolor, thisposx));
        }
        void Mainloop(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                makeRandLog(0, float.NaN, new Color(), float.NaN, formwidth);
                distance++;
            }

            while (true)
            {
                Thread.Sleep(50);
                if (!keyspressed[5]) //=timestopped
                {
                    for (int i = 0; i < lognum; i++)
                    {
                        logs[i].posx += logs[i].speed;
                        if (logs[i].speed > 0)
                        {
                            if (logs[i].posx > 100)
                            {
                                logs[i].posx = -logs[i].width;
                            }
                        }
                        else if (logs[i].speed < 0)
                        {
                            if (logs[i].posx < -logs[i].width)
                            {
                                logs[i].posx = 100 + logs[i].width;
                            }
                        }
                    }

                    if (logs[currlog].speed > 0)
                    {
                        if (posx < 100)
                            posx += logs[currlog].speed;

                    }
                    else
                    {
                        if (posx > 0)
                            posx += logs[currlog].speed;

                    }
                    if (OnLog())
                    {
                        playercol = Color.Black;
                    }
                    else
                    {
                        playercol = Color.Red;
                    }
                }
                currlog = GetCurrLog();
                PlayerMove();
                if (end)
                {
                    break;
                }
                control.BeginInvoke((MethodInvoker)delegate ()
                {
                    Canvas.Refresh();
                });
            }
            return;
        }
        
        void PlayerMove()
        {
            bool moved = false;

             if (keyspressed[0])
            {
                if (posy > 0)
                {
                    posy -= playerspeed;
                    if (keyspressed[4] && posy > dashlength)
                    {
                        Dash(0);
                    }
                    moved = true;
                }

            }

            if (keyspressed[1])
            {
                if (posy < 100)
                {
                    posy += playerspeed;
                    if (keyspressed[4])
                    {
                        Dash(1);
                    }
                    moved = true;
                }
            }

            if (keyspressed[2])
            {
                if (posx > 0)
                {
                    posx -= playerspeed;
                    if (keyspressed[4])
                    {
                        Dash(2);
                    }
                    moved = true;
                }
            }

            if (keyspressed[3])
            {
                if (posx < 100)
                {
                    posx += playerspeed;

                    if (keyspressed[4])
                    {
                        Dash(3);
                    }
                    moved = true;
                }
            }
            if (moved && keyspressed[5])
            {
                if (energy < timeMoveCost)
                {
                    keyspressed[5] = false;
                }
                else
                energy -= timeMoveCost;

            }
            control.BeginInvoke((MethodInvoker)delegate ()
            {
                label1.Text = energy.ToString();
            });
        }

        int GetCurrLog()
        {
            int temp = posy / (logheight + logmargin);
            return (temp);
        }

        void DashDelay(object sender, DoWorkEventArgs e)
        {
            dashdelayed = true;
            Thread.Sleep(100);
            dashdelayed = false;
            return;
        }
        void Dash(int direction) //Directions: 0 = up, 1 = down, 2 = left, 3 = right
        {
            if (dashdelayed || dashtemp.IsBusy)
                return;
            if (energy < 1)
            {

                control.BeginInvoke((MethodInvoker)delegate ()
                {
                    label1.ForeColor = Color.Red;
                });
                return;
            }
            switch (direction)
            {
                case 0:
                    posy -= dashlength;
                break;
                case 1:
                    posy += dashlength;
                break;
                case 2:
                    posx -= dashlength;
                break;
                case 3:
                    posx += dashlength;
                break;
            }
            dashtemp.DoWork += new DoWorkEventHandler(DashDelay);
            dashtemp.WorkerSupportsCancellation = true;
            dashtemp.RunWorkerAsync();
            energy--;
            return;
        }

        bool OnLog()
        {
            return (posx > logs[currlog].posx && posx < logs[currlog].posx + logs[currlog].width);
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
            dashtemp.CancelAsync();
            this.Close();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                keyspressed[0] = true;
                return true;
            }
            if (keyData == Keys.Down)
            {
                keyspressed[1] = true;
                return true;
            }
            if (keyData == Keys.Left)
            {
                keyspressed[2] = true;
                return true;
            }
            if (keyData == Keys.Right)
            {
                keyspressed[3] = true;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < keylist2.Length; i++)
            {
                if ((int)e.KeyCode == keylist2[i])
                {
                    keyspressed[i] = true;
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < keylist.Length; i++)
            {
                if (e.KeyValue == keylist[i] || e.KeyValue == keylist2[i])
                {
                    keyspressed[i] = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fullscreen)
            {
                LeaveFullScreenMode(this);
            }
            else
            {
                EnterFullScreenMode(this);
            }
        }
    }
    public class Log
    {
        public int width;
        public float speed;
        public Color color;
        public float posx;

        public Log(int width, float speed, Color col, float posx)
        {
            this.width = width;
            this.speed = speed;
            this.color = col;
            this.posx = posx;
        }
    }
}
