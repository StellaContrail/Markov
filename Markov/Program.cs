using System;

namespace Markov
{
    class Program
    {

        static void Main(string[] args)
        {
            string path = Environment.CurrentDirectory + @"\rashoumon.txt";

            MarkovChain markov = new MarkovChain();
            markov.tripletsLengthMax = 10;
            markov.tripletsLengthMin = 5;
            markov.LoadText(path);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(markov.Generate());
            }

            Console.WriteLine("\nFinished");
            Console.ReadLine();
        }

    }
}
