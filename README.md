# Automation
CLI tool for pushing a json file to a queue in Azure.

*Before starting fill in the appsettings.json file with the storage account name and storage account key of the storage account where the queue lives.*

# Commands
## DotNet
To run the program run the following command from within the automation folder.
```
dotnet run hii <path_to_json_file>
```

### Run
#### New Customer
To see a sample of the new customer data look at new-customer.json for the schema.
The jobType field must be set to "newcustomer" to kick off a new customer job. 
Customer, and Region are also required fields.
```
dotnet run hii new-customer.json
```

#### Kick off Geo Job
To see a sample of the new customer data look atproject-geo.json for the schema.
The jobType field must be set to "geo" to kick off a new customer job. 
Customer, Project and Region are also required fields.
Region can be either EU or US.
Customer and project names must be between 2 and 14 characters and alphanumberic.
```
dotnet run hii project-geo.json
```

#### Kick off Med Job
To see a sample of the new customer data look at project-geo.json for the schema.
The jobType field must be set to "med" to kick off a new customer job. 
Customer, Project and Region are also required fields.
Region can be either EU or US.
Customer and project names must be between 2 and 14 characters and alphanumberic.
```
dotnet run hii project-med.json
```