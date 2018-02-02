using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
 
namespace multilib.Models
{
    public class Lib
    {
        [Required]
        [Display(Name="Title")]
        [MinLength(5)]
        [MaxLength(45)]
        public string Title {get;set;}

        [Required]
        [Display(Name="Story")]
        [MinLength(50)]
        [MaxLength(2000)]
        public string Story {get;set;}

        [Required]
        [Display(Name="Mutator")]
        public int Mutator {get;set;}
    }
    public class ModelLib{
        public int Id {get;set;}
        public string Title {get;set;}
        public string Story1 {get;set;}
        public string Story2 {get;set;}
        public string Story3 {get;set;}
        public string Story4 {get;set;}
        public string Story5 {get;set;}
        public string Story6 {get;set;}
        public string Story7 {get;set;}
        public string Story8 {get;set;}
        public int Mutator {get;set;}
    }
}