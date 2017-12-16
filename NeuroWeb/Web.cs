using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NeuroWeb
{
    class Web
    {
        const int EXIT_COUNT = 1;

        const double EPSILON = 0.7;

        const double ALFA = 0.3;

        const int TEACH_COUNT = 100;

        const string FIRST_FILE_NAME = "enterToHidden.txt";

        const string SECOND_FILE_NAME = "hiddenToExit.txt";

        Neuron [] enterLayer;

        Neuron [] hiddenLayer;

        Neuron exitNeuron;

        double[][] enterToHidden;
        double[][] enterToHiddenDelta;

        double[][] hiddenToExit;
        double[][] hiddenToExitDelta;

        public Web(int enterCount)
        {
            enterLayer = new Neuron[enterCount];
            hiddenLayer = new Neuron[enterCount];
            for (int i = 0; i < enterCount; i++)
            {
                enterLayer[i] = new Neuron();
                hiddenLayer[i] = new Neuron();
            }
            enterToHiddenDelta =InitDelta(enterCount, enterCount);
            hiddenToExitDelta = InitDelta(EXIT_COUNT, enterCount);
            exitNeuron = new Neuron();
            enterToHidden = new double[enterCount][];
            hiddenToExit = new double[EXIT_COUNT][];
            Random random= new Random();
            for (int i = 0; i < enterCount; i++)
            {    
                enterToHidden[i] = new double[enterCount];           
                for (int j = 0; j < enterCount; j++)
                {
                    enterToHidden[i][j] = -0.2;
                }
                
            }
            for (int i = 0; i < hiddenToExit.Length; i++)
            {
                hiddenToExit[i] = new double[enterCount];
                for (int j = 0; j < enterCount; j++)
                {
                    hiddenToExit[i][j] = 0;//(double)1 / (random.Next(100) + 1);
                }
            }

        }

        public Web()
        {
            StreamReader file = new StreamReader(FIRST_FILE_NAME);
            string line = file.ReadLine();
            string[] temp = line.Split(' ');
            int rowCount = Convert.ToInt32(temp[0]);
            int collumnCount = Convert.ToInt32(temp[1]);
            enterLayer = new Neuron[collumnCount];
            hiddenLayer = new Neuron[rowCount];
            enterToHiddenDelta = InitDelta(rowCount, rowCount);
            hiddenToExitDelta = InitDelta(EXIT_COUNT, rowCount);
            for (int i = 0; i < collumnCount; i++)
            {
                enterLayer[i] = new Neuron();
            }
            for (int i = 0; i < rowCount; i++)
            {
                hiddenLayer[i] = new Neuron();
            }
            exitNeuron = new Neuron();

            enterToHidden = new double[rowCount][];

            for (int i = 0; i < rowCount; i++)
            {
                enterToHidden[i] = new double[collumnCount];
                line = file.ReadLine();
                temp = line.Split(' ');
                for (int j = 0; j < collumnCount; j++)
                {
                    enterToHidden[i][j] = Convert.ToDouble(temp[j]);
                }
            }
            file.Close();
            file = new StreamReader(SECOND_FILE_NAME);
            line = file.ReadLine();
            temp = line.Split(' ');
            rowCount = Convert.ToInt32(temp[0]);
            collumnCount = Convert.ToInt32(temp[1]);

            hiddenToExit = new double[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                hiddenToExit[i] = new double[collumnCount];
                line = file.ReadLine();
                temp = line.Split(' ');
                for (int j = 0; j < collumnCount; j++)
                {
                    hiddenToExit[i][j] = Convert.ToDouble(temp[j]);
                }
            }
            file.Close();
        }

        public void WebToFile()
        {
            StreamWriter file = new StreamWriter(FIRST_FILE_NAME);

            file.WriteLine(enterToHidden.Length.ToString() + ' '  + enterToHidden.Length.ToString());
            for (int i = 0; i < enterToHidden.Length; i++)
            {
                for (int j = 0; j < enterToHidden[i].Length; j++)
                {
                    file.Write(Math.Round(enterToHidden[i][j],5) + " ");               
                }
                file.Write(Environment.NewLine);
            }
            file.Close(); 
            file = new StreamWriter(SECOND_FILE_NAME);
            file.WriteLine(EXIT_COUNT + " " + enterToHidden.Length);
            for (int i = 0; i < hiddenToExit.Length; i++)
            {
                for (int j = 0; j < hiddenToExit[i].Length; j++)
                {
                    file.Write(Math.Round(hiddenToExit[i][j],5) + " ");
                }
                file.Write(Environment.NewLine);
            }
            file.Close();
        }

        public double Compute(string text)
        {
            bool [] array= Classifier.GetVector(text);
            InitEnterLayer(array);
            ComputeNeurons(enterToHidden, enterLayer, hiddenLayer);
            ComputeNeurons(hiddenToExit, hiddenLayer, exitNeuron);
            return exitNeuron.output;
        }

        public void TeachWeb(string[] goodMessages, string[] badMessages, int testCount)
        {
            for (int i = 0; i < testCount; i++)
            {
                for (int j = 0; j < badMessages.Length; j++)
                {
                    double result = Compute(badMessages[j]);
                    //Console.WriteLine(j + " " + (result));
                    Backpropagation(0, result);
                }
                for (int j = 0; j < goodMessages.Length; j++)
                {
                    double result = Compute(goodMessages[j]);              
                    Backpropagation(1, result);
                }
                
            }
            //Console.Clear();
            //for (int j = 0; j < badMessages.Length; j++)
            //{                    
            //    double result = Compute(badMessages[j]); 
            //    Console.WriteLine(j + " " + result + badMessages[j]);          
            //}
            //for (int j = 0; j < goodMessages.Length; j++)
            //{
            //    double result = Compute(goodMessages[j]);
            //    Console.WriteLine(j + " " + result +goodMessages[j]);
            //}
            //Console.ReadKey();
        }

        private void Backpropagation(double ideal,double exit)
        {
            double error= ideal-exit;
            exitNeuron.error = (1 - exitNeuron.output) * exitNeuron.output * (ideal - exit);
            ComputeErrors(hiddenToExit, hiddenLayer, exitNeuron);
            UpdateWeights(hiddenToExit, hiddenToExitDelta, hiddenLayer, exitNeuron);
            ComputeErrors(enterToHidden, enterLayer, hiddenLayer);
            UpdateWeights(enterToHidden, enterToHiddenDelta, enterLayer, hiddenLayer);
        }

        private void UpdateWeights(double[][] connection,double[][] delta, Neuron[] enterLayer, Neuron exit)
        {
            for (int i = 0; i < delta.Length; i++)
            {
                for (int j = 0; j < delta[i].Length; j++)
                {
                    double grad = exit.error * enterLayer[j].output;
                    delta[i][j] = EPSILON * grad + ALFA * delta[i][j];
                    connection[i][j] += delta[i][j];
                }
            }
        }

        private void UpdateWeights(double[][] connection, double[][] delta, Neuron[] enterLayer, Neuron[] exitLayer)
        {
            for (int i = 0; i < delta.Length; i++)
            {
                for (int j = 0; j < delta[i].Length; j++)
                {
                    double grad = exitLayer[i].error * enterLayer[j].output;
                    delta[i][j] = EPSILON * grad + ALFA * delta[i][j];
                    connection[i][j] += delta[i][j];
                }
            }

        }

        private void ComputeErrors(double[][] connection, Neuron[] enterLayer, Neuron exit)
        {
            for (int i = 0; i < connection.Length; i++)
            {
                enterLayer[i].error = connection[0][i] * exit.error*(1 - enterLayer[i].output) * enterLayer[i].output;
            }
        }

        private void ComputeErrors(double[][] connection, Neuron[] enterLayer, Neuron[] exitLayer)
        {
            for (int i = 0; i < exitLayer.Length; i++)
            {
                for (int j = 0; j < enterLayer.Length; j++)
                {
                    enterLayer[j].error += exitLayer[i].error * connection[i][j];
                }
            }
            for (int i = 0; i < enterLayer.Length; i++)
            {
                enterLayer[i].error *= (1 - enterLayer[i].output) * enterLayer[i].output;
            }
        }

        private void ComputeNeurons(double[][] connections,Neuron[] enterArray,Neuron[] exitArray)
        {
            for (int i = 0; i < exitArray.Length; i++)
            {
                exitArray[i].Compute(connections[i], enterArray);
            }
        }

        private void ComputeNeurons(double[][] connections, Neuron[] enterArray, Neuron exitArray)
        {
            exitArray.Compute(connections[0], enterArray);
        }

        private void InitEnterLayer(bool[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] == false)
                {
                    enterLayer[i].output = 0;
                }
                else
                {
                    enterLayer[i].output = 1;
                }
            }
        }

        private double[][] InitDelta(int row, int column)
        {
            double[][] delta = new double[row][];
            for (int i = 0; i < row; i++)
            {
                delta[i] = new double[column];
                for (int j = 0; j < column; j++)
                {
                    delta[i][j] = 0;
                }
            }
            return delta;
        }
    }
}
