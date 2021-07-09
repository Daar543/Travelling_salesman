using System;
using System.Collections.Generic;
using System.IO;
namespace TSP_CSharp
{
    public static class GraphMaker
    {
        public static int[][]GraphFromFile(string file)
        {
            var Matrix = new List<int[]>();
            var sr = new StreamReader(file);
            var line = sr.ReadLine();
            while(line != "" && !(line is null))
            {
                var row = new List<int>();
                var split = line.Split(" ");
                foreach (var item in split)
                {
                    if (!int.TryParse(item, out int weight))
                        break;
                    row.Add(weight);
                }
                Matrix.Add(row.ToArray());
                line = sr.ReadLine();
            }
            if (Matrix.Count != Matrix[^1].Length)
            {
                throw new FormatException("Not a square matrix");
            }
            return Matrix.ToArray();
        }
    }

    public static class Pathfinding
    {
        const int MaxLength = int.MaxValue/2;
        static int EnumSubset(int[]elements)
        {
            int num = 0;
            foreach(var elem in elements)
            {
                num += 1 << elem;
            }
            return num;
        }
        static List<int> DecodeSubset(int order, int setSize)
        {
            var BitPowers = new List<int>();
            for (int i = 0; i < setSize; ++i)
            {
                if ((order & (1 << i)) != 0)
                {
                    BitPowers.Add(i);
                }
            }
            return BitPowers;
        }
        static IEnumerable<List<int>> Combinations (int k, int n)
        {
            int set = (1 << k) - 1;
            int limit = (1 << n);
            while (set < limit)
            {
                List<int> combination = DecodeSubset(set,n);
                yield return combination;

                // Gosper's hack:
                int c = set & -set;
                int r = set + c;
                set = (((r ^ set) >> 2) / c) | r;
            }
        }
        static IEnumerable<int> CombinationsEnum(int k, int n)
        {
            int set = (1 << k) - 1;
            int limit = (1 << n);
            while (set < limit)
            {
                yield return set;

                // Gosper's hack:
                int c = set & -set;
                int r = set + c;
                set = (((r ^ set) >> 2) / c) | r;
            }
        }
        /* Python code
         * MaxLength = 999999
           G[0],G[start] = G[start],G[0]
           n = len(G)
           print("Generating cache...")
           C = [[MaxLength for _ in range(n)] for __ in range(1 << n)]
           Prec = [[0 for _ in range(n)] for __ in range(1 << n)]
           print("Cache generated")
           C[1][0] = 0 # {0} <-> 1
           combcount = -1
           for size in range(1, n):
               if(combcount>0): 
                   print("Calculated",combcount,"path combinations out of",1 << (n-1))
               for S in combinations(range(1, n), size):
                   combcount+=1
                   S = (0,) + S
                   k = sum([1 << i for i in S]) #combination enumeration
                   for i in S:
                       if i == 0: continue
                       for j in S:
                           if j == i: continue
                           cur_index = k ^ (1 << i)
                           cur_val = C[k][i]
                           update_val = C[cur_index][j] + G[j][i] #subset without starting index + returning cost (in short: C[S−{i}][j] )
                           if(update_val < cur_val):
                               Prec[k][i] = j #Preceeding node
                               C[k][i] = update_val

            all_index = (1 << n) - 1 #n ones
            length = MaxLength
            for i in range(n):
                currlength = C[all_index][i]+G[0][i]
                if(currlength<length):
                    length = currlength
                    pr = i #First predecessor
    
            k = 0
            for i in range(0,n):
                k += 1<<i
            tour = [0]
            while(len(tour)<len(G)):
                next_pr = Prec[k][pr]
                tour.insert(0,pr)
                k ^= (1<<pr)
                pr = next_pr
            tour.insert(0,0)
            for i in range(1,len(tour)):
                print(tour[i-1],"->",tour[i],"=",G[tour[i-1]][tour[i]])
            return length,tour
        */
        public static int[][] FindShortestHamPath(int[][]Graph, out int MinLength)
        {
            int GraphSize = Graph.Length;
            MinLength = MaxLength;
            //Generate cache
            Console.WriteLine("generating cache");
            var SubPathWeights = new int[1<<GraphSize][];
            for(int i = 0; i < SubPathWeights.Length; ++i)
            {
                SubPathWeights[i] = new int[GraphSize];
                for (int j = 0; j < SubPathWeights[i].Length; ++j)
                {
                    SubPathWeights[i][j] = MaxLength;
                }
            }
            var PreceedingNodes = new int[1 << GraphSize][];
            for (int i = 0; i < PreceedingNodes.Length; ++i)
            {
                PreceedingNodes[i] = new int[GraphSize];
                for (int j = 0; j < PreceedingNodes[i].Length; ++j)
                {
                    PreceedingNodes[i][j] = 0;
                }
            }

            Console.WriteLine("Cache generated");
            SubPathWeights[1][0] = 0; // {0} <-> 1
            int combcount = -1;

            //Increment length of subpaths
            for(int pathSize = 1; pathSize < GraphSize; ++pathSize)
            {
                if (combcount > 0)
                   Console.WriteLine("Calculated {0} path combinations out of {1}", combcount, 1 << (GraphSize - 1));
                combcount++;
                //New subpath
                var EnumCombo = CombinationsEnum(pathSize, GraphSize-1);
                foreach (var combPath in EnumCombo)
                {
                    int PathID = (combPath << 1) + (1 << 0);
                    combcount += 1;
                    int[] subpath = DecodeSubset(PathID, GraphSize).ToArray(); //All nodes get offset by 1, 0 gets added at the start

                    //Calculate all from-to combinations
                    foreach(int to in subpath)
                    {
                        if (to == 0) continue;
                        foreach(int from in subpath)
                        {
                            if (from == to) continue;
                            int SubPathID = PathID ^ (1 << to);
                            int currentLength = SubPathWeights[PathID][to];
                            int newLength = SubPathWeights[SubPathID][from] + Graph[from][to];

                            if (newLength < currentLength)
                            {
                                PreceedingNodes[PathID][to] = from;
                                SubPathWeights[PathID][to] = newLength;
                            }
                        }
                    }
                }
            }
            int FullID = (1 << GraphSize) - 1;
            int length = MaxLength;
            int prec = 0;
            for(int to = 0; to < GraphSize; ++to)
            {
                int newLength = SubPathWeights[FullID][to] + Graph[0][to];
                if (newLength <= length)
                {
                    length = newLength;
                    prec = to;
                }
            }
            MinLength = length;
            int SubID = FullID;
            var edges = new Stack<int>();
            /*tour = [0]
            while (len(tour) < len(G)):
                next_pr = Prec[k][pr]
                tour.insert(0, pr)
                k ^= (1 << pr)
                pr = next_pr
            tour.insert(0, 0)*/
            edges.Push(0);
            while (edges.Count < GraphSize)
            {
                int predecessor = PreceedingNodes[SubID][prec];
                edges.Push(prec);
                SubID ^= (1 << prec);
                prec = predecessor;
            }

            var final = new List<int[]>();
            int start = 0;
            while(edges.Count!=0)
            {
                int fin = edges.Pop();
                final.Add(new int[] { start, fin, Graph[start][fin] });
                start = fin;
            }
            return final.ToArray();
            /*k = 0
            for i in range(0, n):
                k += 1 << i
            tour = [0]
            while (len(tour) < len(G)):
                next_pr = Prec[k][pr]
                tour.insert(0, pr)
                k ^= (1 << pr)
                pr = next_pr
            tour.insert(0, 0)
            for i in range(1, len(tour)):
                print(tour[i - 1], "->", tour[i], "=", G[tour[i - 1]][tour[i]])
            return length,tour*/
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            int[][] Labyrint = GraphMaker.GraphFromFile("../../../../Labyrint/data.txt");
            int[][] Path = Pathfinding.FindShortestHamPath(Labyrint, out int LenOfThis);
            foreach(var e in Path)
            {
                Console.Write("{0} -> {1} = {2}",e[0],e[1],e[2]);
                Console.WriteLine();
            }
            Console.WriteLine("Total length: {0}",LenOfThis);
            Console.WriteLine("Hello World!");
        }
    }
}
