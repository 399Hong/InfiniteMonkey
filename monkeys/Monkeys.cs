namespace Monkeys
{
    using Carter;
    using Microsoft.AspNetCore.Http;
    using Carter.ModelBinding;
    using Carter.Request;
    using Carter.Response;
    using System.Linq;
    using System.Collections.Generic;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using static System.Console;

    using System.Threading.Tasks;
    using System.Text;

    using System.Collections.Concurrent;

    public class HomeModule : CarterModule
    {
        
       
        static Random random = new Random();

        public HomeModule()
        {

           
            //client.BaseAddress = new System.Uri("http://localhost:8101/");
            //clientFitness.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //clientFitness.BaseAddress = new System.Uri("http://localhost:8091/");

            Post("/try", async (req, res) =>
            {   

               // WriteLine("hhhh");

                var info = await req.Bind<info>();
                await res.CompleteAsync();
                if(info.limit==0)info.limit = 1000;
                
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new System.Uri($"http://localhost:{info.id}/");
                HttpClient clientFitness = new HttpClient();
                clientFitness.BaseAddress = new System.Uri("http://localhost:8091/");
                clientFitness.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //length discovery


                if(info.length==0){
                var discResponse = await clientFitness.PostAsJsonAsync("/assess", new AssessRequest { id = info.id, genomes = new List<string>{""} });
                var discFitness = await discResponse.Content.ReadAsAsync<AssessResponse>();
                info.length= discFitness.scores[0];
               // WriteLine(info.length);





                }


                 



                // generating first set of genome
                List<string> monkeys = new List<string>();
                for (int i = 0; i < info.monkeys; i++) monkeys.Add(newGenome(info.length, random));
             
                
                //#region first run

                var response = await clientFitness.PostAsJsonAsync("/assess", new AssessRequest { id = info.id, genomes = monkeys });

                var fitness = await response.Content.ReadAsAsync<AssessResponse>();
                var minFitnessPair = fitness.scores.Select((v, i) => (v, i)).Min();

                var curFitness = minFitnessPair.v;
                var lastFitness = curFitness;
                //post to client


                await client.PostAsJsonAsync("/top", new TopRequest { id = info.id, loop = 0, score = lastFitness, genome = monkeys[minFitnessPair.i] });
                //#endregion

                //break when target found
                int loop = 0;
                while (curFitness != 0 && loop < info.limit)
                {   
                    //WriteLine(loop);
                    loop++;
                    // get new fitness
                    monkeys = Generate(info.monkeys, info.parallel, fitness.scores, info.length, monkeys, info.crossover / 100.0, info.mutation / 100.0);

                    response = await clientFitness.PostAsJsonAsync("/assess", new AssessRequest { id = info.id, genomes = monkeys });
                    fitness = await response.Content.ReadAsAsync<AssessResponse>();
                    minFitnessPair = fitness.scores.Select((v, i) => (v, i)).Min();
                    curFitness = minFitnessPair.v;


                    if (curFitness < lastFitness)
                    {
                        lastFitness = curFitness;
                        await client.PostAsJsonAsync("/top", new TopRequest { id = info.id, loop = loop, score = lastFitness, genome = monkeys[minFitnessPair.i] });
                        

                    }
                    
                }

                // post to top if finished 

                //await client.PostAsJsonAsync("/top", monkeys[minFitnessPair.i]);







                return;
                
            });
        }

        // other methods?
        // return the index of a geno
        static int ParentSelection(List<int> fitness, int population)
        {
           
            var maxFitness = fitness.Max();

            // coule be optimized tolist is not nessary
            var breedingWeight = fitness.Select(x => maxFitness - x + 1).ToList();
            var sumWeight = breedingWeight.Sum();


            // need check if two seed same TBC????
            var seed = new Random().Next(sumWeight);
            //WriteLine($"seed:{seed}");
            for (int i = 0; i < population; ++i)
            {
                var weight = breedingWeight[i];
                if (seed < weight)
                {   
                    //WriteLine(i);
                    return i;
                }
                else{
                    seed -= weight;
                }

            }

            // error if -1

            WriteLine("Unexpected");

            return 0;
        }
        // length need to be dealed outside this function
        static List<string> Generate(int population, bool parallel, List<int> fitness, int length, List<string> genome, double crossoverProb, double mutationProb)
        {
            //ConcurrentBag<string> newGen = new ConcurrentBag<string>();
            List<string> newGen = new List<string>();


           
            var setCore = new ParallelOptions();
            if(!parallel){
                
                setCore.MaxDegreeOfParallelism =1;
            }
           // WriteLine(setCore.MaxDegreeOfParallelism);

            Parallel.For(0, population / 2,setCore, i =>
            {

                var r = new Random();
                //int[] parentInd = { ParentSelection(fitness, population), ParentSelection(fitness, population) };
               
                string dad = genome[ParentSelection(fitness, population)];
                string mum = genome[ParentSelection(fitness, population)];
                string son;
                string daughter;
                if (r.NextDouble() < crossoverProb)
                {
                    // dynamically discovered if length unknow 
                    // post empty string to get length?


                    int crossoverInd = r.Next(length);
                    son = dad.Substring(0, crossoverInd) + mum.Substring(crossoverInd);
                    daughter = mum.Substring(0, crossoverInd) + dad.Substring(crossoverInd);
                    //WriteLine($"first  {dad.Substring(0,crossoverInd)}  second  {mum.Substring(crossoverInd)};ind:{crossoverInd}");
                    //WriteLine($"first  {mum.Substring(0,crossoverInd)}  second  {dad.Substring(crossoverInd)};ind:{crossoverInd}");


                }
                else
                {
                    son = dad;
                    daughter = mum;
                }
                if (r.NextDouble() < mutationProb)
                {
                    //Console.WriteLine($"son:{son} L:{length}");
                    StringBuilder sb = new StringBuilder(son);
                    sb[r.Next(length)] = (char)r.Next(32, 127);
                    son = sb.ToString();

                }
                if (r.NextDouble() < mutationProb)
                {
                    //Console.WriteLine($"dau:{daughter} L:{length}");
                    StringBuilder sb = new StringBuilder(daughter);
                    sb[r.Next(length)] = (char)r.Next(32, 127);
                    daughter = sb.ToString();


                }
                newGen.Add(son);
                newGen.Add(daughter);


            });
           


            return newGen;//.ToList();
        }
        static string newGenome(int length, Random r) => new string(Enumerable.Range(1, length).Select(_ => (char)r.Next(32, 127)).ToArray());

    }

    public class info
    {
        public int id { get; set; }
        public bool parallel { get; set; }
        public int monkeys { get; set; }
        public int length { get; set; }
        public int crossover { get; set; }
        public int mutation { get; set; }
        public int limit { get; set; }
        public override string ToString()
        {
            return $"{{{id}, {parallel}, {monkeys}, {length}, {crossover}, {mutation}, {limit}}}";
        }

    }
    public class TopRequest
    {
        public int id { get; set; }
        public int loop { get; set; }
        public int score { get; set; }
        public string genome { get; set; }
        public override string ToString()
        {
            return $"{{{id}, {loop}, {score}, {genome}}}";
        }
    }

    public class AssessRequest
    {
        public int id { get; set; }
        public List<string> genomes { get; set; }
        public override string ToString()
        {
            return $"{{{id}, #{genomes.Count}}}";
        }
    }

    public class AssessResponse
    {
        public int id { get; set; }
        public List<int> scores { get; set; }
        public override string ToString()
        {
            return $"{{{id}, #{scores.Count}}}";
        }
    }

    namespace Monkeys
    {
        using Carter;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddCarter();
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseRouting();
                app.UseEndpoints(builder => builder.MapCarter());
            }
        }
    }

    namespace Monkeys
    {
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.Extensions.Hosting;
        using Microsoft.Extensions.Logging;

        public class Program
        {
            public static void Main(string[] args)
            {
                //          var host = Host.CreateDefaultBuilder (args)
                //              .ConfigureWebHostDefaults (webBuilder => webBuilder.UseStartup<Startup>())

                var urls = new[] { "http://localhost:8081" };

                var host = Host.CreateDefaultBuilder(args)

                    .ConfigureLogging(logging =>
                    {
                        logging
                            .ClearProviders()
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Warning);
                    })

                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseUrls(urls);  // !!!
                    })

                    .Build();

                System.Console.WriteLine($"..... starting on {string.Join(", ", urls)}");
                host.Run();
            }
        }
    }

}