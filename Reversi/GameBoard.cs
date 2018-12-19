using System;
using System.Drawing;
using System.Windows.Forms;
using Reversi.AI;

namespace Reversi {
    public delegate void StartTurnEvent(int redStones, int blueStones, Player nextPlayer);
    public delegate void EndGameEvent(int redStones, int blueStones);

    public class GameBoard : Control {
        //changing these values will make the gameboard bigger/smaller, but will also disable the AI.
        public const int BOARD_WIDTH = 8;
        public const int BOARD_HEIGHT = 8;

        public GameBoard() {
            //initialize tiles
            for (int x = 0; x < BOARD_WIDTH; x++) {
                for (int y = 0; y < BOARD_HEIGHT; y++) {
                    Tile tile = new Tile(this, x, y);

                    this.Controls.Add(tile);
                    this.tiles[x, y] = tile;
                }
            }

            //place first four stones
            int bottom = BOARD_WIDTH / 2;
            int right= BOARD_WIDTH / 2;
            int top = bottom - 1;
            int left = right - 1;

            this.tiles[top, left].Stone = Player.Blue.CreateStone(this.tiles[bottom, right]);
            this.tiles[top, right].Stone = Player.Red.CreateStone(this.tiles[bottom, right]);
            this.tiles[bottom, left].Stone = Player.Red.CreateStone(this.tiles[bottom, right]);
            this.tiles[bottom, right].Stone = Player.Blue.CreateStone(this.tiles[bottom, right]);

            this.Resize += (o, e) => {
                this.LayoutTiles();
                this.Invalidate(true);
            };

            this.UpdateTileAvailability();
        }

        readonly Tile[,] tiles = new Tile[BOARD_WIDTH, BOARD_HEIGHT];
        private bool showAvailabilityHelp = true;

        private bool SkippedPreviousTurn { get; set; }
        public Player CurrentPlayer { get; set; } = Player.Starter;
        public bool Finished { get; private set; } = false;

        public event StartTurnEvent OnTurnStart;
        public event EndGameEvent OnGameEnd;

        /// <summary>
        /// Sets the <see cref="Control.Location"/> and <see cref="Control.Size"/> of each tile on the <see cref="GameBoard"/>
        /// </summary>
        void LayoutTiles() {
            int offsetX = this.ClientSize.Width / BOARD_WIDTH;
            int offsetY = this.ClientSize.Height / BOARD_HEIGHT;

            int modOffsetX = this.ClientSize.Width % BOARD_WIDTH;
            int modOffsetY = this.ClientSize.Height % BOARD_HEIGHT;

            for (int x = 0; x < BOARD_WIDTH; x++) {
                for (int y = 0; y < BOARD_HEIGHT; y++) {
                    int extraX = x < modOffsetX ? 1 : 0;
                    int extraY = y < modOffsetY ? 1 : 0;

                    this.tiles[x, y].Location = new Point(x * offsetX + Math.Min(x, modOffsetX), y * offsetY + Math.Min(y, modOffsetY));
                    this.tiles[x, y].Size = new Size(offsetX + extraX, offsetY + extraY);
                }
            }
        }

        /// <summary>
        /// Places a stone at the given coordinates and updates the rest of the game
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void PlaceStone(int x, int y) {
            Tile tile = this.tiles[x, y];
            tile.Stone = this.CurrentPlayer.CreateStone(tile);

            this.ConvertStones(x, y, 0, 1, this.CurrentPlayer);
            this.ConvertStones(x, y, 0, -1, this.CurrentPlayer);
            this.ConvertStones(x, y, 1, 0, this.CurrentPlayer);
            this.ConvertStones(x, y, -1, 0, this.CurrentPlayer);
            this.ConvertStones(x, y, -1, -1, this.CurrentPlayer);
            this.ConvertStones(x, y, -1, 1, this.CurrentPlayer);
            this.ConvertStones(x, y, 1, 1, this.CurrentPlayer);
            this.ConvertStones(x, y, 1, -1, this.CurrentPlayer);

            this.NextTurn();
        }

        /// <summary>
        /// Converts stones when a stone is placed at (<paramref name="placedX"/>, <paramref name="placedY"/>)
        /// </summary>
        /// <param name="placedX">X coordinate where the stone is placed</param>
        /// <param name="placedY">Y coordinate where the stone is placed</param>
        /// <param name="incrementX">The X direction to convert</param>
        /// <param name="incrementY">The Y directino to convert</param>
        /// <param name="placer">the player that placed the stone</param>
        void ConvertStones(int placedX, int placedY, int incrementX, int incrementY, Player placer) {
            int chainLength = this.ChainLength(placedX, placedY, incrementX, incrementY, placer);

            int x = placedX;
            int y = placedY;

            for(int i = 0; i < chainLength; i++) {
                x += incrementX;
                y += incrementY;

                this.tiles[x, y].Stone = placer.CreateStone(this.tiles[x, y]);
            }
        }

