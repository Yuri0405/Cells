namespace Cells0
{
    internal class Program
    {
        static Random rand = new Random();
        static bool stop = false;
        static object lockObject = new object(); // Shared lock object

        // Define a ThreadLocal variable to store an integer value
        static void Main(string[] args)
        {
            // Define parameters
            int N = 10; // Number of cells
            int K = 2; // Number of particles
            double p = 0.5; // Probability of moving right
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

            for (int i = 0;i < N; i++)
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
                        --cells[currentId];
                        currentId--;
                        ++cells[currentId];
                    }
                    else if (m < p && currentId < cells.Count - 1)
                    {
                        --cells[currentId];
                        currentId++;
                        ++cells[currentId];
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
            List<int> cells = (List<int>)state;
            Console.Write("[");
            for (int i = 0; i < cells.Count; i++)
            {
                if (i == cells.Count - 1)
                    Console.Write(cells[i]);
                else
                    Console.Write(cells[i] + ", ");
            }
            Console.WriteLine("] in thread " + Thread.CurrentThread.Name);
        }

        static void StopProgram(object state)
        {
            stop = true;
        }


    }
}
