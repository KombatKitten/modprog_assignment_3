using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reversi.AI;
using System.Threading;

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
            this.autoPlayer.Click += (s, e) => this.AutoPlay();

            this.AIModes.Items.AddRange(new AIMode[]{
                new AIMode() { Text = "No AI" },
                new AIMode() { Text = "Blue vs AI", RedAI = true },
                new AIMode() { Text = "AI vs Red", BlueAI = true },
                new AIMode() { Text = "AI vs AI", BlueAI = true, RedAI = true }
            });
            this.AIModes.SelectedIndex = 0;

            this.AIModes.SelectedValueChanged += (s, e) => {
                AIMode selectedMode = (AIMode)(this.AIModes.SelectedItem);
                this.BlueAI = selectedMode.BlueAI;
                this.RedAI = selectedMode.RedAI;

                this.CheckAutoPlay();
            };

            this.Controls.AddRange(new Control[]{
                this.newGame,
                this.toggleHelp,
                this.blueScore,
                this.redScore,
                this.currentTurn,
            });

            if(GameBoard.BOARD_HEIGHT == HeadlessGameBoard.BOARD_HEIGHT && GameBoard.BOARD_WIDTH == HeadlessGameBoard.BOARD_WIDTH) {
                this.Controls.Add(this.autoPlayer);
                this.Controls.Add(this.AIModes);
            }

            this.ResetGame();
        }

        private void OnTurnStart(int redStones, int blueStones, Player nextPlayer) {
            this.redScore.Stones = redStones;
            this.blueScore.Stones = blueStones;
            this.currentTurn.Text = nextPlayer.Name + " is to put his";

            this.CheckAutoPlay();
        }

        private void CheckAutoPlay() {
            const int PLAY_DELAY = 200;

            var self = this;

            Console.WriteLine("checking auto play");

            if ((this.gameBoard.CurrentPlayer == Player.Red && this.RedAI) || (this.gameBoard.CurrentPlayer == Player.Blue && this.BlueAI)) {
                Console.WriteLine("setting up auto play");
                var timer = new System.Windows.Forms.Timer {
                    Interval = PLAY_DELAY,
                    Enabled = true,
                };
                timer.Tick += (s2, e2) => {
                    Console.WriteLine("starting auto play");
                    timer.Enabled = false;
                    self.AutoPlay();
                };
            }
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
        private Button autoPlayer = new Button() {
            Text = "Auto Play"
        };
        private ComboBox AIModes = new ComboBox();

        public bool BlueAI { get; private set; } = false;
        public bool RedAI { get; private set; } = false;

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

            this.CheckAutoPlay();
        }

        private void OnGameEnd(int redStones, int blueStones) {
            this.blueScore.Stones = blueStones;
            this.redScore.Stones = redStones;

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

            float tileSize = Math.Min((float)maxWidth / GameBoard.BOARD_WIDTH,(float)maxHeight / GameBoard.BOARD_HEIGHT);

            Size gameBoardSize = new Size(
                (int)(tileSize * GameBoard.BOARD_WIDTH),
                (int)(tileSize * GameBoard.BOARD_HEIGHT)
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
            
            this.AIModes.Location =new Point(this.ClientSize.Width - this.AIModes.Width, 0);
        }

        public void AutoPlay() {
            if (GameBoard.BOARD_HEIGHT != HeadlessGameBoard.BOARD_HEIGHT || GameBoard.BOARD_WIDTH != HeadlessGameBoard.BOARD_WIDTH) {
                MessageBox.Show("Can't use AI in board size other then 8x8");
                return;
            }

            const int MAX_NEW_THREAD_DEPTH = 1;
            const int MAX_CALCULATIONS_DEPTH = 6;

            byte maximizingPlayer = this.gameBoard.CurrentPlayer == Player.Red ? TileState.Red : TileState.Blue;
            
            var (x, y, score) = this.gameBoard.ToHeadless().BestMove(MAX_CALCULATIONS_DEPTH, MAX_NEW_THREAD_DEPTH, maximizingPlayer);

            if(x < 0 || y < 0) {
                Console.WriteLine("No move found!");
                Console.WriteLine($"x: {x}, y: {y}, score: {score}");
            }
            else {
                this.gameBoard.PlaceStone(x, y);
            }
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

    public class AIMode {
        public bool BlueAI { get; set; } = false;
        public bool RedAI { get; set; } = false;
        public string Text { get; set; } = "";

        public override string ToString() => this.Text;
    }
}
