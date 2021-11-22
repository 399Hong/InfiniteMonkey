using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorSignalRApp.Shared
{

    public class TryRequest {
        public int id { get; set; }
        public bool parallel { get; set; }
        public int monkeys { get; set; }
        public int length { get; set; }
        public int crossover { get; set; }
        public int mutation { get; set; }
        public int limit { get; set; }
        public override string ToString () {
            return $"{{{id}, {parallel}, {monkeys}, {length}, {crossover}, {mutation}, {limit}}}";
        }
    }
    
    public class TopRequest {
        public int id { get; set; }
        public int loop { get; set; }
        public int score { get; set; }
        public string genome { get; set; }
        public override string ToString () {
            return $"ID:{id} Generation: {loop} Score:{score} Text: {genome}";
        }  
    }  
        public class TargetRequest {
        public int id { get; set; }
        public bool parallel { get; set; }
        public string target { get; set; }
        public override string ToString () {
            return $"{{{id}, {parallel}, \"{target}\"}}";
        }  
    }    

}
