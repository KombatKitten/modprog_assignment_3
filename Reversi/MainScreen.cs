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
        const int RED_START_STONES = 2;
        const int BLUE_START_STONES = 2;

        public MainScreen() {
            this.InitializeComponent();

            Resize += (s, e) => this.LayoutControls();

            this.Size = new Size(500, 600);
            this.Text = "Reversi";

            this.newGame.Click += (s, e) => this.ResetGame();
            this.toggleHelp.Click += (s, e) => this.gameBoard.ShowAvailabilityHelp = !this.gameBoard.ShowAvailabilityHelp;

            this.Controls.AddRange(new Control[]{
                this.newGame,
                this.toggleHelp,
                this.blueScore,
                this.redScore,
                this.currentTurn,
            });

            this.ResetGame();
        }

        private void OnTurnStart(int redStones, int blueStones, Player nextPlayer) {
            this.redScore.Stones = redStones;
            this.blueScore.Stones = blueStones;
            this.currentTurn.Text = nextPlayer.Name + " is to put his";
        }

        private Button newGame = new Button() {
            Text = "New Game",
        };
        private Button toggleHelp = new Button() {
            Text = "Toggle Help",
        };
        private InterimScore redScore = new InterimScore(RED_START_STONES, Color.Red);
        private InterimScore blueScore = new InterimScore(BLUE_START_STONES, Color.Blue);
        private Label currentTurn = new Label();

        GameBoard gameBoard = new GameBoard() {
            Location = new Point(0, GAME_BOARD_OFFSET_TOP),
        };

        public void ResetGame() {
            if(this.gameBoard != null) {
                this.Controls.Remove(this.gameBoard);
            }

            this.gameBoard = new GameBoard();

            this.gameBoard.OnTurnStart += this.OnTurnStart;
            this.gameBoard.OnGameEnd += this.OnGameEnd;
            this.Controls.Add(this.gameBoard);

            this.redScore.Stones = RED_START_STONES;
            this.blueScore.Stones = BLUE_START_STONES;

            this.currentTurn.Text = Player.Starter.Name + " is to put his";

            this.LayoutControls();
        }

        private void OnGameEnd(int redStones, int blueStones) {
            if(redStones > blueStones) {
                this.currentTurn.Text = Player.Red.Name + " won the game";
            }
            else if (redStones < blueStones) {
                this.currentTurn.Text = Player.Blue.Name + " won the game";
            }
            else {
                this.currentTurn.Text = "There is a tie!";
            }
        }

        private void LayoutControls() {
            //move and resize gameboard
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

            //set button locations
            this.newGame.Location = new Point(
                this.newGame.Width / -2 + this.ClientRectangle.Width / 4,
                this.newGame.Height / -2 + GAME_BOARD_OFFSET_TOP / 4
            );

            this.toggleHelp.Location = new Point(
                this.toggleHelp.Width / -2 + this.ClientRectangle.Width * 3 / 4,
                this.toggleHelp.Height / -2 + GAME_BOARD_OFFSET_TOP / 4
            );

            //set interim score locations
            this.redScore.Location = new Point(
                this.newGame.Width / -2 + this.ClientRectangle.Width / 4,
                this.newGame.Height / -2 + GAME_BOARD_OFFSET_TOP * 2 / 3
            );

            this.blueScore.Location = new Point(
                this.toggleHelp.Width / -2 + this.ClientRectangle.Width * 3 / 4,
                this.toggleHelp.Height / -2 + GAME_BOARD_OFFSET_TOP * 2 / 3
            );

            this.currentTurn.Location = new Point(this.ClientSize.Width / 2 - this.currentTurn.Width / 2,
                GAME_BOARD_OFFSET_TOP / 2 - this.currentTurn.Height / 2);
        }
    }

    public class InterimScore : Control {
        public InterimScore(int stones, Color color) {
            this.playerColor = color;
            this.circleBrush = new SolidBrush(color);

            this.Controls.Add(this.score);
            this.Paint += this.OnScorePaint;

            this.Width = 200;
            this.Height = 50;

            this.score.Width = this.Width - this.Height;
            this.score.Location = new Point(this.Height + 10, this.Height / 2 - this.score.Height / 2);
            this.score.ForeColor = color;

            this.Stones = stones;
        }

        private void OnScorePaint(object sender, PaintEventArgs e) {
            e.Graphics.FillEllipse(this.circleBrush, 0, 0, this.Height, this.Height);
        }

        private Label score = new Label();
        private readonly Brush circleBrush;
        private readonly Color playerColor;

        public int Stones {
            set {
                this.score.Text = value + " stones";
                this.score.Invalidate();
            }
        }
    }
}
