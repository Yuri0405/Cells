namespace Cells1
{
    internal class Program
    {
        static Random rand = new Random();
        static bool stop = false;
        static object lockObject = new object(); // Shared lock object

        static void Main(string[] args)
        {
            Console.WriteLine("Enter parameters in the format '-N 10 -K 2 -p 0,5':");
            string input = Console.ReadLine();
            var parameters = ParseInput(input);

            int N = parameters.ContainsKey("N") ? int.Parse(parameters["N"]) : 10; // default 10 cells
            int K = parameters.ContainsKey("K") ? int.Parse(parameters["K"]) : 2;  // default 2 particles
            double p = parameters.ContainsKey("p") ? double.Parse(parameters["p"]) : 0.5; // default probability 0.5
            //int simulationTimeInSeconds = 60;

            List<int> cells = new List<int>(N);

            InitializeCells(cells, N, K);
            PrintCrystall(cells);

            List<Thread> threads = new List<Thread>(N);

            for (int i = 0; i < K; i++)
            {
                int initialId = N - 1;
                Thread thread = new Thread(() => MoveCell(cells, p));
                thread.Name = $"Particle #{i}";
                threads.Add(thread);
                thread.Start();
            }

            Timer printTimer = new Timer(PrintCrystall, cells, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Timer stopTimer = new Timer(StopProgram, printTimer, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            int sumOfParticles = 0;

            for (int i = 0; i < N; i++)
            {
                sumOfParticles += cells[i];
            }

            Console.WriteLine("Recent state of crystal:");
            PrintCrystall(cells);

            Console.WriteLine($"Particles in crystal: {sumOfParticles}");
        }

        static void MoveCell(List<int> cells, double p)
        {
            int currentId = cells.Count - 1;

            while (!stop)
            {
                lock (lockObject)
                {
                    double m = rand.NextDouble();

                    if (m > p && currentId > 0)
                    {
                        cells[currentId] -= 1;
                        currentId -= 1;
                        cells[currentId] += 1;
                    }
                    else if (m < p && currentId < cells.Count - 1)
                    {
                        cells[currentId] -= 1;
                        currentId += 1;
                        cells[currentId] += 1;
                    }
                }
            }

        }

        static private void InitializeCells(List<int> cells, int N, int K)
        {
            for (int i = 0; i < N; i++)
            {
                cells.Add(0);
            }

            cells[N - 1] = K;
        }

        static private void PrintCrystall(object state)
        {
            lock (lockObject)
            {
                List<int> cells = (List<int>)state;

                Console.Write("[");
                for (int i = 0; i < cells.Count; i++)
                {
                    if (i == cells.Count - 1)
                        Console.Write(cells[i]);
                    else
                        Console.Write(cells[i] + ", ");
                }
                Console.WriteLine("] at time : " + DateTime.Now.ToLongTimeString());
            }
        }

        static void StopProgram(object state)
        {
            stop = true;
        }


        static Dictionary<string, string> ParseInput(string input)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var tokens = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i += 2)
            {
                if (i + 1 < tokens.Length && tokens[i].StartsWith("-"))
                {
                    parameters[tokens[i].TrimStart('-')] = tokens[i + 1];
                }
            }
            return parameters;
        }
    }
}
