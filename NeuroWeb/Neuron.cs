using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeuroWeb
{
    class Neuron
    {
        public double output = 0;

        public double error = 0;

        public void Compute(double[] connections,Neuron[] enterArray)
        {
            double summ = 0;
            for (int i = 0; i < connections.Length; i++)
            {
                summ += enterArray[i].output * connections[i];
            }
            output = (double)1 / (1 + Math.Pow(Math.E, -summ));
        }
    }
}
