using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;


namespace Othello
{
    public partial class Form1 : Form
    {
        //定数の定義：const
        const int MATRIX_NUM = 10;
        
        //ボードを表す配列
        int[,] board;

        //石の色
        const int BLACK = 1;
        const int WHITE = 2;
        const int EMPTY = 0;

        //プレイヤー
        int player;
        int rival;

        //石の数の表示
        int white_count;
        int black_count;

        //終了メッセージ
        string message;

        //オセロの戦略用
        Random rnd = new Random();

        //コンストラクタ
        public Form1()
        {
            InitializeComponent();
            //Text = "Reversi";
            ClientSize = new Size(900, 780);
            BackColor = Color.Green;
            Init();
        }

        private void Init()
        {

            board = new int[MATRIX_NUM, MATRIX_NUM];
            board[MATRIX_NUM / 2 - 1, MATRIX_NUM / 2 - 1] = WHITE;
            board[MATRIX_NUM / 2 - 1, MATRIX_NUM / 2] = BLACK;
            board[MATRIX_NUM / 2, MATRIX_NUM / 2 - 1] = BLACK;
            board[MATRIX_NUM / 2, MATRIX_NUM / 2] = WHITE;

            //盤面が正しいか出力ウィンドウで確認できる
            /*
            for (int y=0; y < MATRIX_NUM; y++)
            {
                for (int x=0; x < MATRIX_NUM; x++)
                {
                    
                    Debug.Write(board[x, y]);
                    Debug.Write("");
                }
                Debug.WriteLine("");
            }
            */
            //先手は黒
            player = BLACK;
            rival = WHITE;

            message = "";

            CountStones();




        }
        

        private void CountStones()
        {
            black_count = 0;
            white_count = 0;
            for (int y = 0; y < MATRIX_NUM; y++)
            {
                for (int x = 0; x < MATRIX_NUM; x++)
                {
                    if (board[x,y] == BLACK)
                    {
                        black_count++;
                    }
                    else if (board[x,y] == WHITE)
                    {
                        white_count++;
                    }
                }
            }
        }

        //GUIの作成
        /*protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            for (int i = 0; i <= MATRIX_NUM; i++)
            {
                //マス目の描写,縦横9マスずつ引く
                //(x1,y1)から(x2,y2)に幅1の黒い線を引く
                e.Graphics.DrawLine(Pens.Black, 10, 30 * i + 10, 250, 30 * i + 10);
                e.Graphics.DrawLine(Pens.Black, 30 * i + 10, 10, 30 * i + 10, 250);


            }
        }
        */

