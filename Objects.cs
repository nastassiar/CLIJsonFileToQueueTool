using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace hii.automation
{
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
}