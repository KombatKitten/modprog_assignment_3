using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
        //changing these values will break the AI.
        public const int BOARD_WIDTH = 8;  //measured in tiles
        public const int BOARD_HEIGHT = 8;  //measured in tiles
        public const int BOARD_HALF_HEIGHT = BOARD_HEIGHT / 2;
        public const int TILE_SIZE = 2;    //the size of one tile in memory measured in bits

        public HeadlessGameBoard(byte player) {
            this.firstTiles = 0;
            this.lastTiles = 0;
            this.player = player;
            this.skippedPreviousTurn = false;
        }

        ulong firstTiles; //first 32 tiles
        ulong lastTiles;  //last 32 tiles
        byte player;
        bool skippedPreviousTurn;

        /// <summary>
        /// Returns the best move according to the AI and it's score
        /// </summary>
        /// <param name="maxDepth">How many moves into the future the AI will simulate</param>
        /// <param name="maxNewThreadDepth">How many moves into the future the AI will create new threads for simulation</param>
        public (int x, int y, int score) BestMove(int maxDepth, int maxNewThreadDepth, byte maximizingPlayer) {
            if (maxDepth < 1) {
                //calculate score
                return (-1, -1, this.CalcScore(maximizingPlayer));
            }

            // warning: this method is highly optimized and thus quite unreadable. you'd better not touch it and asume it works...
            var self = this;

            bool hasOponentInBetween = false;
            bool startsWithOwnColor = false;

            int totalAvailableTiles = 0;
            ulong allAvailableTiles = 0;
            ulong horizontalAvailableTilesToRight = 0;
            ulong horizontalAvailableTilesToLeft = 0;
            ulong verticalAvailableTilesToBottom = 0;
            ulong verticalAvailableTilesToTop = 0;

            ulong diagonalAvailableTilesToBottomRight = 0;
            ulong diagonalAvailableTilesToTopLeft = 0;
            ulong diagonalAvailableTilesToBottomLeft = 0;
            ulong diagonalAvailableTilesToTopRight = 0;

            //cheks for available tiles in the given direction
            void CheckTileAvailability(int startX, int startY, int endX, int endY, int scanDirectionX, int scanDirectionY, ref ulong availability) {
                int minX = startX < endX ? startX : endX;
                int maxX = startX < endX ? endX : startX;

                int minY = startY < endY ? startY : endY;
                int maxY = startY < endY ? endY : startY;

                int incrementX = Math.Sign(endX - startX);
                int incrementY = Math.Sign(endY - startY);

                for (int x = startX, y = startY; x >= minX && x <= maxX && y >= minY && y <= maxY; x += incrementX, y += incrementY) {
                    hasOponentInBetween = false;
                    startsWithOwnColor = false;
                    
                    for (int x2 = x, y2 = y; x2 >= 0 && x2 < BOARD_WIDTH && y2 >= 0 && y2 < BOARD_HEIGHT; x2 += scanDirectionX, y2 += scanDirectionY) {
                        byte tileState = self[x2, y2];

                        if (tileState == self.player) {

                            startsWithOwnColor = true;
                            hasOponentInBetween = false;
                        }
                        else if (tileState == TileState.Empty) {
                            if (startsWithOwnColor && hasOponentInBetween) {
                                //found possible move

                                if (x2 > -1 && y2 > -1 && x2 < BOARD_WIDTH && y2 < BOARD_HEIGHT //check bounds
                                    && self[x2, y2] == TileState.Empty) /* check if tile is empty */ {

                                    ulong index = 1ul << x2 + y2 * BOARD_WIDTH;
                                    availability |= index;

                                    if ((allAvailableTiles & index) == 0) {
                                        allAvailableTiles |= index;
                                        totalAvailableTiles += 1;
                                    }
                                }
                            }
                            startsWithOwnColor = false;
                            hasOponentInBetween = false;
                        }
                        else {
                            //found oponent stone
                            hasOponentInBetween = true;
                        }
                    }
                }
            }

            CheckTileAvailability(0, 0, 0, BOARD_HEIGHT - 1, 1, 0, ref horizontalAvailableTilesToRight);
            CheckTileAvailability(BOARD_WIDTH - 1, 0, BOARD_WIDTH - 1, BOARD_HEIGHT - 1, -1, 0, ref horizontalAvailableTilesToLeft);
            CheckTileAvailability(0, 0, BOARD_WIDTH - 1, 0, 0, 1, ref verticalAvailableTilesToBottom);
            CheckTileAvailability(0, BOARD_HEIGHT - 1, BOARD_WIDTH - 1, BOARD_HEIGHT - 1, 0, -1, ref verticalAvailableTilesToTop);

            CheckTileAvailability(0, 0, BOARD_WIDTH - 1 - 2, 0, 1, 1, ref diagonalAvailableTilesToBottomRight);
            CheckTileAvailability(BOARD_WIDTH - 1, BOARD_HEIGHT - 1, 2, BOARD_HEIGHT - 1, -1, -1, ref diagonalAvailableTilesToTopLeft);
            CheckTileAvailability(2, 0, BOARD_WIDTH - 1, 0, -1, 1, ref diagonalAvailableTilesToBottomLeft);
            CheckTileAvailability(0, BOARD_HEIGHT - 1, BOARD_WIDTH - 1 - 2, BOARD_HEIGHT - 1, 1, -1, ref diagonalAvailableTilesToTopRight);

            //return results
            if (totalAvailableTiles < 1) {
                //no move possible, skip turn or end game
                if (this.skippedPreviousTurn) {
                    //the game has ended
                    var (red, blue) = this.CountStones();
                    var score = maximizingPlayer == TileState.Red ? red - blue : blue - red;

                    /// Tie gets a score of 0, winning <see cref="int.MaxValue"/>, losing negative <see cref="int.MaxValue"/>
                    return (-1, -1, Math.Sign(score) * int.MaxValue);
                }
                else {
                    HeadlessGameBoard copy = new HeadlessGameBoard() {
                        player = self.player == TileState.Red ? TileState.Blue : TileState.Red,
                        firstTiles = self.firstTiles,
                        lastTiles = self.lastTiles,
                        skippedPreviousTurn = true,
                    };

                    return copy.BestMove(maxDepth, maxNewThreadDepth, maximizingPlayer);
                }
            }
            else if(maxNewThreadDepth > 0) {
                Thread[] threads = new Thread[totalAvailableTiles];
                (int x, int y, int score)[] scores = new (int, int, int)[totalAvailableTiles];

                int nextThreadId = 0;

                for (int i = 0; i < BOARD_HEIGHT * BOARD_WIDTH; i++) {
                    if ((allAvailableTiles & (1ul << i)) != 0) {
                        int threadId = nextThreadId;

                        int iCopy = i;
                        int x = iCopy % BOARD_WIDTH;
                        int y = iCopy / BOARD_WIDTH;

                        threads[threadId] = new Thread(() => {
                            HeadlessGameBoard copy = new HeadlessGameBoard() {
                                player = self.player == TileState.Red ? TileState.Blue : TileState.Red,
                                firstTiles = self.firstTiles,
                                lastTiles = self.lastTiles,
                                skippedPreviousTurn = false,
                            };

                            if ((horizontalAvailableTilesToRight & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, 1, 0, self.player);
                            }
                            if ((horizontalAvailableTilesToLeft & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, -1, 0, self.player);
                            }
                            if ((verticalAvailableTilesToBottom & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, 0, 1, self.player);
                            }
                            if ((verticalAvailableTilesToTop & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, 0, -1, self.player);
                            }
                            if ((diagonalAvailableTilesToBottomRight & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, 1, 1, self.player);
                            }
                            if ((diagonalAvailableTilesToTopLeft & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, -1, -1, self.player);
                            }
                            if ((diagonalAvailableTilesToBottomLeft & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, -1, 1, self.player);
                            }
                            if ((diagonalAvailableTilesToTopRight & (1ul << iCopy)) != 0) {
                                copy.ReplaceStones(x, y, 1, -1, self.player);
                            }
                            
                            scores[threadId] = (x, y, copy.BestMove(maxDepth - 1, maxNewThreadDepth - 1, maximizingPlayer).score);
                        });
                        threads[threadId].Start();

                        nextThreadId++;
                    }
                }

                threads[0].Join();
                (int x, int y, int score) bestScore = scores[0];

                for(int i = 1; i < totalAvailableTiles; i++) {
                    threads[i].Join();
                    if((scores[i].score > bestScore.score) == (self.player == maximizingPlayer)) {
                        bestScore = scores[i];
                    }
                }
                return bestScore;
            }
            else {
                (int x, int y, int score)[] scores = new (int, int, int)[totalAvailableTiles];

                int index = 0;
                for (int i = 0; i < BOARD_HEIGHT * BOARD_WIDTH; i++) {
                    if ((allAvailableTiles & (1ul << i)) != 0) {

                        int x = i % BOARD_WIDTH;
                        int y = i / BOARD_WIDTH;
                        
                        HeadlessGameBoard copy = new HeadlessGameBoard() {
                            player = self.player == TileState.Red ? TileState.Blue : TileState.Red,
                            firstTiles = self.firstTiles,
                            lastTiles = self.lastTiles,
                            skippedPreviousTurn = false,
                        };

                        if ((horizontalAvailableTilesToRight & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, 1, 0, self.player);
                        }
                        if ((horizontalAvailableTilesToLeft & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, -1, 0, self.player);
                        }
                        if ((verticalAvailableTilesToBottom & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, 0, 1, self.player);
                        }
                        if ((verticalAvailableTilesToTop & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, 0, -1, self.player);
                        }
                        if ((diagonalAvailableTilesToBottomRight & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, 1, 1, self.player);
                        }
                        if ((diagonalAvailableTilesToTopLeft & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, -1, -1, self.player);
                        }
                        if ((diagonalAvailableTilesToBottomLeft & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, -1, 1, self.player);
                        }
                        if ((diagonalAvailableTilesToTopRight & (1ul << i)) != 0) {
                            copy.ReplaceStones(x, y, 1, -1, self.player);
                        }

                        scores[index++] = (x, y, copy.BestMove(maxDepth - 1, maxNewThreadDepth - 1, maximizingPlayer).score);
                    }
                }

                (int x, int y, int score) bestScore = scores[0];
                for (int i = 1; i < totalAvailableTiles; i++) {
                    if ((scores[i].score > bestScore.score) == (self.player == maximizingPlayer)) {
                        bestScore = scores[i];
                    }
                }
                return bestScore;
            }
        }

        private void ReplaceStones(int startX, int startY, int incrementX, int incrementY, byte targetTileState) {
            for (int x = startX, y = startY; this[x, y] != targetTileState; x += incrementX, y += incrementY) {
                this[x, y] = targetTileState;
            }
        }

        private (int red, int blue) CountStones() {

            int redStones = 0;
            int blueStones = 0;
            for (int x = 1; x < BOARD_WIDTH - 1; x++) {
                for (int y = 1; y < BOARD_HEIGHT - 1; y++) {
                    switch (this[x, y]) {
                        case TileState.Red:
                            redStones++;
                            break;
                        case TileState.Blue:
                            blueStones++;
                            break;
                    }
                }
            }

            return (redStones, blueStones);
        }

        /// <summary>
        /// Assigns a score to the current state of the game that indicates who is winning
        /// </summary>
        /// <param name="maximizingPlayer"></param>
        /// <returns></returns>
        private int CalcScore(byte maximizingPlayer) {
            //this is where normally a neural network would come in to play, but that's far beyond the scope of this project

            const int DEFAULT_SCORE = 2;
            const int EDGE_SCORE = 3;
            const int CORNER_SCORE = 4;
            const int TURN_ENDER_SCORE = 3;

            int redScore = 0;
            int blueScore = 0;

            for (int x = 1; x < BOARD_WIDTH - 1; x++) {
                for (int y = 1; y < BOARD_HEIGHT - 1; y++) {
                    switch (this[x, y]) {
                        case TileState.Red:
                            redScore += DEFAULT_SCORE;
                            break;
                        case TileState.Blue:
                            blueScore += DEFAULT_SCORE;
                            break;
                    }
                }
                //evaluate tiles at the left and right edge
                switch (this[x, 0]) {
                    case TileState.Red:
                        redScore += EDGE_SCORE;
                        break;
                    case TileState.Blue:
                        blueScore += EDGE_SCORE;
                        break;
                }

                switch (this[x, BOARD_HEIGHT - 1]) {
                    case TileState.Red:
                        redScore += EDGE_SCORE;
                        break;
                    case TileState.Blue:
                        blueScore += EDGE_SCORE;
                        break;
                }
            }
            //evaluate tiles at the top and bottom edge
            for (int y = 1; y < BOARD_HEIGHT - 1; y++) {
                switch (this[0, y]) {
                    case TileState.Red:
                        redScore += EDGE_SCORE;
                        break;
                    case TileState.Blue:
                        blueScore += EDGE_SCORE;
                        break;
                }

                switch (this[BOARD_WIDTH - 1, y]) {
                    case TileState.Red:
                        redScore += EDGE_SCORE;
                        break;
                    case TileState.Blue:
                        blueScore += EDGE_SCORE;
                        break;
                }
            }

            //evaluate tiles at the corners
            switch (this[0, 0]) {
                case TileState.Red:
                    redScore += CORNER_SCORE;
                    break;
                case TileState.Blue:
                    blueScore += CORNER_SCORE;
                    break;
            }
            switch (this[0, BOARD_HEIGHT - 1]) {
                case TileState.Red:
                    redScore += CORNER_SCORE;
                    break;
                case TileState.Blue:
                    blueScore += CORNER_SCORE;
                    break;
            }
            switch (this[BOARD_WIDTH - 1, 0]) {
                case TileState.Red:
                    redScore += CORNER_SCORE;
                    break;
                case TileState.Blue:
                    blueScore += CORNER_SCORE;
                    break;
            }
            switch (this[BOARD_WIDTH - 1, BOARD_HEIGHT - 1]) {
                case TileState.Red:
                    redScore += CORNER_SCORE;
                    break;
                case TileState.Blue:
                    blueScore += CORNER_SCORE;
                    break;
            }

            if(this.player == TileState.Red) {
                redScore += TURN_ENDER_SCORE;
            }
            else {
                blueScore += TURN_ENDER_SCORE;
            }

            return maximizingPlayer == TileState.Red ? redScore - blueScore : blueScore - redScore;
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
