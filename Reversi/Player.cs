using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Reversi {
    public abstract class Player {
        public static PlayerBlue Blue;
        public static PlayerRed Red;
        public static Player Starter;

        static Player() {
            PlayerRed.Init();
            PlayerBlue.Init();

            Starter = Blue;
        }

        public abstract Stone CreateStone(Tile tile);
        public abstract Player Oponent {
            get;
        }

        public abstract string Name {
            get;
        }

        public abstract Color Color {
            get;
        }

        public class PlayerBlue : Player {
            private PlayerBlue() {

            }

            public override Stone CreateStone(Tile tile) {
                return new StoneBlue(tile);
            }

            public override Player Oponent {
                get => Player.Red;
            }

            public override string Name {
                get => "Blue";
            }

            public override Color Color {
                get => Color.Blue;
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

            public override Player Oponent {
                get => Player.Blue;
            }

            public override string Name {
                get => "Red";
            }

            public override Color Color {
                get => Color.Red;
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
