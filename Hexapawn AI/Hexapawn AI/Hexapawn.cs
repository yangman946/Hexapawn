/* Hexapawn AI by Clarence Yang 9/01/2023
 * Features:
 * - Ability to learn: stores training data in json
 * - interfaces with engine
 * - plays according to legal moves
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Hexapawn_AI
{

    public class saveData
    {
        public int[][] config { get; set; }
        public int[][] moves { get; set; }
        public int[] weights { get; set; }
    }
    class Hexapawn
    {
        // store a list of all outcomes: 1. board config for this outcome, 2. all possible moves, 3. weights for possible moves
        public List<(int[][], int[][], int[])> outcomes = new List<(int[][], int[][], int[])>();

        // store a list of all played moves, this will decide punishment/reward in the event the bot loses/wins
        (int[][], int) LastMove; // index
        int[][] b;

        public int[] getMove(int[,] board) // main function
        {
            b = converter(board);
            int[] move = searchOutComes(b);
            //LastMove = (board, )
            return move;
        }

        public void init(bool train)
        {
            outcomes.Clear(); //will override any training data
            if (!train)
            {
                using (StreamReader r = new StreamReader("smartBrain.json"))
                {
                    string json = r.ReadToEnd();
                    List<saveData> items = JsonSerializer.Deserialize<List<saveData>>(json);
                    for (int i = 0; i < items.Count; i++)
                    {
                        outcomes.Add((items[i].config, items[i].moves, items[i].weights));
                    }
                }


            }

            // otherwise do nothing - empty 
            // will write text file contents to outcomes list
            
        }

        public void save()
        {
            List<saveData> d = new List<saveData>();
            
            
            for (int i = 0; i < outcomes.Count; i++)
            {
                
                d.Add(new saveData()
                {
                    config = outcomes[i].Item1,
                    moves = outcomes[i].Item2,
                    weights = outcomes[i].Item3
                });
            }
            string fileName = "brain.json";
            string jsonString = JsonSerializer.Serialize(d);
            File.WriteAllText(fileName, jsonString);
        }

        public int[][] converter(int[,] board) // 2d array to array of arrays
        {
            List<int[]> array = new List<int[]>();
            List<int> row = new List<int>();
            int num = 1;
            foreach (int index in board)
            {

                row.Add(index);
                //MessageBox.Show(index.ToString());

                if (num % 3 == 0)
                {
                    array.Add(row.ToArray());
                    row = new List<int>();
                }
                num += 1;

            }

            return array.ToArray();
        }

        // Will store to outcomes for each new outcome the bot encounters
        public int[] newOutCome(int[,] board, int[][] legalMoves)
        {
            // get possible moves for all pieces (start, destination) match to weights, set to 0 otherwise
            // write to outcomes list
            //MessageBox.Show(legalMoves.Length.ToString());
            int[] weights = new int[legalMoves.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 1;
                //MessageBox.Show(legalMoves[i].Item1.ToString() + " " + legalMoves[i].Item2.ToString());
            }
            
            outcomes.Add((converter(board), legalMoves, weights)); // each outcomes item contains one board config, one list of legalmoves and one weights
            return getOutCome(outcomes.Count - 1);
        }


        // will access current board config and get move
        int[] searchOutComes(int[][] bo)
        {
            // search outcomes list for board config (without buttons)
            // if doesnt exist create new outcome
            
            int i = 0;
            foreach (var item in outcomes)
            {
                //MessageBox.Show("checking");
                //MessageBox.Show(bo.Length + " " + item.Item1.Length);
                //MessageBox.Show(bo[0].Length + " " + item.Item1[0].Length);
                /*
                List<int> b1 = bo[0].ToList();
                List<int> I1 = item.Item1[0].ToList();
                b1.Sort();
                I1.Sort();

                List<int> b2 = bo[1].ToList();
                List<int> I2 = item.Item1[1].ToList();
                b2.Sort();
                I2.Sort();

                List<int> b3 = bo[2].ToList();
                List<int> I3 = item.Item1[2].ToList();
                b3.Sort();
                I3.Sort();
                */



                //if (b1.SequenceEqual(I1) && b2.SequenceEqual(I2) && b3.SequenceEqual(I3))
                if (compare2dArray(bo, item.Item1))
                {
                    //MessageBox.Show("seen this one before");

                    
                    
                    return getOutCome(i);
                }
                i += 1;
            }
            return new int[] { 0, 0 };


            // will get outcome from tuple of weights
        }

        bool compare2dArray(int[][] first, int[][] second)
        {
            int v = 0;
            int n = 0;
            

            foreach (int[] val in second)
            {
                foreach (int num in val)
                {

                    if (num != first[v][n])
                    {
                        return false;
                    }
                    n += 1;
                }
                v += 1;
                n = 0;
            }

            return true;
        }

        int[] getOutCome(int index)
        {
            Random rnd = new Random();
            // use random to get me a move
            int sumWeights = 0;
            
            List<int> sums = new List<int>();
            for (int i = 0; i < outcomes[index].Item3.Length; i++)
            {
                sumWeights += outcomes[index].Item3[i];
                sums.Add(sumWeights);
            }

            double r = rnd.NextDouble() * sumWeights;

            int selected = 0;

            for (int i = 0; i < outcomes[index].Item3.Length; i++)
            {
                if (sums[i] >= r)
                {
                    selected = i;
                    break;
                }
            }
            //MessageBox.Show(outcomes.Count.ToString());
            //MessageBox.Show(outcomes[index].Item2.Length.ToString() + " | " + outcomes[index].Item3.Length.ToString());
            //MessageBox.Show(selected.ToString());
            //MessageBox.Show(outcomes[index].Item2[selected].ToString());
            LastMove = (b, selected);
            return outcomes[index].Item2[selected];

        }

        // if win, add weights to winning move 
        // if lose, remove weights for losing move
        public void endGame(bool won)
        {
            //MessageBox.Show("updating...");
            for (int i = 0; i < outcomes.Count; i++)
            {
                if (compare2dArray(outcomes[i].Item1, LastMove.Item1))
                {
                    //MessageBox.Show("found board config");
                    if (won)
                    {
                        outcomes[i].Item3[LastMove.Item2] += 1;
                    }
                    else
                    {
                        outcomes[i].Item3[LastMove.Item2] -= 1;
                        if (outcomes[i].Item3[LastMove.Item2] < 0)
                        {
                            outcomes[i].Item3[LastMove.Item2] = 0;
                        }
                    }
                    
                }
            }
            save();
        }

    }

}
