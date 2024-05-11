using System.Drawing;
using System.IO;
using System.Threading;
namespace Raycaster
{
    public partial class BasicRays : Form
    {
        public BasicRays()
        {
            InitializeComponent();
        }

        /*todo
         * .generate rays
         */

        static int nr_of_rays = 30;
        static int nr_of_blocks = 10;
        int max_size = 90; //max width/height for blocks
        int min_size = 50; //min width/height for blocks
        int source_size=30; //size of the source of light
        int sourcex, sourcey;
        int centerx, centery;  //the center of the source is the starting point for all the rays
        Random rnd;
        public class block
        {
            public int x,y,width,height;
            public Rectangle r;

            public block(int x, int y, int width, int height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                r=new Rectangle(x, y, width, height);
            }
        }
        public class ray
        {
            public double endx, endy;
            public ray(double endx, double endy)
            {
                this.endx = endx;
                this.endy = endy;
            }
        }
        block[] blocks = new block[nr_of_blocks+1];
        ray[] rays = new ray[nr_of_rays+1];

        public void generate_blocks()
        {
            for (int i = 0; i < nr_of_blocks; i++)
            {
                rnd = new Random();
                int pozx = rnd.Next(0, this.Width);
                Thread.Sleep(50);
                int pozy = rnd.Next(0, this.Height);
                Thread.Sleep(50);
                int w = rnd.Next(min_size, max_size);
                Thread.Sleep(50);
                int h = rnd.Next(min_size, max_size);
                blocks[i] = new block(pozx, pozy, w, h);
            }
        }

        public void generate_source()
        {
            rnd = new Random();
            do
            {
                sourcex = rnd.Next(0, this.Width-source_size);
                Thread.Sleep(50);
                sourcey = rnd.Next(0, this.Height-source_size);
                Thread.Sleep(50);
            }
            while (!(source_fits()));
        }

        public bool source_fits()
        {
            Rectangle source = new Rectangle(sourcex, sourcey, source_size, source_size);
            foreach (var block in blocks)
                if(block!=null)
                 if (source.IntersectsWith(block.r))
                    return false;
            return true;
        }

        private void BasicRays_Load(object sender, EventArgs e)
        {
            generate_blocks();
            generate_source();
            generate_rays();
            this.Refresh();
        }

        public void generate_rays()
        {
            double current_angle = 0;
            double angle_growth_rate = 360 / nr_of_rays;
            centerx = sourcex + source_size / 2;
            centery = sourcey + source_size / 2;
            for(int i=1; i<=nr_of_rays; i++)
            {
                double radians_angle = Math.PI * current_angle / 180.0;
                double sinAngle = Math.Sin(radians_angle);
                double cosAngle = Math.Cos(radians_angle);
                double tg = sinAngle / cosAngle;
                //y - centery = tg*(x-centerx)
                // y = tgx * x + (centery-tg*centerx) ; y =mx+n
                double endx = centerx;
                double endy = centery;
                while(ray_doesnt_hit_wall(endx,endy))
                {
                    if(current_angle>=0 && current_angle<=90 || current_angle>=270 && current_angle<=360)
                        endx = endx + 10;
                    else
                        endx = endx - 10;
                    endy = (tg * endx) + (centery - tg * centerx);
                }
                rays[i]=new ray(endx, endy);
                current_angle = current_angle + angle_growth_rate;
            }
        }

        public bool ray_doesnt_hit_wall(double currx,double curry)
        {
            //check if the ray exited the form
            if(currx>=this.Width || curry>=this.Height) return false;   
            if(currx<=0 || curry<=0) return false;
            //check if ray hit block
            Rectangle current_position = new Rectangle(Convert.ToInt32(currx),Convert.ToInt32(curry),2,2);
            foreach (var block in blocks)
                if(block!=null)
                if (current_position.IntersectsWith(block.r))
                    return false;
            return true;
        }

        private void BasicRays_Paint(object sender, PaintEventArgs e)
        {
            Pen black = new Pen(Color.Black);
            SolidBrush brush = new SolidBrush(Color.Red);
            SolidBrush solidBrush = new SolidBrush(Color.Black);
            Rectangle source = new Rectangle(sourcex, sourcey, source_size, source_size);  //source:
            e.Graphics.FillEllipse(solidBrush, source);
            foreach (var ray in rays)             //rays:
                if (ray != null)
                    e.Graphics.DrawLine(black, centerx, centery, Convert.ToInt32(ray.endx), Convert.ToInt32(ray.endy));
            foreach (var block in blocks)   //blocks:
                if(block!=null)
                e.Graphics.FillRectangle(brush, block.r);
        }
    }
}
