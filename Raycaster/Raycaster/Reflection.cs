using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Raycaster.BasicRays;

namespace Raycaster
{
    public partial class Reflection : Form
    {
        public Reflection()
        {
            InitializeComponent();
        }

        static int nr_of_rays = 10;
        static int nr_of_blocks = 30;
        int max_size = 60; //max width/height for blocks
        int min_size = 30; //min width/height for blocks
        static int source_size = 30; //size of the source of light
        static int sourcex, sourcey;
        double angle_growth_rate = 360 / nr_of_rays;
        int centerx, centery; //the starting point for all the main rays is the center of the source
        Random rnd;
        double current_angle = 0; //for the main rays
        int currentindex = 1; //for the main rays

        public class block
        {
            public int x, y, width, height;
            public Rectangle r;

            public block(int x, int y, int width, int height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                r = new Rectangle(x, y, width, height);
            }
        }
        public class ray
        {
            public double startx, starty;
            public double angle; //measured in 0->180 degrees
            public double endx, endy;
            public string endside; // can take values: v,h or wall;
            //(vertical or horizontal side of block)
            public ray(double startx, double starty, double endx, double endy, double angle)
            {
                this.startx = startx;
                this.starty = starty;
                this.endx = endx;
                this.endy = endy;
                this.angle = angle;
            }
        }
        block[] blocks = new block[100];
        ray[] rays = new ray[100]; //for main rays
        int copy;

        public void generate_blocks()
        {
            for (int i = 1; i <= nr_of_blocks; i++)
            {
                rnd = new Random();
                int pozx = rnd.Next(0, this.Width - max_size);
                Thread.Sleep(50);
                int pozy = rnd.Next(0, this.Height - max_size);
                Thread.Sleep(50);
                int w = rnd.Next(min_size, max_size);
                Thread.Sleep(50);
                int h = rnd.Next(min_size, max_size);
                blocks[i] = new block(pozx, pozy, w, h);

            }
        }

        public void generate_source()
        {
            do
            {
                rnd = new Random();
                sourcex = rnd.Next(0, this.Width - source_size);
                Thread.Sleep(50);
                sourcey = rnd.Next(0, this.Height - source_size);
                Thread.Sleep(50);
            }
            while (!(source_fits()));
            centerx = sourcex + source_size / 2;
            centery = sourcey + source_size / 2;
        }

        public bool source_fits()
        {
            Rectangle source = new Rectangle(sourcex, sourcey, source_size, source_size);
            foreach (var block in blocks)
                if (block != null)
                    if (source.IntersectsWith(block.r))
                        return false;
            return true;
        }

        public void generate_rays()
        {
            for (currentindex = 1; currentindex <= nr_of_rays; currentindex++)
            {
                current_angle = (currentindex - 1) * angle_growth_rate;
                rays[currentindex] = new ray(centerx, centery, centerx, centery, current_angle); //before tracing,the starting point is the same as the endpoint;

                copy = currentindex; //use copy to save currentindex as we will use this and the rays array to iterate further reflexions until the next main ray;
                trace_ray(currentindex);
                draw_ray_and_reflections(this.CreateGraphics());
                currentindex = copy;
            }
            draw_blocks_and_source(this.CreateGraphics());
        }

        public void trace_ray(int i)
        {
            double radians_angle = Math.PI * rays[currentindex].angle / 180.0;
            double sinAngle = Math.Sin(radians_angle);
            double cosAngle = Math.Cos(radians_angle);
            double tg = sinAngle / cosAngle;
            double copyx, copyy;
            do
            {
                copyx = rays[currentindex].endx;
                copyy = rays[currentindex].endy;
                if (cadran(currentindex) == 1 || cadran(currentindex) == 4)
                    rays[currentindex].endx += 1;
                else
                    rays[currentindex].endx -= 1; ;
                rays[currentindex].endy = (tg * rays[currentindex].endx) + (rays[currentindex].starty - tg * rays[currentindex].startx);
            } while (ray_doesnt_hit_wall(i));

            rays[currentindex].endx = copyx;
            rays[currentindex].endy = copyy;
            if (String.Compare(rays[currentindex].endside, "wall") != 0) //recursive algorithm for generating reflections
                generate_reflection();
        }

