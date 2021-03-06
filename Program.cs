﻿
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
    using Microsoft.Extensions.Configuration.Json;

    public class Program
    {
        static public IConfiguration Configuration { get; set; }
        
        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            if (null == args || 0 >= args.Length || args.Length < 1)
            {
                Console.WriteLine("Arguments should be sent in the following order: <Path_To_Json_File>");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            var filePath = args[0];
            // Adding in check for filepath to account for docker not counting tag as an argument
            string extension = Path.GetExtension(filePath);
            if (String.IsNullOrEmpty(extension) && args.Length >= 2)
            {
                filePath =  args[1];
            }
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist. Please check "+filePath+ " exists and rerun.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            Console.WriteLine("\nPulling data from "+filePath+ "...");
            Job job = null;
            using (StreamReader r = new StreamReader(filePath))
            {
                string jsonInput = r.ReadToEnd();
                job = JsonConvert.DeserializeObject<Job>(jsonInput);
            }
            // Check to make sure the input is valid!
            if (job == null)
            {
                Console.WriteLine("Job was null, please check the input : "+filePath + " and rerun.");
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
            if (job.customer.Length > 14 || job.customer.Length < 3 || !job.customer.All(char.IsLetterOrDigit))
            {
                Console.WriteLine("Customer name must be between 3 and 14 characters and only contain alpha numeric characters.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            job.customer = job.customer.ToLower();
            if (Configuration == null)
            {
                Console.WriteLine("SET TO NULL FOR TESTING.");
                return;
            }
            string inputAccountKey = Configuration["accountKey"]; 
            string inputAccountName = Configuration["accountName"];
            string queueName = Configuration["queueName"];

            if (string.IsNullOrEmpty(inputAccountName) || string.IsNullOrEmpty(inputAccountKey) || string.IsNullOrEmpty(queueName))
            {
                Console.WriteLine("Fill in the accountName, accountKey and queueName in the appsettings.json file.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
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
                    Console.WriteLine("\nCreating new customer " + job.customer+" in "+job.region+" region...");
                }
                else
                {
                    // If GEO or MED Need the project name
                    if (String.IsNullOrEmpty(job.project) || job.project.Length > 14 || job.project.Length < 3 || !job.project.All(char.IsLetterOrDigit))
                    {
                        Console.WriteLine("Project name must be between 3 and 14 characters and only contain alpha numeric characters.");
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey(true);
                        return;
                    }
                    job.project = job.project.ToLower();
                    Console.WriteLine("\nStarting new "+job.jobType+" job for project " + job.project+" for customer "+job.customer+" in "+job.region+" region...");
                }
            }
            

            // TODO: Add check for Parameters based on the job type
            
            //Send Message to Queue
            CloudQueue queue = CreateQueueAsync(inputAccountName,inputAccountKey,queueName).Result;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(job);
            InsertMessage(queue,json).Wait();
            Console.WriteLine("\nGood to go! Press any key to exit!");
            Console.ReadKey(true);
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