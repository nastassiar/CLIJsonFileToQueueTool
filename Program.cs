
namespace hii.automation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        static public IConfiguration Configuration { get; set; }
        
        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            if (null == args || 0 >= args.Length)
            {
                UnknownCommands();
                return;
            }

             if (args.Length < 1)
            {
                Console.WriteLine("Arguments should be sent in the following order: <Path_To_Json_File>");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            for (int i=0;i<args.Length;i++)
            {
                Console.WriteLine("\narg " + i.ToString() +":" + args[i]);
            }
            var filePath = args[1];
            Console.WriteLine(File.Exists(filePath) ? "File exists." : "File does not exist.");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist. Please check "+filePath);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            Job job = null;
            using (StreamReader r = new StreamReader(filePath))
            {
                string jsonInput = r.ReadToEnd();
                job = JsonConvert.DeserializeObject<Job>(jsonInput);
            }
            // Check to make sure the input is valid!
            if (job == null)
            {
                Console.WriteLine("Job was null, please check the input : "+filePath);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            // Make sure the customer name is included!
            if (String.IsNullOrEmpty(job.customer))
            {
                Console.WriteLine("Customer Name is required. Please edit file and rerun.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            // Make sure the customer name fits the paramters 
            if (job.customer.Length > 14 || job.customer.Length < 2 || !job.customer.All(char.IsLetterOrDigit))
            {
                Console.WriteLine("Customer name must be between 2 and 14 characters and only contain alpha numeric characters.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            string inputAccountName = Configuration["accountName"];
            string inputAccountKey = Configuration["accountKey"];
            string queueName = Configuration["queueName"];
            if (string.IsNullOrEmpty(inputAccountName) || string.IsNullOrEmpty(inputAccountKey) || string.IsNullOrEmpty(queueName))
            {
                Console.WriteLine("Fill in the accountName, accountKey and queueName in the appsettings.json file.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }

            if (String.IsNullOrEmpty(job.jobType))
            {
                Console.WriteLine("Job type must be newcustomer, med or geo. Cannot be empty.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            else 
            {
                job.jobType = job.jobType.ToLower();
                if (!(new[] {"geo", "med", "newcustomer"}.Contains(job.jobType)))
                {
                    Console.WriteLine("Job type must be newcustomer, med or geo");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey(true);
                    return;
                }
                else if (job.jobType == "newcustomer")
                {
                    Console.WriteLine("Creating new customer: "+job.customer);
                }
                else
                {
                    // If GEO or MED Need the project name
                    if (String.IsNullOrEmpty(job.project) || job.project.Length > 14 || job.project.Length < 2 || !job.project.All(char.IsLetterOrDigit))
                    {
                        Console.WriteLine("Project name must be between 2 and 14 characters and only contain alpha numeric characters.");
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey(true);
                        return;
                    }
                    Console.WriteLine("Starting new job for project "+job.project+" for customer "+job.customer);
                }
            }
            if (String.IsNullOrEmpty(job.region))
            {
                Console.WriteLine("Job region must be EU, or US. Cannot be empty");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            else 
            {
                job.region = job.region.ToLower();
                if (!(new[] {"us", "eu"}.Contains(job.region)))
                {
                    Console.WriteLine("Job region must be EU, or US");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey(true);
                    return;
                }
                else
                {
                    Console.WriteLine("Job region set to :"+job.region);
                }
            }

            // TODO: Add check for Parameters based on the job type
            
            //Send Message to Queue
            CloudQueue queue = CreateQueueAsync(inputAccountName,inputAccountKey, queueName).Result;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(job);
            InsertMessage(queue,json).Wait();

            Console.WriteLine("Running project task.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static void UnknownCommands()
        {
            Console.WriteLine("Invalid arguments.");
            Console.WriteLine("\t'customer' or 'c': create new customer");
            Console.WriteLine("\t'project' or 'p': run new project");
        }

        private static async Task<CloudQueue> CreateQueueAsync(string accountName, string accountKey, string queueName)
        {
            string myAccountName, myAccountKey, myQueueName;

            myAccountName = accountName;
            myAccountKey = accountKey;
            myQueueName = queueName;

            StorageCredentials storageCredentials = new StorageCredentials(myAccountName, myAccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            Console.WriteLine("\nCreating queue...");
            CloudQueue queue = queueClient.GetQueueReference(myQueueName);
            try
            {
                await queue.CreateIfNotExistsAsync();
            }
            catch (StorageException ex)
            {
                Console.WriteLine("Failed to create queue!" + "Error: " + ex.ToString());
                Console.ReadLine();
                throw;
            }

            return queue;
        }

        private static async Task InsertMessage(CloudQueue queue, string message)
        {
            Console.WriteLine("\nAdding Message...");
            try
            {
                await queue.AddMessageAsync(new CloudQueueMessage(message));
            }
            catch(StorageException ex)
            {
                Console.WriteLine("Failed to add Message:" + ex.ToString());
                throw;
            }
            Console.WriteLine("\nSuccessfully added Queue Message!");
        }
    }
}