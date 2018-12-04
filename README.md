# Automation
CLI tool for pushing a json file to a queue in Azure.


*Before starting fill in the appsettings.json file with the storage account name and storage account key and queue name. All this info can be found in the [azure portal](https://portal.azure.com)*

[Info about how to find your storage keys](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-manage)

## Commands
### Docker

There is a dockerfile created to build and run a docker container with the application inside. Because the program relies on a JSON file you have to mount the file drive to the container. The following commands worked for windows there may be slight adjustments for Linux.

Check out [this link](https://docs.docker.com/storage/volumes/) for guidance.

Or check out [this helpful blog](https://rominirani.com/docker-on-windows-mounting-host-directories-d96f3f056a2c) to figure out how to do it for Windows. 

#### Docker Build Command
Start by building the docker container

```
docker build . -t hii
```
You can include additonal tags. I choose just to include hii

#### Docker Run command
After you've built the container mount the drive and run it. 

```
 docker run -it -v <local_path_to_json_file_folder>:<mount_folder> hii "<mount_path_to_json_file>"
```
Example:
```
 docker run -it -v c:/SourceFiles/HyperSpectral/automation/samplejson:/config hii "/config/project-geo.json"
```

### DotNet
To run the program run directly from the command line run the following command from within the automation folder.
```
dotnet run hii <path_to_json_file>
```

#### dotnet: command not found 
If you get this error it is because dotnet is not installed

[Instructions here for Linux subsystem on Windows](https://weblog.west-wind.com/posts/2017/Apr/13/Running-NET-Core-Apps-under-Windows-Subsystem-for-Linux-Bash-for-Windows)

[Instructions here for Installing on Windows, Linux and MacOs](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial#linuxubuntu)

## JSON File Format

The Json file you pass in must be in a specific format otherwise you will get errors. Some fields are required depending on the type of job you are running. See the specific sections for details. 

Overall the format is:

```
{
    "jobType": "string",
    "project": "string",
    "customer": "string",
    "region":"string",
    "arguments": {
        "app": "string",
        "process": "string",
        "inputs": "string",
        "sensor": "string",
        "basename": "string",
        "depths": "string",
        "sample": "string",
        "stain": "string"
    }
}
```
This format is dictated by the Objects.cs file. 
To add new arguments go to the Objects.cs file and add a line following the existing format.
For example adding:
> public string testarg;
would add a new argument called testarg you could now specify in the JSON file. 

```
public class Job
    {
        public string jobType;
        public Params arguments;
        public string customer;
        public string region;
        public string project;
    }
    public class Params
    {
        public string app; 
        public string process;
        public string inputs;

        // GEO Specific Params
        public string sensor;
        public string basename;
        public string depths;
        
        // MED Specific Params
        public string sample;
        public string stain;
    }
```
*Note there is no validation on the arguments json*

### New Customer
To see a sample of the new customer data look at new-customer.json for the schema.
The jobType field must be set to "newcustomer" to kick off a new customer job. 
Customer, and Region are also required fields.
Region can be either EU or US.
Customer name must be between 3 and 14 characters and alphanumberic.
```
dotnet run hii samplejson\new-customer.json
```

### Geo Job
To see a sample of the new customer data look at project-geo.json for the schema.
The jobType field must be set to "geo" to kick off a new customer job. 
Customer, Project and Region are also required fields.
Region can be either EU or US.
Customer and project names must be between 3 and 14 characters and alphanumberic.
```
dotnet run hii samplejson\project-geo.json
```

### Med Job
To see a sample of the new customer data look at project-geo.json for the schema.
The jobType field must be set to "med" to kick off a new customer job. 
Customer, Project and Region are also required fields.
Region can be either EU or US.
Customer and project names must be between 3 and 14 characters and alphanumberic.
```
dotnet run hii samplejson\project-med.json
```