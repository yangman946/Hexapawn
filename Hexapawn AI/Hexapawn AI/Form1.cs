/* Hexapawn 2.0 by Clarence Yang 19/12/22 
 * Features:
 * - game engine: contains rules for legal moves, winning, etc.
 * - can play against other user or bot
 * - chess notation implemented that updates with player moves
 * - clean UI with legal move indications
 * - score tracker
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Hexapawn_AI
{
    public partial class Form1 : Form
    {
        (int, Button)[,] boardConfig;
        (int, Button)[,] newboardConfig = new (int, Button)[3,3];

        int currentPlayer = 0;
        int previousPlayer = 0;
        int round = 1;
        string saved = "";

        int moveNum = 0; // 2 moves: 1 for selecting your piece, 2 for selecting where to move
        int selectedPiece = 0;
        List<int> legalMove = new List<int>();

        int whiteScore = 0;
        int blackScore = 0;

        bool PlayingWithBot = false;
        bool training = false;

        Hexapawn H;

        public Form1()
        {
            InitializeComponent();

            boardConfig = new (int, Button)[,] {
                { (1, a3), (1, b3), (1, c3) },
                { (0, a2), (0, b2), (0, c2)},
                { (2, a1), (2, b1), (2, c1) }
            };

            H = new Hexapawn();
            H.init(training);

            
            reset();
        }

        int[,] strip()
        {
            int[,] stripped = new int[,]
            {
                { newboardConfig[0, 0].Item1, newboardConfig[0, 1].Item1, newboardConfig[0, 2].Item1 },
                { newboardConfig[1, 0].Item1, newboardConfig[1, 1].Item1, newboardConfig[1, 2].Item1 },
                { newboardConfig[2, 0].Item1, newboardConfig[2, 1].Item1, newboardConfig[2, 2].Item1 }
            };

            return stripped;

        }

        void moved()
        {

            previousPlayer = currentPlayer;
            moveNum = 0;
            selectedPiece = 0;
            legalMove.Clear();
            
            if (currentPlayer == 0) // white
            {
                round += 1;
                currentPlayer = 1;
                label10.ForeColor = SystemColors.Highlight;
                label9.ForeColor = SystemColors.ControlText;

                

            }
            else if (currentPlayer == 1)
            {
                currentPlayer = 0;
                label9.ForeColor = SystemColors.Highlight;
                label10.ForeColor = SystemColors.ControlText;
            }



            if (checkForMoves())
            {
                updateScreen(0, 0, false, true, true);
                win(previousPlayer);
                return;
            }

            if (PlayingWithBot && currentPlayer == 1)
            {
                // get selected piece
                // move to destination
                int[] result = H.getMove(strip());
                
                if (result[0] == 0 && result[1] == 0)
                {
                    //MessageBox.Show("not found");
                    int i = 0;
                    List<int[]> moves = new List<int[]>();
                    List<int> m = new List<int>();
                    // no moves found because outcome is new
                    foreach (var item in newboardConfig)
                    {
                        // error here, moves empty
                        if (item.Item1 == 1)
                        {
                            var r = legalMoves(i);
                            foreach (var move in r)
                            {
                                m = new List<int>();
                                m.Add(i);
                                m.Add(move);
                                moves.Add(m.ToArray());
                            }
                        }
                        i += 1;
                    }
                    result = H.newOutCome(strip(), moves.ToArray());
                    //MessageBox.Show(result[0] + " " + result[1]);
                }
                selectedPiece = result[0];
                move(result[1]);
            }
        }

        bool checkForMoves()
        {


            for (int i = 0; i < 9; i++)
            {
                if (occupied(i) == 0)
                {
                    if (legalMoves(i).Count > 0)
                    {

                        return false;
                    }
                }
            }
            return true;

        }

        void toggle()
        {
            if (PlayingWithBot)
            {
                checkBox2.Enabled = true;
            }
            else
            {
                checkBox2.Enabled = false;
            }
        }

        void reset()
        {
            round = 1;
            textBox1.Text = "";
            moveNum = 0;
            selectedPiece = 0;
            legalMove.Clear();
            currentPlayer = 0;
            label9.ForeColor = SystemColors.Highlight;
            label10.ForeColor = SystemColors.ControlText;
            Array.Copy(boardConfig, newboardConfig, boardConfig.Length);
            resetColors();
            refresh();

        }

        void updateScreen(int start, int end, bool takes, bool display, bool win)
        {
            
            if (start != end)
            {
                string endcoord = "";
                string startcoord = "";


                int i = 0;
                foreach (var item in newboardConfig)
                {
                    if (i == end)
                    {
                        if (!takes)
                        {
                            if (!display)
                            {
                                saved += round.ToString() + ". ";
                            }
                            saved += item.Item2.Name;
                            if (win)
                                saved += "#";
                            saved += " ";
                            break;
                        }
                        else
                        {
                            endcoord = item.Item2.Name;
                        }

                    }
                    if (i == start && takes)
                    {
                        startcoord = item.Item2.Name;
                    }

                    i += 1;
                }


                if (takes)
                {
                    if (!display)
                    {
                        saved += round.ToString() + ". ";
                    }
                    saved += startcoord + "x" + endcoord;
                    if (win)
                        saved += "#";
                    saved += " ";
                }

            }
            else
            {
                saved += "#";
                saved += " ";
            }
            

            

            //MessageBox.Show(saved);

            if (display || win)
            {
                saved += Environment.NewLine;
                textBox1.AppendText(saved);
                saved = "";
            }

        }

        void refresh()
        {
            //MessageBox.Show("refreshing");
            foreach (var item in newboardConfig)
            {
                if (item.Item1 == 1)
                {
                    item.Item2.BackgroundImage = Properties.Resources.b_pawn;
                }
                else if (item.Item1 == 2)
                {
                    item.Item2.BackgroundImage = Properties.Resources.w_pawn;
                }
                else
                {
                    item.Item2.BackgroundImage = null;
                }
                
                //MessageBox.Show(item.Item2.Name);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            whiteScore = blackScore = 0;
            reset();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (PlayingWithBot)
            {
                PlayingWithBot = false;
            }
            else
            {
                PlayingWithBot = true;
            }
            toggle();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (training)
            {
                
                if (MessageBox.Show("This will erase any training progress you have done", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes){
                    training = false;
                }
                else
                {
                    return;
                }
                
            }
            else
            {
                MessageBox.Show("Training mode engaged - Have fun teaching the bot");
                training = true;
            }

            H.init(training);
        }

        private void BTN_Click(object sender, EventArgs e)
        {
            if (currentPlayer == 1 && PlayingWithBot)
            {
                return;
            }

            int index = 0;
            var senderBTN = (Button)sender;

            switch (senderBTN.Name)
            {
                case "a3":
                    index = 0;
                    break;
                case "b3":
                    index = 1;
                    break;
                case "c3":
                    index = 2;
                    break;
                case "a2":
                    index = 3;
                    break;
                case "b2":
                    index = 4;
                    break;
                case "c2":
                    index = 5;
                    break;
                case "a1":
                    index = 6;
                    break;
                case "b1":
                    index = 7;
                    break;
                case "c1":
                    index = 8;
                    break;
            }
            

            if (moveNum == 0)
            {
                if (calculateLegalMoves(index))
                {
                    // draw legal moves
                    legalMove = legalMoves(selectedPiece);

                    int i = 0;
                    foreach (var item in newboardConfig)
                    {
                        if (legalMove.Contains(i))
                        {
                            item.Item2.BackColor = Color.Orange;
                            //MessageBox.Show(i.ToString());
                        }
                        i++;
                    }
                }
            }
            else
            {
                resetColors();
                if (calculateLegalMoves(index))
                {
                    // move
                    move(index);

                }
                else
                {
                    if (calculateLegalMoves(index))
                    {
                        // draw legal moves
                        legalMove = legalMoves(selectedPiece);

                        int i = 0;
                        foreach (var item in newboardConfig)
                        {
                            if (legalMove.Contains(i))
                            {
                                item.Item2.BackColor = Color.Orange;
                            }
                            i++;
                        }
                    }
                }
            }
                

        }

        void resetColors()
        {
            int i = 0;
            foreach (var item in newboardConfig)
            {
                if (i % 2 == 0)
                {
                    
                    item.Item2.BackColor = SystemColors.ControlLightLight;

                }
                else
                {
                    
                    item.Item2.BackColor = SystemColors.ControlText;
                }
                i++;
            }
        }

        private List<int> legalMoves(int selected)
        {
            
            int offset_min = 2;
            int offset_max = 5;
            List<int> validMoves = new List<int>();



            //enemy

            int possible = 0;
            
            // will return us an array of legal moves for this piece
            if (currentPlayer == 0)
            {
                if (selected == 1 || selected == 4 || selected == 7) // middle row
                {
                    offset_min = 2;
                    offset_max = 4;
                }
                else if (selected == 0 || selected == 3 || selected == 6)
                {
                    offset_min = 2;
                    offset_max = 3;

                }
                else
                {
                    offset_min = 3;
                    offset_max = 4;
                }

                possible = selected - 3;
            }
            else
            {
                if (selected == 1 || selected == 4 || selected == 7) // middle row
                {
                    offset_min = 2;
                    offset_max = 4;
                }
                else if (selected == 0 || selected == 3 || selected == 6)
                {
                    offset_min = 3;
                    offset_max = 4;

                }
                else
                {
                    offset_min = 2;
                    offset_max = 3;
                }

                possible = selected + 3;
                // possible +- 1

            }

            int[] test = { possible - 1, possible, possible + 1 };


            foreach (var item in test)
            {

                if (Math.Abs(selected - item) >= offset_min && Math.Abs(selected - item) <= offset_max)
                {
                    //valid
                    if (occupied(item) == 0) // blocked my my piece
                    {
                        
                        continue;
                    }
                    if (item != possible) //not directly across
                    {
                        if (occupied(item) != 2) // not enemy piece
                        {
                            //MessageBox.Show(item.ToString());
                            continue;
                        }
                    }
                    else
                    {
                        if (occupied(item) == 2) // enemy piece
                        {
                            continue;
                        }
                    }
                    validMoves.Add(item);
                }
            }

            return validMoves;
        }

        private int occupied(int index) // index we want to test
        {
            var r = 2;
            if (currentPlayer == 1)
            {
                r = 1;
            }

            

            var loc = getLOC(index);
            //MessageBox.Show(newboardConfig[loc.Item1, loc.Item2].ToString());

            if (newboardConfig[loc.Item1, loc.Item2].Item1 == r) // my piece
            {
                return 0;
            }
            else if (newboardConfig[loc.Item1, loc.Item2].Item1 == 0) // empty
            {
                return 1;
            }


            return 2; // if enemy pieces occupy it
        }

        private bool calculateLegalMoves(int index)
        {
            

            if (moveNum == 0)
            {
                //MessageBox.Show(occupied(index).ToString());
                if (occupied(index) == 0)
                {
                    selectedPiece = index;
                    moveNum = 1;
                    return true;
                }


                return false; // do nothing

            }
            else
            {
                // rule: pawns can only move forward or diagonally (to take)
                moveNum = 0;

                if (legalMove.Contains(index))
                {
                    // move
                    
                    
                    return true;
                }
                else // reset back to move 0
                {

                    legalMove.Clear();
                    return false;
                }
                
            }
        }

        private void move(int destination)
        {
            int val = 2;
            bool wins = false;

            if (currentPlayer == 1)
            {
                val = 1;
            }

            var location = getLOC(destination);
            var start = getLOC(selectedPiece);

            if (location.Item1 == 0 || location.Item1 == 2)
            {
                wins = true;

            }

            if (newboardConfig[location.Item1, location.Item2].Item1 != 0)
            {
                if (currentPlayer == 1)
                {
                    updateScreen(selectedPiece, destination, true, true, wins);
                }
                else
                {
                    updateScreen(selectedPiece, destination, true, false, wins);
                }

            }
            else
            {
                if (currentPlayer == 1)
                {
                    updateScreen(selectedPiece, destination, false, true, wins);
                }
                else
                {
                    updateScreen(selectedPiece, destination, false, false, wins);
                }
            }

            newboardConfig[location.Item1, location.Item2].Item1 = val;

            newboardConfig[start.Item1, start.Item2].Item1 = 0;



            refresh();

            if (wins)
            {
                win(currentPlayer);
            }
            else
            {
                moved();
            }
            
        }

        private void win(int winner)
        {
            string player = "";
            if (winner == 0)
            {
                player = "White";
                whiteScore += 1;
                label11.Text = whiteScore.ToString();
                if (PlayingWithBot)
                {
                    // lost
                    H.endGame(false);

                }

            }
            else
            {
                player = "Black";
                blackScore += 1;
                label12.Text = blackScore.ToString();
                if (PlayingWithBot)
                {
                    //  won
                    H.endGame(true);
                }

            }


            MessageBox.Show(player + " wins!");
            reset();
        }

        private (int, int) getLOC(int index)
        {
            int x = (index / 3);
            int y = 0;

            if (index == 1 || index == 4 || index == 7)
            {
                y = 1;
            }
            else if (index == 2 || index == 5 || index == 8)
            {
                y = 2;
            }

            return (x, y);

        }
    }
}
