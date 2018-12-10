using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Reversi {
    public class Tile : Control {
        public GameBoard GameBoard { get; }
        Stone stone;
        readonly int x;
        readonly int y;


        public Tile(GameBoard gameBoard, int x, int y) {
            this.GameBoard = gameBoard;
            this.stone = new StoneUnavailable(this);

            this.x = x;
            this.y = y;
            
            this.Click += OnTileClick;
        }

        private void OnTileClick(object sender, EventArgs e) {
            if(this.stone.Available) {
                this.GameBoard.PlaceStone(x, y);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            int width = this.Width;
            int height = this.Height;

            if(this.x == GameBoard.BoardWidth - 1) {
                width -= 1;
            }
            if (this.y == GameBoard.BoardHeight - 1) {
                height -= 1;
            }

            g.DrawRectangle(Pens.Black, 0, 0, width, height);

            this.Stone.Draw(g, this.Size);
        }

        public Stone Stone {
            get {
                return this.stone;
            }
            set {
                this.stone = value;
                this.Invalidate();
            }
        }
    }
}
