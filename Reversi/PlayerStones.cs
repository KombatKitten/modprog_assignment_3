using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi {
    public abstract class StoneOwned : Stone {
        public StoneOwned(Tile t) : base(t) {

        }

        protected static void DrawStone(Brush stoneBrush, Graphics g, Size clip) {
            g.FillEllipse(stoneBrush, 1, 1, clip.Width - 2, clip.Height - 2);
        }

        public abstract Player Owner {
            get;
        }
    }

    class StoneBlue : StoneOwned {
        public StoneBlue(Tile t) : base(t) {

        }

        private static readonly Brush brush = new SolidBrush(Player.Blue.Color);

        public override void Draw(Graphics g, Size clip) {
            DrawStone(brush, g, clip);
        }

        public override Player Owner {
            get {
                return Player.Blue;
            }
        }
    }

    class StoneRed : StoneOwned {
        public StoneRed(Tile t) : base(t) {

        }

        private static readonly Brush brush = new SolidBrush(Player.Red.Color);

        public override void Draw(Graphics g, Size clip) {
            DrawStone(brush, g, clip);
        }

        public override Player Owner {
            get {
                return Player.Red;
            }
        }
    }
}
