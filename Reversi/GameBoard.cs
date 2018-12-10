using System;
using System.Drawing;
using System.Windows.Forms;

namespace Reversi {
    public class GameBoard : Control {
        const int BOARD_WIDTH = 8;
        const int BOARD_HEIGHT = 8;

        public GameBoard() {
            for (int x = 0; x < BOARD_WIDTH; x++) {
                for (int y = 0; y < BOARD_HEIGHT; y++) {
                    Tile tile = new Tile(this, x, y);

                    this.Controls.Add(tile);
                    this.tiles[x, y] = tile;
                }
            }

            int bottom = BOARD_WIDTH / 2;
            int right= BOARD_WIDTH / 2;
            int top = bottom - 1;
            int left = right - 1;

            this.tiles[top, left].Stone = Player.Blue.CreateStone(this.tiles[bottom, right]);
            this.tiles[top, right].Stone = Player.Red.CreateStone(this.tiles[bottom, right]);
            this.tiles[bottom, left].Stone = Player.Red.CreateStone(this.tiles[bottom, right]);
            this.tiles[bottom, right].Stone = Player.Blue.CreateStone(this.tiles[bottom, right]);

            this.Resize += (o, e) => LayoutTiles();

            this.UpdateTileAvailability();
        }

        readonly Tile[,] tiles = new Tile[BOARD_WIDTH, BOARD_HEIGHT];
        public bool ShowAvailabilityHelp = true;

        public Player CurrentPlayer { get; set; } = Player.Blue;

        public void LayoutTiles() {
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

        public void PlaceStone(int x, int y) {
            Tile tile = this.tiles[x, y];
            tile.Stone = CurrentPlayer.CreateStone(tile);

            this.ConvertStones(x, y, 0, 1, this.CurrentPlayer);
            this.ConvertStones(x, y, 0, -1, this.CurrentPlayer);
            this.ConvertStones(x, y, 1, 0, this.CurrentPlayer);
            this.ConvertStones(x, y, -1, 0, this.CurrentPlayer);

            if (this.CurrentPlayer == Player.Blue) {
                this.CurrentPlayer = Player.Red;
            }
            else {
                this.CurrentPlayer = Player.Blue;
            }

            this.UpdateTileAvailability();
        }

        public void ConvertStones(int placedX, int placedY, int incrementX, int incrementY, Player placer) {
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
        public int ChainLength(int startX, int startY, int incrementX, int incrementY, Player currentPlayer) {
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
                            Console.WriteLine($"chain at ({startX}, {startY}) with increment ({incrementX}, {incrementY}) ending at ({x}, {y})");
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

        public void UpdateTileAvailability() {
            
            for (int x = 0; x < BOARD_WIDTH; x++){
                for (int y = 0; y < BOARD_HEIGHT; y++) {
                    Tile tile = this.tiles[x, y];

                    if (this.RecheckTileAvailability(x, y)) {
                        tile.Stone = new StoneAvailable(tile);
                    }
                    else if(tile.Stone is StoneEmpty) {
                        tile.Stone = new StoneUnavailable(tile);
                    }
                }
            }

        }

        public bool RecheckTileAvailability(int tileX, int tileY) {
            if (!(this.tiles[tileX, tileY].Stone is StoneEmpty)) {
                return false;
            }
            else {
                return
                    this.ChainLength(tileX, tileY, 1, 0, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, -1, 0, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, 0, 1, this.CurrentPlayer) > 0 ||
                    this.ChainLength(tileX, tileY, 0, -1, this.CurrentPlayer) > 0;
            }

            bool hasOponentInBetween = false;

            for(int x = tileX + 1; x < this.tiles.GetLength(0); x++) {
                Stone s = this.tiles[x, tileY].Stone;
                if(s is StoneEmpty) {
                    break;
                }
                if(s is StoneOwned stonePlayer) {
                    if (stonePlayer.Owner == this.CurrentPlayer) {
                        if(hasOponentInBetween) {
                            return true;
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        hasOponentInBetween = true;
                    }
                }
            }
            hasOponentInBetween = false;

            for (int x = tileX - 1; x > 0; x--) {
                Stone s = this.tiles[x, tileY].Stone;
                if (s is StoneEmpty) {
                    break;
                }
                if (s is StoneOwned stonePlayer) {
                    if (stonePlayer.Owner == this.CurrentPlayer) {
                        if (hasOponentInBetween) {
                            return true;
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        hasOponentInBetween = true;
                    }
                }
            }
            hasOponentInBetween = false;

            for (int y = tileY + 1; y < this.tiles.GetLength(1); y++) {
                Stone s = this.tiles[tileX, y].Stone;
                if (s is StoneEmpty) {
                    break;
                }
                if (s is StoneOwned stonePlayer) {
                    if (stonePlayer.Owner == this.CurrentPlayer) {
                        if (hasOponentInBetween) {
                            return true;
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        hasOponentInBetween = true;
                    }
                }
            }
            hasOponentInBetween = false;

            for (int y = tileY - 1; y > 0; y--) {
                Stone s = this.tiles[tileX, y].Stone;
                if (s is StoneEmpty) {
                    break;
                }
                if (s is StoneOwned stonePlayer) {
                    if (stonePlayer.Owner == this.CurrentPlayer) {
                        if (hasOponentInBetween) {
                            return true;
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        hasOponentInBetween = true;
                    }
                }
            }

            return false;
        }

        public Tile this[int x, int y] {
            get {
                return this.tiles[x, y];
            }
        }

        public static int BoardWidth {
            get {
                return BOARD_WIDTH;
            }
        }

        public static int BoardHeight {
            get {
                return BOARD_HEIGHT;
            }
        }
    }
}
