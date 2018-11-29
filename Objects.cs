using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace hii.automation
{
    public class Job
    {
        // Commenting out to change to string to simplfy
        //public JobType? type;
        public string jobType;
        public Params arguments;
        public string customer;
         // Commenting out to change to string to simplfy
        //public Region region;
        public string region;
        public string project;
    }
    /* 
    // Commenting out to change to string to simplfy
    public enum JobType : byte
    {
        [EnumMember(Value = "med")] 
        MED = 0,
        [EnumMember(Value = "geo")] 
        GEO = 1,
        [EnumMember(Value = "newcustomer")] 
        NEWCUSTOMER = 2,
    }
    public enum Region : byte
    {
        [EnumMember(Value = "us")] 
        US = 0,
        [EnumMember(Value = "eu")] 
        EU = 1,
    }*/
    public class Params
    {
        public string usecase,
                        process,
                        inputs;
    }
    public class ParamsGeo: Params
    {
        public string sensor,
                        basename,
                        depths;

    }
    public class ParamsMed: Params
    {
        public string sample,
                        stain;
    }
}