        public void reduce_angle(int i) //reduces angle to a value between 0 and 180
        {
            double angle = rays[i].angle+360;
            while (angle > 360) angle = angle - 360;
            rays[i].angle = angle;
        }

        public int cadran(int i) //return angle's cadran
        {
            double angle = rays[i].angle;
            if (angle >= 0 && angle <= 90) return 1;
            if (angle >= 90 && angle <= 180) return 2;
            if (angle >= 180 && angle <= 270) return 3;
            if (angle >= 270 && angle <= 360) return 4;
            return 0;
        }

        public void generate_reflection() //The angle is relative to the surface's normal;the x and y are coordonites where the ray meets block
        {
            reduce_angle(currentindex);
            currentindex++;
            if (currentindex <= 15)
            {
                double newangle = 0;
                if (rays[currentindex - 1].endside == "h")
                        newangle = 360- rays[currentindex - 1].angle;
                if (rays[currentindex - 1].endside == "v" && cadran(currentindex - 1) <= 2)
                    newangle = 180 - rays[currentindex - 1].angle;
                if (rays[currentindex - 1].endside == "v" && cadran(currentindex - 1) > 2)
                    newangle = 360 - rays[currentindex - 1].angle + 180;
                rays[currentindex] = new ray(rays[currentindex - 1].endx, rays[currentindex - 1].endy, rays[currentindex - 1].endx, rays[currentindex - 1].endy, newangle);
                trace_ray(currentindex);
            }
        }

        public void draw_ray_and_reflections(Graphics g)
        {
            Pen black = new Pen(Color.Black);
            foreach (var ray in rays)
                if (ray != null)
                    g.DrawLine(black, Convert.ToInt32(ray.startx), Convert.ToInt32(ray.starty), Convert.ToInt32(ray.endx), Convert.ToInt32(ray.endy));
        }

        public bool ray_doesnt_hit_wall(int i)
        {
            double currx = rays[i].endx;
            double curry = rays[i].endy;
            //check if the ray exited the form(in this case ray doesnt come back)
            if (currx >= this.Width || curry >= this.Height || currx <= 0 || curry <= 0)
            {
                rays[i].endside = "wall";
                return false;
            }
            //check if ray hit block
            Rectangle current_position = new Rectangle(Convert.ToInt32(currx), Convert.ToInt32(curry), 2, 2);
            foreach (var block in blocks)
                if (block != null)
                    if (current_position.IntersectsWith(block.r))
                    {
                        //see which sides of the square are intersected so you can calculate angle relative to the surfaces's normal
                        //if ray hits verticla sides: newangle=(-1)*oldangle | if angle hits horizonta; sides: newangle=Abs(180-oldangle)
                        Rectangle v1 = new Rectangle(block.r.X, block.r.Y, 10, block.r.Height);
                        Rectangle v2 = new Rectangle(block.r.X + block.r.Width, block.r.Y, 10, block.r.Height);
                        if (current_position.IntersectsWith(v1) || current_position.IntersectsWith(v2))
                            rays[i].endside = "v";
                        else
                            rays[i].endside = "h";
                        return false;
                    }
            return true;
        }

        public void Start()
        {
            generate_blocks();
            generate_source();
            generate_rays();
        }

        public void draw_blocks_and_source(Graphics g)
        {
            SolidBrush brush = new SolidBrush(Color.Red);
            SolidBrush solidBrush = new SolidBrush(Color.Black);
            Rectangle source = new Rectangle(sourcex, sourcey, source_size, source_size);  //source:
            g.FillEllipse(solidBrush, source);
            foreach (var block in blocks)   //blocks:
                if (block != null)
                    g.FillRectangle(brush, block.r);
        }

        private void Reflection_Click(object sender, EventArgs e)
        {
            Start();
        }
    }
}
