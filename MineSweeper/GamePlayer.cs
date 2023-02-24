using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using HumanBenchmark;
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using Microsoft.Win32.SafeHandles;

namespace MineSweeper
{
    internal class GamePlayer
    {
        // CONSTANTS

        public const int SCAN_SIZE = 30;

        public const int GAME_WIDTH = 30;
        public const int GAME_HEIGHT = 16;

        public const int DELAY_AFTER_CLICK = 100;
        public const int CLICK_DELAY = 0;
        public const int SCREENSHOT_DELAY = 50;

        public const int BUFFER_SIZE = GAME_WIDTH * GAME_HEIGHT / 2;

        public const int CLICK_OFFSET_X = 10;
        public const int CLICK_OFFSET_Y = 10;

        public const int RANDOM_MIN_EXPLORED = 2;
        public const bool PAUSE_BEFORE_RANDOM = false;

        // VARIABLES


        public static ScreenReader reader;

        public static Point gameLocation;

        public static Tile[,] board;

        //public static Tile[] explore = new Tile[BUFFER_SIZE];
        public static Tile[] read = new Tile[BUFFER_SIZE];
        public static Tile[] update = new Tile[BUFFER_SIZE];
        public static List<Tile> active = new List<Tile>();

        //public static int exploreNum;
        public static int readNum;
        public static int updateNum;

        public static bool change;


        // MAIN

        static void Main()
        {

            initBoard();

            gameLocation = CursorManager.findColorNearCursor(Tile.tileColor, SCAN_SIZE, SCAN_SIZE);

            reader = new ScreenReader(gameLocation.X, gameLocation.Y, 100, 100);
            reader.captureScreenshot();
            findTileSize();

            reader = new ScreenReader(gameLocation.X, gameLocation.Y, GAME_WIDTH * Tile.size, GAME_HEIGHT * Tile.size);
            reader.captureScreenshot();

            CursorManager.click(gameLocation.X + Tile.size * GAME_WIDTH / 2 + GAME_WIDTH / 2,
                                    gameLocation.Y + Tile.size * GAME_HEIGHT / 2 + GAME_HEIGHT / 2, "left");
            Thread.Sleep(DELAY_AFTER_CLICK);

            read[0] = board[GAME_WIDTH / 2, GAME_HEIGHT / 2];
            read[0].status = 1;

            readNum = 0;
            updateNum = -1;

            int j = 400;
            while (j > 0)
            {
                if (CursorManager.cursorMoved())
                {
                    Console.WriteLine("Paused");
                    Console.ReadLine();
                }

                readNewTiles();
                updateNewTiles();

                if (active.Count == 0) break;

                change = false;
                flagTrivialTiles(ref change);
                finishMinelessTiles(ref change);

                if (!change)
                    considerOverlap(ref change);

                if (!change)
                    pickLowestProbabilty(ref change);

                removeFinishedTiles();

                if (!change) break;
                j--;
            }

            /*
            Console.WriteLine("ACTIVE");
            for (int i = 0; i < active.Count; i++)
                active[i].print();
            Console.WriteLine("READ");
            for (int i = 0; i <= readNum; i++)
                read[i].print();
            Console.WriteLine("UPDATE");
            for (int i = 0; i <= updateNum; i++)
                update[i].print();
            */

            Console.WriteLine("Finished");
            Console.ReadLine();
        }



        // PUBLIC METHODS

        public static void initBoard()
        {
            Tile.init();
            board = new Tile[GAME_WIDTH, GAME_HEIGHT];

            for (int x = 0; x < GAME_WIDTH; x++)
                for (int y = 0; y < GAME_HEIGHT; y++)
                    board[x, y] = new Tile(x, y);

            for (int x = 0; x < GAME_WIDTH; x++)
                for (int y = 0; y < GAME_HEIGHT; y++)
                    board[x, y].initUnexploredAdj();
        }


        public static void findTileSize()
        {
            Tile.size = 1;
            while (reader.screenshot.GetPixel(Tile.size, 10).Equals(Tile.tileColor)) Tile.size++;
            while (!reader.screenshot.GetPixel(Tile.size, 10).Equals(Tile.tileColor)) Tile.size++;
        }



        public static void print()
        {
            for (int y = 0; y < GAME_HEIGHT; y++)
            {
                for (int x = 0; x < GAME_WIDTH; x++)
                {
                    Console.Write((board[x, y].unexploredNum) + " ");
                }
                Console.WriteLine();
            }
        }



        public static void readNewTiles()
        {
            Thread.Sleep(SCREENSHOT_DELAY);
            reader.captureScreenshot();

            Tile tile;
            while (readNum >= 0)
            {
                tile = read[readNum];
                readNum--;
                tile.read();
            }
        }



        public static void updateNewTiles()
        {
            Tile tile;
            while (updateNum >= 0)
            {
                tile = update[updateNum];
                updateNum--;
                tile.update();

                active.Add(tile);
            }
        }



        public static void finishMinelessTiles(ref bool change)
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].minesLeft == 0)
                {
                    active[i].exploreAdjacent();
                    change = true;
                }
            }
        }



        public static void flagTrivialTiles(ref bool change)
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].minesLeft == active[i].unexploredNum)
                {
                    active[i].flagAdjacent();
                    change = true;
                }
            }
        }



        public static void considerOverlap(ref bool change)
        {
            for (int i = 0; i < active.Count; i++)
            {
                active[i].considerSharedMines(ref change);
            }
        }



        public static void pickLowestProbabilty(ref bool change)
        {
            Tile tile;
            Tile best = board[0, 0];

            int count;
            double sumProb = 0;
            double min = 1;


            for (int y = 0; y < GAME_HEIGHT; y++)
            {
                for (int x = 0; x < GAME_WIDTH; x++)
                {
                    if (board[x, y].status != 0) continue;

                    count = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        tile = board[x, y].adjacentTiles[i];

                        if (tile.status == 3)
                        {
                            sumProb += tile.minesLeft / (double)tile.unexploredNum;
                            count++;
                        }

                        //if (tile.status == 6) count++;

                    }

                    if (count < RANDOM_MIN_EXPLORED) continue;

                    sumProb /= count;

                    if (sumProb < min)
                    {
                        min = sumProb;
                        best = board[x, y];
                    }

                }
            }

            if (min == 1) return;

            Console.WriteLine("random guess -> x: " + best.x + "   y: " + best.y + " p: ~" + min + "%");
            if (PAUSE_BEFORE_RANDOM) Console.ReadLine();

            best.exploreTile();
            change = true;

        }



        public static void removeFinishedTiles()
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].status == 4)
                {
                    //Console.WriteLine("removing " + active[i].x + " " + active[i].y);
                    active.RemoveAt(i);
                    i--;
                }
            }
        }



        public static void markActiveTiles()
        {
            for (int i = 0; i < active.Count; i++)
                for (int j = 0; j < 10; j++)
                    for (int k = 0; k < 10; k++)
                        reader.screenshot.SetPixel(active[i].x * Tile.size + j, active[i].y * Tile.size + k, Color.FromArgb(255, 0, 0, 0));
        }


        

    }
}
