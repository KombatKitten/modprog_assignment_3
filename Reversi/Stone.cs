using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Reversi {
    public abstract partial class Stone {
        readonly protected Tile tile;

        public Stone(Tile tile) {
            this.tile = tile;
        }

        public abstract void Draw(Graphics g, Size clip);

        public virtual bool Available {
            get {
                return false;
            }
        }
    }
}
