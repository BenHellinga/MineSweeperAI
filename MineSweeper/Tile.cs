using HumanBenchmark;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineSweeper
{
    internal class Tile
    {

        // VARIABLES

        // static
        
        public static int size;

        public static Color tileColor = Color.FromArgb(255, 255, 255, 255);
        public static Color emptyColor = Color.FromArgb(255, 189, 189, 189);
        public static Color flagColor = Color.FromArgb(255, 255, 0, 0);
        public static List<Color> numberColors = new List<Color>()
            { Color.FromArgb(255, 0, 0, 255),
              Color.FromArgb(255, 0, 123, 0),
              Color.FromArgb(255, 255, 0, 0),
              Color.FromArgb(255, 0, 0, 123),
              Color.FromArgb(255, 123, 0, 0),
              Color.FromArgb(255, 0, 123, 123),
              Color.FromArgb(255, 0, 0, 0),
              Color.FromArgb(255, 123, 123, 123)};


        // not static

        public int x;
        public int y;

        public int num;

        public int minesLeft;
        public int unexploredNum;

        public Tile[] adjacentTiles = new Tile[8];

        public int status;

        public static Tile NULL;



        // CONSTRUCTORS

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;

            num = -1;
            minesLeft = -1;
            status = 0;
        }



        // PUBLIC METHODS

        public void read()
        {
            num = readNumber();
            minesLeft = num;

            if (num == 0)
            {
                Tile tile;

                for (int i = 0; i < 8; i++)
                {
                    tile = adjacentTiles[i];

                    tile.unexploredNum--;

                    if (tile.status == 1) continue;

                    if (tile.status == 0)
                    {
                        tile.status = 1;
                        tile.addTileToRead();
                    }
                }

                unexploredNum = 0;
                return;
            }

            if (num > 0)
            {
                if (status == 1)
                {
                    GamePlayer.updateNum++;
                    GamePlayer.update[GamePlayer.updateNum] = this;
                    status = 2;
                }

                return;
            }

            if (num == -2)
            {
                Tile tile;

                status = 6;

                for (int i = 0; i < 8; i++)
                {
                    tile = adjacentTiles[i];

                    if (tile.status == 3)
                    {
                        tile.minesLeft--;
                        tile.unexploredNum--;

                        if (tile.unexploredNum == 0)
                            tile.status = 4;
                    }
                }

                return;
            }

        }


        
        public void update()
        {
            Tile tile;

            status = 3;

            for (int i = 0; i < 8; i++)
            {
                tile = adjacentTiles[i];

                if (tile.status == 5 || tile.status == 6)
                {
                    minesLeft--;
                    unexploredNum--;
                    continue;
                }

                if (tile.status == 3 || tile.status == 4)
                {
                    tile.unexploredNum--;
                    unexploredNum--;
                }
            }
        }



        public void exploreAdjacent()
        {
            status = 4;

            //Console.WriteLine("middle clicking");
            CursorManager.click(x * Tile.size + GamePlayer.gameLocation.X + GamePlayer.CLICK_OFFSET_X,
                                y * Tile.size + GamePlayer.gameLocation.Y + GamePlayer.CLICK_OFFSET_Y, "middle");
            Thread.Sleep(GamePlayer.CLICK_DELAY);

            Tile tile;

            for (int i = 0; i < 8; i++)
            {
                tile = adjacentTiles[i];

                if (tile.status == 0)
                {
                    tile.status = 1;
                    tile.addTileToRead();
                }
            }
        }



        public void flagAdjacent()
        {
            status = 4;

            Tile tile;

            for (int i = 0; i < 8; i++)
            {
                tile = adjacentTiles[i];

                if (tile.status == 0)
                    tile.flagTile();
            }
        }



        public void considerSharedMines(ref bool change)
        {
            Tile tileA;
            Tile tileB;

            int count;

            int oxmin;
            int oxmax;
            int oymin;
            int oymax;

            for (int k = 0; k < 8; k++)
            {
                tileB = adjacentTiles[k];

                if (tileB.status != 3) continue;
                if (tileB.minesLeft != 1) continue;

                oxmin = Math.Max(x - 1, tileB.x - 1);
                oxmax = Math.Min(x + 1, tileB.x + 1);
                oymin = Math.Max(y - 1, tileB.y - 1);
                oymax = Math.Min(y + 1, tileB.y + 1);

                count = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (x + j >= oxmin && x + j <= oxmax &&
                            y + i >= oymin && y + i <= oymax)
                            continue;

                        if (i == 0 && j == 0) continue;

                        if (x + j < 0 || x + j >= GamePlayer.GAME_WIDTH ||
                            y + i < 0 || y + i >= GamePlayer.GAME_HEIGHT)
                            continue;

                        if (GamePlayer.board[x + j, y + i].status == 0 ||
                            GamePlayer.board[x + j, y + i].status == 5)
                            count++;
                    }
                }

                //Console.WriteLine(count + " " + minesLeft + " " + tileB.minesLeft);
                if (count > minesLeft - tileB.minesLeft) continue;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (x + j >= oxmin && x + j <= oxmax &&
                            y + i >= oymin && y + i <= oymax)
                            continue;

                        if (i == 0 && j == 0) continue;

                        tileA = adjacentTiles[(j + 1) + 3 * (i + 1) -
                                             ((j + 1) + 3 * (i + 1) > 4 ? 1 : 0)]; // subtract 1 cuz it skips over .this

                        if (tileA.status == 0)
                        {
                            tileA.flagTile();
                            change = true;
                        }
                    }
                }

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (tileB.x + j >= oxmin && tileB.x + j <= oxmax &&
                            tileB.y + i >= oymin && tileB.y + i <= oymax)
                            continue;

                        if (i == 0 && j == 0) continue;

                        tileA = tileB.adjacentTiles[(j + 1) + 3 * (i + 1) -
                                                   ((j + 1) + 3 * (i + 1) > 4 ? 1 : 0)]; // subtract 1 cuz it skips over .this

                        if (tileA.status == 0)
                        {
                            tileA.exploreTile();
                            change = true;
                        }
                    }
                }
                //Console.WriteLine();
            }
        }

        public void print()
        {
            Console.Write("   x: " + x + "   y: " + y);
            Console.Write("   number: " + num + "   unexplored: " + unexploredNum);
            Console.WriteLine("   mines left: " + minesLeft + "   status: " + status);
        }
        


        public int readNumber()
        {
            if (GamePlayer.reader.screenshot.GetPixel(x * size, y * size).Equals(tileColor))
            {
                for (int i = size / 3; i < 2 * size / 3; i++)
                    for (int j = 0; j < 8; j++)
                        if (GamePlayer.reader.screenshot.GetPixel(x * size + i, y * size + i).Equals(flagColor))
                            return -2;

                return -1;
            }

            for (int i = size / 4; i < 3 * size / 4; i++)
                for (int j = 0; j < 8; j++)
                    if (GamePlayer.reader.screenshot.GetPixel(x * size + i, y * size + i).Equals(numberColors[j]))
                        return j + 1;

            return 0;
        }



        public void initUnexploredAdj()
        {
            int k = -1;
            unexploredNum = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    k++;

                    if (x + j < 0 || x + j >= GamePlayer.GAME_WIDTH ||
                        y + i < 0 || y + i >= GamePlayer.GAME_HEIGHT)
                    {
                        adjacentTiles[k] = NULL;
                        continue;
                    }

                    unexploredNum++;
                    adjacentTiles[k] = GamePlayer.board[x + j, y + i];
                }
            }
        }



        public static void init()
        {
            NULL = new Tile(0, 0);
            NULL.status = -1;
        }



        public void addTileToRead()
        {
            GamePlayer.readNum++;
            GamePlayer.read[GamePlayer.readNum] = this;
        }



        public void exploreTile()
        {
            CursorManager.click(x * Tile.size + GamePlayer.gameLocation.X + GamePlayer.CLICK_OFFSET_X,
                                y * Tile.size + GamePlayer.gameLocation.Y + GamePlayer.CLICK_OFFSET_Y, "left");
            Thread.Sleep(GamePlayer.CLICK_DELAY);

            status = 1;
            addTileToRead();
        }



        public void flagTile()
        {
            CursorManager.click(x * Tile.size + GamePlayer.gameLocation.X + GamePlayer.CLICK_OFFSET_X,
                                y * Tile.size + GamePlayer.gameLocation.Y + GamePlayer.CLICK_OFFSET_Y, "right");
            Thread.Sleep(GamePlayer.CLICK_DELAY);

            status = 5;
            addTileToRead();
        }
    }
}
