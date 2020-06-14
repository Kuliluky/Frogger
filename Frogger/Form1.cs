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
        public static Form1 Instance = null;
        public bool logcols = true;

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

        public double energy = 100;
        public bool dashdelayed = false;
        public int dashlength;
        public static double timeMoveCost = 1.25;
        public bool paused = false;
        public uint hp = 100;
        public uint score = 0;

        public bool fullscreen = false;

        public uint cameraposy = 0;

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
            SettingsButton.Location = new Point(formwidth - 100, 10);
            logheight = 100 / lognum;
            logmargin = logheight / lognum;
            playersize = (int)(logheight / 1.5);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EnterFullScreenMode(this);
            dashlength = logheight * 2;
            logs.Add(new Log(formwidth, 0, Color.White, 0, 0));
            worker.DoWork += new DoWorkEventHandler(Mainloop);
            worker.WorkerSupportsCancellation = true;
            control = Canvas;
            worker.RunWorkerAsync();
            Form1.Instance = this;
        }
           
        private void CameraMove(int dir, int amnt) //0=up, 1=down, 2=left, 3=right
        {

        }

        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            long offsetTop;
            for (int i = 0; i < logs.Count; i++)
            {
                offsetTop = i * logheight * formheight / 100 + i * logmargin * formheight / 100 - cameraposy*formheight/100;
                Rectangle rect = new Rectangle((int)logs[i].posx*formwidth/100, (int)offsetTop, logs[i].width*formwidth/100, logheight*formheight/100);
                g.FillRectangle(new SolidBrush(logs[i].color), rect);
                if (logs[i].powerup == 1)
                {
                    rect = new Rectangle(rect.X + rect.Width / 2  - rect.Height/4, rect.Y + rect.Height / 4, rect.Height / 2, rect.Height / 2);
                    g.FillEllipse(new SolidBrush(Color.Black), rect);
                    rect = new Rectangle(rect.X + rect.Width / 2 - rect.Height / 4, rect.Y + rect.Height / 4 + 5, rect.Height / 2 - 1, rect.Height / 2 - 1);
                    g.FillEllipse(new SolidBrush(Color.Red), rect);
                }
                if (logs[i].powerup == 2)
                {
                    rect = new Rectangle(rect.X + rect.Width / 2 - rect.Height / 4, rect.Y + rect.Height / 4, rect.Height / 2, rect.Height / 2);
                    g.FillEllipse(new SolidBrush(Color.Black), rect);
                    rect = new Rectangle(rect.X + rect.Width / 2 - rect.Height / 4, rect.Y + rect.Height / 4 + 5, rect.Height / 2 - 1, rect.Height / 2 - 1);
                    g.FillEllipse(new SolidBrush(Color.DeepSkyBlue), rect);
                }
            }
            g.FillEllipse(new SolidBrush(playercol), new Rectangle((int)(posx*formwidth/100 - playersize * formheight / 100 / 2), (int)((posy-cameraposy)*formheight/100 - playersize * formheight / 100 / 2), (int)playersize*formheight/100, (int)playersize * formheight / 100));
        }
        public void makeRandLog(int width, float speed, Color col, float posx, int formwidth, float powerup)
        {
            int thiswidth;
            float thisspeed;
            Color thiscolor;
            float thisposx;
            short thispowerup;
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
                if (logcols)
                thiscolor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                else
                {
                    int temp = rand.Next(256);
                    thiscolor = Color.FromArgb(temp, temp, temp / 2);
                }
            }
            if (!float.IsNaN(posx))
            {
                thisposx = posx;
            }
            else
            {
                thisposx = (float)rand.NextDouble() * 100;
            }
            if (!float.IsNaN(powerup))
            {
                thispowerup = short.Parse(powerup.ToString());
            }
            else
            {
                int temp = rand.Next(100);
                if (temp < 10)
                {
                    thispowerup = 1;
                }
                else if (temp < 25)
                {
                    thispowerup = 2;
                }
                else
                {
                    thispowerup = 0;
                }

            }


            logs.Add(new Log(thiswidth, thisspeed, thiscolor, thisposx, thispowerup));
        }

        public void Die()
        {
            MessageBox.Show("To je konec. Utopil(a) jste se.\n Vaše skóre je: " + score.ToString());
            control.BeginInvoke((MethodInvoker)delegate ()
            {
                this.Close();
            });
        }
        void Mainloop(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < lognum; i++)
            {
                makeRandLog(0, float.NaN, new Color(), float.NaN, formwidth, float.NaN);
                distance++;
            }

            while (true)
            {
                Thread.Sleep(50);
                if (!paused)
                {
                    score = uint.Parse(GetCurrLog().ToString());
                    if (!keyspressed[5]) //=timestopped
                    {
                        for (int i = 0; i < logs.Count; i++)
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
                            switch (logs[GetCurrLog()].powerup)
                            {
                                case 2:
                                    if (energy < Int32.MaxValue - 10)
                                    {
                                        energy += 10;
                                        logs[GetCurrLog()].powerup = 0;
                                    }
                                    break;
                                case 1:
                                    if (hp < Int32.MaxValue - 10)
                                    {
                                        hp += 10;
                                        logs[GetCurrLog()].powerup = 0;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            playercol = Color.Red;
                            hp--;
                            control.BeginInvoke((MethodInvoker)delegate ()
                            {
                                label2.Text = hp.ToString();
                            });
                            if (hp == 0)
                            {
                                Die();
                            }
                        }
                    }
                    currlog = GetCurrLog();
                    PlayerMove();
                    if (posy > 50) //Move camera?
                    {
                        cameraposy = (uint)posy - 50;
                    }
                    if (end)
                    {
                        break;
                    }
                    control.BeginInvoke((MethodInvoker)delegate ()
                    {
                        Canvas.Refresh();
                    });
                }
                    if (currlog + lognum / 2 > logs.Count)
                    {
                        makeRandLog(0, float.NaN, new Color(), float.NaN, formwidth, float.NaN);
                    }
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
                    posy += playerspeed;
                    if (keyspressed[4])
                    {
                        Dash(1);
                    }
                    moved = true;
                
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
            if (!end)
            {
                control.BeginInvoke((MethodInvoker)delegate ()
                {
                    label1.Text = energy.ToString();
                });
            }
        }

        int GetCurrLog()
        {
            int temp;
            temp = posy / (logheight + logmargin);
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
            dashtemp.WorkerSupportsCancellation = true;
            dashtemp.DoWork += new DoWorkEventHandler(DashDelay);
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
            if (dashtemp.WorkerSupportsCancellation)
            {
                dashtemp.CancelAsync();
            }
            this.Close();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)//sipky, shift, ctrl
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
        private void Form1_KeyDown(object sender, KeyEventArgs e)//WASD
        {
            for (int i = 0; i < keylist2.Length; i++)
            {
                if ((int)e.KeyCode == keylist2[i])
                {
                    keyspressed[i] = true;
                }
            }
        } 

        private void Form1_KeyUp(object sender, KeyEventArgs e) //zvednuti 
        {
            for (int i = 0; i < keylist.Length; i++)
            {
                if (e.KeyValue == keylist[i] || e.KeyValue == keylist2[i])
                {
                    keyspressed[i] = false;
                }
            }
        }
        
        private void DarkenGame()
        {
            foreach (Log i in logs)
            {
                i.color = Color.FromArgb(i.color.R / 2, i.color.G / 2, i.color.B / 2);
            }
            Canvas.BackColor = Color.FromArgb(Canvas.BackColor.R / 2, Canvas.BackColor.G / 2, Canvas.BackColor.B / 2);
            playercol = Color.FromArgb(playercol.R / 2, playercol.G / 2, playercol.B / 2);
            return;
        }
        private void UndarkenGame()
        {

            foreach (Log i in logs)
            {
                i.color = Color.FromArgb(i.color.R * 2, i.color.G * 2, i.color.B * 2);
            }
            Canvas.BackColor = Color.FromArgb(Canvas.BackColor.R * 2, Canvas.BackColor.G * 2, Canvas.BackColor.B * 2);
            playercol = Color.FromArgb(playercol.R * 2, playercol.G * 2, playercol.B * 2);
            return;

        }

        private void button1_Click(object sender, EventArgs e) //fullscreen
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

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            bool restart = false;
            //TODO: display settings form
            paused = true;
            DarkenGame();
            Form2 setform = new Form2();
            setform.ShowDialog();
            logcols = (this.Text.Split(',')[0]=="True")? true:false;
            if (this.Text.Split(',')[1] == "True")
            {
                restart = true;
                logs.Clear();
                posy = 1;
            }
            paused = false;
            UndarkenGame();
            if (restart)
            {
                logs.Add(new Log(formwidth, 0, Color.White, 0, 0));
                for (int i = 0; i < lognum; i++)
                {
                    makeRandLog(0, float.NaN, new Color(), float.NaN, formwidth, float.NaN);
                    distance++;
                }
            }
        }
    }
    public class Log
    {
        public int width;
        public float speed;
        public Color color;
        public float posx;
        public short powerup;

        public Log(int width, float speed, Color col, float posx, short powerup)
        {
            this.width = width;
            this.speed = speed;
            this.color = col;
            this.posx = posx;
            this.powerup = powerup;
        }
    }
}
