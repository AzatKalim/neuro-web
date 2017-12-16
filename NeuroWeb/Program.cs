using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NeuroWeb
{
    class Program
    { 
        static void Main(string[] args)
        {        
            Web web = new Web(Classifier.WordsCount);
            //Web web = new Web();
            //Console.WriteLine(web.Compute(message));
            string[] goodMessages = Classifier.GetMesaages("good.txt");
            string[] badMessages = Classifier.GetMesaages("bad.txt");
            web.TeachWeb(goodMessages, badMessages, 250);
            web.WebToFile();
            Test(web);
        }
        static void Test(Web web)
        {
            Console.WriteLine("Test:");
            string testFile = @"goodTest.txt";
            string message;
            StreamReader sr = new StreamReader(testFile);
            Console.WriteLine("HAM:");
            for (int i = 0; i < 10; i++)
            {
                message = sr.ReadLine();
                string result = web.Compute(message).ToString();
                Console.WriteLine(i + 1 + ") " + result);
            }
            testFile = @"badTest.txt";
            sr = new StreamReader(testFile);
            Console.WriteLine("SPAM:");
            for (int i = 0; i < 10; i++)
            {
                message = sr.ReadLine();
                string result = web.Compute(message).ToString();
                Console.WriteLine(i + 1 + ") " + result);
            }
            sr.Close();
        }
    }
}
