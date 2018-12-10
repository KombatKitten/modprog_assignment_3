using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi {
    public abstract class Player {
        public static PlayerBlue Blue;
        public static PlayerRed Red;

        static Player() {
            PlayerRed.Init();
            PlayerBlue.Init();
        }

        public abstract Stone CreateStone(Tile tile);

        public class PlayerBlue : Player {
            private PlayerBlue() {

            }

            public override Stone CreateStone(Tile tile) {
                return new StoneBlue(tile);
            }

            /// <summary>
            /// Sets <see cref="Player.Blue"/> equal to an instance of this class. Don't call this method since it's already automatically called when <see cref="Player"/> is first referenced
            /// </summary>
            internal static void Init() {
                Player.Blue = new PlayerBlue();
            }
        }

        public class PlayerRed : Player {
            private PlayerRed() {

            }

            public override Stone CreateStone(Tile tile) {
                return new StoneRed(tile);
            }

            /// <summary>
            /// Sets <see cref="Player.Red"/> equal to an instance of this class. Don't call this method since it's already automatically called when <see cref="Player"/> is first referenced
            /// </summary>
            internal static void Init() {
                Player.Red = new PlayerRed();
            }
        }
    }
}
