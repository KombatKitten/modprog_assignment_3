using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reversi {
    public partial class MainScreen : Form {
        const int GAME_BOARD_OFFSET_TOP = 150;

        public MainScreen() {
            

            InitializeComponent();

            Resize += OnMainScreenResize;

            this.Size = new Size(500, 600);
            this.Text = "Reversi";

            this.Controls.Add(this.gameBoard);
        }

        private void OnMainScreenResize(object sender, EventArgs e) {
            int maxWidth = this.ClientRectangle.Width;
            int maxHeight = this.ClientRectangle.Height - GAME_BOARD_OFFSET_TOP;

            float tileSize = Math.Min((float)maxWidth / GameBoard.BoardWidth,(float)maxHeight / GameBoard.BoardHeight);

            Size gameBoardSize = new Size(
                (int)(tileSize * GameBoard.BoardWidth),
                (int)(tileSize * GameBoard.BoardHeight)
            );

            this.gameBoard.Size = gameBoardSize;

            this.gameBoard.Location = new Point(
                (maxWidth - gameBoardSize.Width) / 2,
                (maxHeight - gameBoardSize.Height) / 2 + GAME_BOARD_OFFSET_TOP
            );
        }

        GameBoard gameBoard = new GameBoard() {
            Location = new Point(0, GAME_BOARD_OFFSET_TOP),
        };
    }
}
