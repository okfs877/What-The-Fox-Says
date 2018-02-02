using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using multilib.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

//google cloud natural language api import statements
using Google.Cloud.Language.V1;
using static Google.Cloud.Language.V1.AnnotateTextRequest.Types;
using Google.Protobuf.Collections;

namespace multilib.Controllers
{
    public class LibController : Controller
    {
        public Random rand = new Random();
        public List<string> textList =new List<string>();
        private LibContext _context;
        public LibController(LibContext context)
        {
            _context = context;
        }

        // [START analyze_entities_from_string]
        private static void AnalyzeEntitiesFromText(string text){
            var client = LanguageServiceClient.Create();
            var response = client.AnalyzeEntities(new Document(){
                Content = text,
                Type = Document.Types.Type.PlainText
            });
            //WriteEntities(response.Entities);
        }

        private static void WriteEntities(IEnumerable<Entity> entities){
            Console.WriteLine("Entities:");
            foreach (var entity in entities){
                Console.WriteLine($"\tName: {entity.Name}");
                Console.WriteLine($"\tType: {entity.Type}");
                Console.WriteLine($"\tSalience: {entity.Salience}");
                Console.WriteLine("\tMentions:");
                foreach (var mention in entity.Mentions)
                    Console.WriteLine($"\t\t{mention.Text.BeginOffset}: {mention.Text.Content}");
                Console.WriteLine("\tMetadata:");
                foreach (var keyval in entity.Metadata){
                    Console.WriteLine($"\t\t{keyval.Key}: {keyval.Value}");
                }
            }
        }
        // [END analyze_entities_from_string]

        // [START analyze_syntax_from_string]
        public RepeatedField<Token> AnalyzeSyntaxFromText(string text){
            var client = LanguageServiceClient.Create();
            var response = client.AnnotateText(new Document(){
                Content = text,
                Type = Document.Types.Type.PlainText
            },
            new Features() { ExtractSyntax = true });
            //WriteSentences(response.Sentences, response.Tokens);
            return response.Tokens;
        }
        // [END analyze_syntax_from_string]

        private static void WriteSentences(IEnumerable<Sentence> sentences, RepeatedField<Token> tokens){
            Console.WriteLine("Sentences:");
            foreach (var sentence in sentences){
                Console.WriteLine($"\t{sentence.Text.BeginOffset}: {sentence.Text.Content}");
            }
            Console.WriteLine("Tokens:");
            foreach (var token in tokens){
                Console.WriteLine($"\t{token.PartOfSpeech.Tag} "
                    + $"{token.Text.Content}");
            }
        }

        // GET: /Home/
        [HttpGet]
        [Route("dashboard")]
        public IActionResult Index(){
            if(HttpContext.Session.GetInt32("id")==null){//ensures logged in user
                return RedirectToAction("Index", "Home");
            }
            ViewBag.libs = _context.Libs;
            ViewBag.curUser = _context.Users.SingleOrDefault(a=>a.id == HttpContext.Session.GetInt32("id"));
            return View();
        }

        [HttpGet]
        [Route("dashboard/random")]
        public IActionResult Random(){
            int randomId = _context.Libs.ToList()[rand.Next(0, _context.Libs.ToList().Count)].Id;
            return RedirectToAction("Play", new {id=randomId});
        }

