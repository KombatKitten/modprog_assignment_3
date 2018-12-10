using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Reversi {
    public abstract class StoneEmpty : Stone {
        public StoneEmpty(Tile t) : base(t) {

        }
    }

    public class StoneUnavailable : StoneEmpty {
        public StoneUnavailable(Tile t) : base(t) {

        }

        public override void Draw(Graphics g, Size clip) {}
    }

    public class StoneAvailable : StoneEmpty {
        public StoneAvailable(Tile t) : base(t) {

        }

        public override void Draw(Graphics g, Size clip) {
            const float MARGIN = .7f;
            const float REVERSE_MARGIN = 1f - MARGIN;

            if (this.tile.GameBoard.ShowAvailabilityHelp) {
                g.DrawEllipse(Pens.Black, clip.Width * MARGIN / 2, clip.Height * MARGIN / 2, clip.Width * REVERSE_MARGIN, clip.Height * REVERSE_MARGIN);
            }
        }

        public override bool Available {
            get {
                return true;
            }
        }
    }
}