        //onPaintと同じ．FormのイベントでPaintにDrawと入力した
        private void Draw(object sender, PaintEventArgs e)
        {
            for (int i = 0; i <= MATRIX_NUM; i++)
            {
                //マス目の描写
                e.Graphics.DrawLine(Pens.Black, 30, 90 * i + 30, 750, 90 * i + 30);
                e.Graphics.DrawLine(Pens.Black, 90 * i + 30, 30, 90 * i + 30, 750);
            }


            //マス目をすべて確認して，1のものに黒，2のものに白の石を描く
            //ここでGUIとCUIを結び付けている
            for (int y = 0; y < MATRIX_NUM; y++)
            {
                for (int x = 0; x < MATRIX_NUM; x++)
                {
                    if (board[x, y] == BLACK)
                    {
                        e.Graphics.FillEllipse(Brushes.Black, 90 * x + 31, 90 * y + 31, 88, 88);
                    }
                    else if (board[x, y] == WHITE)
                    {
                        e.Graphics.FillEllipse(Brushes.White, 90 * x + 31, 90 * y + 31, 88, 88);
                    }
                }
            }

            //誰のターンなのかを表示する
            e.Graphics.DrawRectangle(Pens.Black, 780, 660, 90, 90);
            if (player == BLACK)
            {
                e.Graphics.FillEllipse(Brushes.Black, 781, 661, 88, 88);
            }
            if (player == WHITE)
            {
                e.Graphics.FillEllipse(Brushes.White, 781, 661, 88, 88);
            }

            //描写した文字列を中央寄せする
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString("Turn", Font, Brushes.White, new Rectangle(750, 690, 150, 90), sf);

            //どちらが何個の石を持っているか表示する
            e.Graphics.FillEllipse(Brushes.Black, 781, 31, 88, 88);
            e.Graphics.DrawString(black_count.ToString(), Font, Brushes.Black, new Rectangle(750, 120, 150, 90),sf);
            e.Graphics.FillEllipse(Brushes.White, 781, 211, 88, 88);
            e.Graphics.DrawString(white_count.ToString(), Font, Brushes.White, new Rectangle(750, 300, 150, 90),sf);


            if (message != "")

            {

                var r = new Rectangle(60, 360, 660, 60);
                e.Graphics.FillRectangle(Brushes.White, r);
                e.Graphics.DrawRectangle(Pens.Red, r);
                e.Graphics.DrawString(message, Font, Brushes.Black, r, sf);



            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            //GAMEOVERで再開する
            if (message != "")
            {
                Init();
                Refresh();
            }
            else
            {
                //(0,0)から(7,7)のどこをタッチしたのか取得する
                int x = (e.X - 30) / 90;
                int y = (e.Y - 30) / 90;

                if (PutStone(x,y) > 0)
                {
                    if (Change() == 1)
                    {
                        do
                        {
                            Refresh();
                            MonteCarloMethodThink();
                        }
                        while (Change() == 2);
                    }
                    Refresh();
                }


            }
        }

        private bool Check(int x,int y,int player)
        {
            return 0 <= x && x < MATRIX_NUM && 0 <= y && y < MATRIX_NUM && board[x, y] == player;
        }

        private int CountStone(int x,int y,int dx,int dy)
        {
            int x1 = x + dx;
            int y1 = y + dy;
            int stone = 0;
            while (Check(x1, y1, rival))
            {
                x1 += dx;
                y1 += dy;
                stone++;
            }
            //自分の石で挟んでいるか確認
            if (Check(x1, y1, player))
            {
                return stone;
            }
            //はさんでいなくて，最後の石が空白の場合は0を返す
            else
            {
                return 0;
            }
        }

        private int PutStone(int x,int y,int dx,int dy)
        {
            int stone = CountStone(x, y, dx, dy);
            for (int i = 1;i <= stone; i++)
            {
                board[x + dx * i, y + dy * i] = player;
            }
            return stone;
        }

        private int PutStone(int x,int y)
        {
            int stone = 0;
            if (Check(x, y, EMPTY))
            {
                stone += PutStone(x, y, 1, 0);
                stone += PutStone(x, y, -1, 0);
                stone += PutStone(x, y, 0, 1);
                stone += PutStone(x, y, 0, -1);
                stone += PutStone(x, y, 1, 1);
                stone += PutStone(x, y, 1, -1);
                stone += PutStone(x, y, -1, 1);
                stone += PutStone(x, y, -1, -1);
                if (stone > 0)
                {
                    //自分が置いた場所を自分の石の色にする
                    board[x, y] = player;
                    //自分が置いた石の分をを変数に追加する
                    stone++;
                    CountStones();
                }
            }
            return stone;
        }

        private int CountStone(int x, int y)

        {

            int stone = 0;

            if (Check(x, y, 0))
            {
                stone += CountStone(x,y,1,0);
                stone += CountStone(x,y,-1,0);  
                stone += CountStone(x,y,0,1);   
                stone += CountStone(x,y,0,-1);  
                stone += CountStone(x,y,1,1);   
                stone += CountStone(x,y,1,-1);  
                stone += CountStone(x,y,-1,1);  
                stone += CountStone(x,y,-1,-1); 
            }
            return stone;
        }

        private bool CanPut()
        {
            for (int y = 0; y < MATRIX_NUM; y++)
            {
                for (int x = 0; x<MATRIX_NUM; x++)
                {
                    if (CountStone(x,y) > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int Change()
        {
            int p = player;
            player = rival;
            rival = p;

            if (CanPut())
            {
                return 1;
            }

            rival = player;
            player = p;

            if (CanPut())
            {
                return 2;
            }

            if (black_count > white_count)
            {
                message = "Black Wins!";
            }
            else if (black_count < white_count)
            {
                message = "White Wins!";
            }
            else
            {
                message = "Draw!";
            }

            return 3;

        }

        private void RandomThink()
        {
            int x;
            int y;
            do
            {
                x = rnd.Next(MATRIX_NUM);
                y = rnd.Next(MATRIX_NUM);
            }
            while (PutStone(x, y) == 0);
        }

        private void MonteCarloMethodThink()

        {
            int p = player, r = rival;
            int[,] win = new int[MATRIX_NUM, MATRIX_NUM];
            int[,] bak = new int[MATRIX_NUM, MATRIX_NUM];        
            Array.Copy(board, bak, MATRIX_NUM * MATRIX_NUM);
            int max = 0;
            int tx = 0;
            int ty = 0;
            for (int i = 1; i <= 1000; i++)
            {
                int x1 = -1;
                int y1 = -1;
                //無限ループ
                //このfor文で石を適当な場所に置く
                //
                for (; ; )
                {
                    int x2 = rnd.Next(MATRIX_NUM);
                    int y2 = rnd.Next(MATRIX_NUM);
                    if (PutStone(x2, y2) > 0)
                    {
                        if (x1 < 0)
                        {
                            x1 = x2;
                            y1 = y2;
                        }
                        if (Change() == 3)
                        {
                            break;
                        }
                    }
                }
                //石を置いたときに自分（コンピュータ）の石が相手の石より少なくなるとき，選んだマス目の評価値を1足す
                //評価値が一番高いものを選ぶ
                if ((p == 1 && black_count > white_count) || (p == 2 && black_count < white_count))
                {
                    win[x1, y1]++;                
                    if (max < win[x1, y1])
                    {
                        max = win[x1, y1];
                        tx = x1;
                        ty = y1;
                    }
                }
                Array.Copy(bak, board, MATRIX_NUM * MATRIX_NUM);
                player = p;
                rival = r;
            }
            message = "";
            CountStones();
            if (max > 0)
            {
                PutStone(tx, ty);
            }
            else
            {
                RandomThink();
            }
        }


    }
}