        [HttpGet]
        [Route("dashboard/play/{id}")]
        public IActionResult Play(int id){
            ModelLib lib = _context.Libs.Where(a=>a.Id ==id).First();
            textList = new List<string>();
            //mutator has a scale of 1-5 with 5 replacing the most and 1 replacing the least nouns, verbs, adverbs, adjectives, and numbers
            RepeatedField<Token> tokens = AnalyzeSyntaxFromText(lib.Story1 + lib.Story2 + lib.Story3 + lib.Story4 + lib.Story5 + lib.Story6 + lib.Story7 + lib.Story8);
            List<string> inputs = new List<string>();
            foreach (var token in tokens){
                if(token.PartOfSpeech.Proper.ToString() == "Proper" || rand.Next(0,9) < lib.Mutator){//handle mutator check
                    if(token.PartOfSpeech.Tag.ToString() == "Noun"){
                        textList.Add("*-*");
                        if(token.PartOfSpeech.Number.ToString() == "Proper"){//proper nouns here
                            inputs.Add("Proper Noun");
                        } else if(token.PartOfSpeech.Number.ToString() == "Plural"){//plural noun
                            inputs.Add("Plural Noun");
                        } else{
                            inputs.Add("Noun");
                        }
                    } else if(token.PartOfSpeech.Tag.ToString() == "Verb"){
                        textList.Add("*-*");
                        if(token.PartOfSpeech.Tense.ToString() == "Present"){//tense
                            inputs.Add("Verb ending in -ing");
                        } else if(token.PartOfSpeech.Tense.ToString() == "Past"){
                            inputs.Add("Verb ending in -ed");
                        } else{
                            inputs.Add("Verb");
                        }
                    } else if(token.PartOfSpeech.Tag.ToString() == "Num"){
                        inputs.Add("Number");
                        textList.Add("*-*");
                    } else if(token.PartOfSpeech.Tag.ToString() == "Adv"){
                        inputs.Add("Adverb");
                        textList.Add("*-*");
                    } else if(token.PartOfSpeech.Tag.ToString() == "Adj"){
                        inputs.Add("Adjective");
                        textList.Add("*-*");
                    } else {
                        textList.Add(token.Text.Content);
                    }
                }else{
                    textList.Add(token.Text.Content);
                }
            }
            ViewBag.inputs = inputs;
            string astory ="";
            foreach (var item in textList)
            {
                astory+= item + " ";
            }
            ViewBag.lib = lib;
            HttpContext.Session.SetString("story", astory);
            return View();
        }

        [HttpPost]
        [Route("dashboard/process")]
        public IActionResult Process(List<string> inputs, int id){
            string[] textList = HttpContext.Session.GetString("story").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string story ="";
            int count =0;
            foreach(var word in textList){
                if(word == "*-*"){
                    story+= inputs[count] + " ";
                    count++;
                } else{
                    story += word + " ";
                }
            }
            ViewBag.story = story;
            ViewBag.lib = _context.Libs.Where(a=>a.Id ==id).First();
            return View("Result");
        }

        [HttpGet]
        [Route("dashboard/view/{id}")]
        public IActionResult Show(int id){
            ModelLib lib = _context.Libs.Where(a=>a.Id ==id).First();
            ViewBag.lib = lib;
            return View();
        }

        [HttpGet]
        [Route("dashboard/new")]
        public IActionResult New(){
            return View();
        }

        [HttpPost]
        [Route("dashboard/create")]
        public IActionResult Create(Lib newLib){
            string story1= newLib.Story;
            string story2="";
            string story3="";
            string story4="";
            string story5="";
            string story6="";
            string story7="";
            string story8="";
            if(newLib.Story.Length>255){
                story2= newLib.Story.Substring(256);
            }
            if(newLib.Story.Length>255*2){
                story3 = newLib.Story.Substring(255*2+1);
            }
            if(newLib.Story.Length>255*3){
                story4= newLib.Story.Substring(255*3+1);
            }
            if(newLib.Story.Length>255*4){
                story5 = newLib.Story.Substring(255*4+1);
            }
            if(newLib.Story.Length>255*5){
                story6 = newLib.Story.Substring(255*5+1);
            }
            if(newLib.Story.Length>255*6){
                story7 = newLib.Story.Substring(255*6+1);
            }
            if(newLib.Story.Length>255*7){
                story8 = newLib.Story.Substring(255*7+1);
            }
            if(ModelState.IsValid){
                ModelLib aLib = new ModelLib{
                Title = newLib.Title,
                Story1 = story1,
                Story2 = story2,
                Story3 = story3,
                Story4 = story4,
                Story5 = story5,
                Story6 = story6,
                Story7 = story7,
                Story8 = story8,
                Mutator = newLib.Mutator
                };
                _context.Libs.Add(aLib);
                _context.SaveChanges();
                return View("Play", (int)_context.Libs.ToList().Last().Id);
            }
            return View("New");
        }
    }
}