        /// <summary>
        /// Calculates the length of a chain starting and ending with a stone of the current player, measured in oponent stones in between
        /// </summary>
        /// <param name="startX">Starting X coordinate of the chain</param>
        /// <param name="startY">Starting Y coordinate of the chain</param>
        /// <param name="incrementX">X direction of the chain, usually -1, 0 or 1</param>
        /// <param name="incrementY">Y direction of the chain, usually -1, 0 or 1</param>
        int ChainLength(int startX, int startY, int incrementX, int incrementY, Player currentPlayer) {
            bool hasOponentInBetween = false;
            int chainLength = 0;

            for ((int x, int y) = (startX + incrementX, startY + incrementY); x < BOARD_WIDTH && x >= 0 && y < BOARD_HEIGHT && y >= 0;  (x, y) = (x + incrementX, y + incrementY)) {
                Stone s = this.tiles[x, y].Stone;
                if (s is StoneEmpty) {
                    return 0;
                }
                else if (s is StoneOwned stonePlayer) {
                    if (stonePlayer.Owner == currentPlayer) {
                        if (hasOponentInBetween) {
                            return chainLength;
                        }
                        else {
                            return 0;
                        }
                    }
                    else {
                        hasOponentInBetween = true;
                    }
                }
                chainLength++;
            }

            return 0;
        }

        /// <summary>
        /// Updates the availability for placement of each tile and returns whether there is at least one available tile
        /// </summary>
        bool UpdateTileAvailability() {
            bool hasAvailableTile = false;
            
            for (int x = 0; x < BOARD_WIDTH; x++){
                for (int y = 0; y < BOARD_HEIGHT; y++) {
                    Tile tile = this.tiles[x, y];

                    if (this.RecheckTileAvailability(x, y)) {
                        hasAvailableTile = true;
                        tile.Stone = new StoneAvailable(tile);
                    }
                    else if(tile.Stone is StoneEmpty) {
                        tile.Stone = new StoneUnavailable(tile);
                    }
                }
            }

            return hasAvailableTile;
        }

        /// <summary>
        /// Calculates whether the tile at the given coordinates should be available
        /// </summary>
        bool RecheckTileAvailability(int tileX, int tileY) {
            if (!(this.tiles[tileX, tileY].Stone is StoneEmpty)) {
                return false;
            }
            else {
                return
                    this.ChainLength(tileX, tileY, 1, 0, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, -1, 0, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, 0, 1, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, 0, -1, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, 1, 1, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, -1, 1, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, -1, -1, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, 1, -1, this.CurrentPlayer) > 0;
            }
        }

        /// <summary>
        /// Checks whether the game should and if the playing player should be switched. Does so if needed
        /// </summary>
        public void NextTurn() {
            this.CurrentPlayer = this.CurrentPlayer.Oponent;

            int redStones = this.CountStones<StoneRed>();
            int blueStones = this.CountStones<StoneBlue>();

            if (!this.UpdateTileAvailability()) {
                if(this.SkippedPreviousTurn) {
                    this.EndGame(redStones, blueStones);
                }
                else {
                    this.SkippedPreviousTurn = true;
                    this.NextTurn();
                }
            }
            else {
                this.OnTurnStart?.Invoke(redStones, blueStones, this.CurrentPlayer);
                this.SkippedPreviousTurn = false;
            }
        }

        /// <summary>
        /// Ends the game
        /// </summary>
        /// <param name="redStones">The amount of red stones on the board</param>
        /// <param name="blueStones">The amount of blue stones on the board</param>
        void EndGame(int redStones, int blueStones) {
            this.Finished = true;
            this.OnGameEnd?.Invoke(redStones, blueStones);
        }

        /// <summary>
        /// Counts the amount of stones of the given type
        /// </summary>
        /// <typeparam name="S">Type to count</typeparam>
        public int CountStones<S>() where S: Stone {
            int count = 0;

            for(int x = 0; x < BOARD_WIDTH; x++) {
                for(int y = 0; y < BOARD_HEIGHT; y++) {
                    if(this.tiles[x, y].Stone is S) {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Generates a <see cref="HeadlessGameBoard"/> based on the current state
        /// </summary>
        public HeadlessGameBoard ToHeadless() {
            var result = new HeadlessGameBoard(this.CurrentPlayer == Player.Red ? TileState.Red : TileState.Blue);

            foreach(Tile tile in this.tiles) {
                byte state;

                switch(tile.Stone) {
                    case StoneRed _:
                        state = TileState.Red;
                        break;
                    case StoneBlue _:
                        state = TileState.Blue;
                        break;
                    default:
                        state = TileState.Empty;
                        break;
                }

                result[tile.X, tile.Y] = state;
            }

            return result;
        }

        public bool ShowAvailabilityHelp {
            get => this.showAvailabilityHelp;
            set {
                this.showAvailabilityHelp = value;

                //rerender the available tiles
                for (int x = 0; x < BOARD_WIDTH; x++) {
                    for (int y = 0; y < BOARD_HEIGHT; y++) {
                        Tile tile = this.tiles[x, y];

                        if (tile.Stone is StoneAvailable) {
                            tile.Invalidate(true);
                        }
                    }
                }
            }
        }
    }
}
