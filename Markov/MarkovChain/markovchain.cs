using NMeCab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal class MarkovChain
{
    private const string BOS = "__BOS__";
    private const string EOS = "__EOS__";

    // setting
    public int attemptsLimit { get; set; } = 50;

    public int tripletsLengthMax { get; set; } = 10;
    public int tripletsLengthMin { get; set; } = 3;

    private List<Triplet> triplets;

    public MarkovChain()
    {
        triplets = new List<Triplet>();
    }

    public void Load(List<string> sentences)
    {
        List<string> words = new List<string>();
        using (MeCabTagger mecab = MeCabTagger.Create())
        {
            foreach (string sentence in sentences)
            {
                var nodes = mecab.ParseToNode(sentence) as MeCabNode;
                words.Add(BOS);
                nodes = nodes.Next;
                while (nodes != null)
                {
                    words.Add(nodes.Surface);
                    nodes = nodes.Next;
                }
                words.Add(EOS);
            }
        }

        for (int i = 0; i < words.Count() / 3; i++)
            triplets.Add(new Triplet(new string[] { words[3 * i], words[3 * i + 1], words[3 * i + 2] }));
    }

    public void LoadText(string path)
    {
        List<string> sentences = new List<string>();
        using (var sr = new StreamReader(path))
        {
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                sentences.AddRange(
                    line.Split('。', '.')
                    .Select(x => x.Trim())
                    .ToList() as List<string>
                    );
            }
        }

        Load(sentences);
    }

    public void LoadTweets(string path)
    {
        List<string> sentences = new List<string>();
        using (var sr = new StreamReader(path))
        {
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                sentences.AddRange(
                    line.Split(',')
                    .Where((item, index) => index == 5 && item.IndexOf("@") == -1 && item.IndexOf("RT") == -1 && item.IndexOf("http") == -1)
                    .Select(x => x.Trim('"'))
                    .ToList() as List<string>
                    );
            }
        }

        Load(sentences);
    }

    private Random random = new Random();

    public string Generate()
    {
        List<Triplet> sentenceTriplets = new List<Triplet>();

        List<Triplet> headTriplets = triplets.Where(triplet => triplet[0] == BOS).ToList();

        Triplet firstTriplet = headTriplets[random.Next(headTriplets.Count())];
        sentenceTriplets.Add(firstTriplet);

        string head = firstTriplet[0];
        string mid = firstTriplet[1];
        string tail = firstTriplet[2];

        int attemptsCount = 0;
        while (attemptsCount++ < attemptsLimit && mid != EOS && tail != EOS)
        {
            List<Triplet> nextTriplets = triplets.Where(triplet => triplet[0] == tail).ToList();
            if (nextTriplets.Count() == 0)
            {
                break;
            }
            Triplet nextTriplet = nextTriplets[random.Next(nextTriplets.Count())];
            mid = nextTriplet[1];
            tail = nextTriplet[2];
            sentenceTriplets.Add(new Triplet(new string[] { "", mid, tail }));
        }

        int tripletsLength = sentenceTriplets.Count();
        if (tripletsLengthMin <= tripletsLength && tripletsLength <= tripletsLengthMax)
        {
            string result = "";
            for (int i = 0; i < tripletsLength - 1; i++)
            {
                Triplet triplet = sentenceTriplets[i];
                result += triplet[1] + triplet[2];
            }
            Triplet lastTriplet = sentenceTriplets[tripletsLength - 1];
            result += lastTriplet[1] == EOS ? "" : lastTriplet[1];
            return result;
        }
        else
        {
            return Generate();
        }
    }

    private class Triplet
    {
        private string[] words;

        public Triplet(string[] _words)
        {
            if (_words.Length == 3)
                words = _words;
            else
                throw new ArgumentException("Insufficient parameters");
        }

        public string this[int i]
        {
            get => words[i];
        }
    }
}