using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi.AI {
    public static class TileState {
        public const byte Blue = 0;
        public const byte Red = 1;
        public const byte Empty = 2;
    }

    /// <summary>
    /// Highly optimized not-a-Control version of the <see cref="GameBoard"/> used for AI computations
    /// </summary>
    public struct HeadlessGameBoard {
        public const int BOARD_WIDTH = GameBoard.BOARD_HEIGHT;  //measured in tiles
        public const int BOARD_HEIGHT = GameBoard.BOARD_WIDTH;  //measured in tiles
        public const int BOARD_HALF_HEIGHT = BOARD_HEIGHT / 2;
        public const int TILE_SIZE = 2;    //the size of one tile in memory measured in bits

        public HeadlessGameBoard(bool redPlays) {
            this.firstTiles = 0;
            this.lastTiles = 0;
            this.redPlays = redPlays;
        }

        ulong firstTiles; //first 32 tiles
        ulong lastTiles;  //last 32 tiles
        bool redPlays;

        /// <summary>
        /// Returns the best move according to the AI and it's score
        /// </summary>
        /// <param name="maxDepth">How many moves into the future the AI will simulate</param>
        /// <param name="maxNewThreadDepth">How many moves into the future the AI will create new threads for simulation</param>
        public (int x, int y, int score) BestMove(int maxDepth, int maxNewThreadDepth) {
            
            throw new NotImplementedException();
        }

        public byte this[int x, int y] {
            get {
                if(y < BOARD_HALF_HEIGHT) {
                    return (byte)((this.firstTiles >> (x + BOARD_WIDTH * y) * TILE_SIZE) & 3);
                }
                else {
                    return (byte)((this.lastTiles >> (x + BOARD_WIDTH * (y % BOARD_HALF_HEIGHT)) * TILE_SIZE) & 3);
                }
            }
            set {
                if (y < BOARD_HALF_HEIGHT) {
                    int shift = (x + BOARD_WIDTH * y) * TILE_SIZE;
                    //first, set the bits that are to be modified to zero
                    this.firstTiles &= ulong.MaxValue - ((ulong)3 << shift);
                    //actually set the bits to the right value
                    this.firstTiles |= (ulong)value << shift;
                }
                else {
                    int shift = (x + BOARD_WIDTH * (y % BOARD_HALF_HEIGHT)) * TILE_SIZE;
                    this.lastTiles &= ulong.MaxValue - ((ulong)3 << shift);
                    this.lastTiles |= (ulong)value << shift;
                }
            }
        }

        public override string ToString() {
            string result = "";

            for(int y = 0; y < BOARD_WIDTH; y++) {
                if(y != 0) {
                    result += "\n";
                }
                for(int x = 0; x < BOARD_HEIGHT; x++) {
                    switch(this[x, y]) {
                        case TileState.Blue:
                            result += "O";
                            break;
                        case TileState.Red:
                            result += "X";
                            break;
                        default:
                            result += " ";
                            break;
                    }
                }
            }

            return result;
        }
    }
}
