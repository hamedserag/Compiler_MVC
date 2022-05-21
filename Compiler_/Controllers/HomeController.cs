using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Compiler_.Models;

namespace Compiler_.Controllers
{
    public class HomeController : Controller
    {
        DB db = new DB();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult Add(HttpPostedFileBase img)
        {
            ViewBag.reader = true;
            if (img != null)
            {
                img.SaveAs(Server.MapPath("~/Content/Resources/" + img.FileName));
                FileDb f = new FileDb();
                f.Id = db.FileDbs.ToList().Count;
                f.FileDirectory = "~/Content/Resources/" + img.FileName;
                f.isServerGen = 0;
                db.FileDbs.Add(f);
                db.SaveChanges();
            }
            else
            {
                ViewBag.error = "null";
            }
            return RedirectToAction("Reader");
        }
        public ActionResult TxtInput()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult TxtInput(string input)
        {
            ViewBag.reader = true;
            var path = Server.MapPath("~/Content/Resources/Input.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(input);
                sw.Close();
            }
            FileDb f = new FileDb();
            f.Id = db.FileDbs.ToList().Count;
            f.FileDirectory = path;
            f.isServerGen = 1;
            db.FileDbs.Add(f);
            db.SaveChanges();
            return RedirectToAction("Reader");
        }
        public ActionResult Reader()
        {
            List<FileDb> files = db.FileDbs.ToList();
            string path = files[files.Count - 1].FileDirectory;
            StreamReader reader;
            if (files[files.Count - 1].isServerGen == 0)
            {
                reader = System.IO.File.OpenText(Server.MapPath(path));
            }
            else
            {
                reader = System.IO.File.OpenText(System.IO.Path.GetFullPath(path));
            }
            List<string> tokens = createTokens(reader);

            int line = 0;
            foreach (var lextoken in tokens)
            {
                if (lextoken == "_$")
                {
                    line++;
                }
                else
                {
                    if (lextoken == "")
                        continue;
                    if (isRule(lextoken, line))
                    {
                        // print rule name
                    }
                    else if (isConstant(lextoken))
                    {
                        Console.WriteLine("line: " + line + " █ " + lextoken + " → " + "Contstant");
                        ViewBag.tokens += "line: " + line + " █ " + lextoken + " → " + "Contstant <br>";
                    }
                    else if (isIdentifier(lextoken))
                    {
                        Console.WriteLine("line: " + line + " █ " + lextoken + " → " + "Identifier");
                        ViewBag.tokens += "line: " + line + " █ " + lextoken + " → " + "Identifier <br>";
                    }
                    else
                    {
                        Console.WriteLine("lexical error at: " + lextoken);
                    }
                }

            }
            reader.Close();
            return View();
        }
        public static IDictionary<string, int> TOKENS = new Dictionary<string, int>();
        public static Dictionary<string, string> RULES = new Dictionary<string, string>()
    {
      {"Type","Class" },
      {"Infer","Inheritance" },
      {"If","Condition" },
      {"Else ","Condition" },
      {"Ipok","Integer" },
      {"Sipok","SInteger" },
      {"Craf","Character" },
      {"Sequence","String" },
      {"Ipokf","Float" },
      {"Sipokf","SFloat" },
      {"Valueless","Void" },
      {"Rational","Boolean" },
      {"Endthis","Break" },
      {"However","Loop" },
      {"When","Loop" },
      {"Respondwith","Return" },
      {"Srap","Struct" },
      {"Scan","Switch" },
      {"Conditionof","Switch" },
      {"Require","Inclusion" },
      {"@","Start Symbol" },
      {"^","Start Symbol" },
      {"$","End Symbol" },
      {"# ","End Symbol" },
      {"+","Arithmetic Operation" },
      {"-","Arithmetic Operation" },
      {"*","Arithmetic Operation" },
      {"/","Arithmetic Operation" },
      {"&&","Logic operators" },
      {"||","Logic operators" },
      {"~","Logic operators" },
      {"==","relational operators" },
      {"<","relational operators" },
      {">","relational operators" },
      {"!=","relational operators" },
      {"<=","relational operators" },
      {">=","relational operators" },
      {"=","Assignment operator" },
      {"->","Access Operator" },
      {"{","Braces" },
      {"}","Braces" },
      {"[","Braces" },
      {"]","Braces" },
      {",","Quotation Mark" },
      {";","SemiColon"}
    };

        public static List<string> createTokens(StreamReader sr)
        {
            int line = 0;
            string str;
            string temp;
            string token = "";
            bool flag = false;
            var tokens = new List<string>();
            while (!sr.EndOfStream)
            {
                line++;
                //Console.WriteLine("Line = :" + line);

                str = sr.ReadLine();
                temp = line + ": " + str;
                //Console.WriteLine(temp);
                // Console.WriteLine("line length" + str.Length);
                tokens.Add("_$");
                for (int i = 0; i < str.Length; i++)
                {
                    //for block comment
                    if (str[i] == '<' && str[i + 1] == '/')
                    {
                        Console.WriteLine(str[i] + " start of block comment " + str.Length);
                        flag = true;
                        break;
                    }
                    if (flag && str[i] != '/')
                    {
                        //str.EndsWith('>');
                        continue;
                    }
                    if (flag && i == str.Length - 1 && flag && str[i] == '/')
                    {
                        break;
                    }
                    if (flag && str[i] == '/' && str[i + 1] == '>')
                    {
                        i++;
                        flag = false;
                        continue;
                    }
                    // check for line comment
                    if (str[i] == '*' && str[i + 1] == '*' && str[i + 2] == '*')
                    {
                        Console.WriteLine(str[i] + "start of line comment");
                        break;
                    }


                    if (str[i] == ' ' || str[i] == '\n')
                    {
                        //TOKENS.Add(new KeyValuePair<string, int>(token, line));
                        tokens.Add(token);
                        token = "";
                    }
                    else
                    {
                        //Console.WriteLine(str[i]);
                        if (i != str.Length - 1 && isDoubleOperator(str[i], str[i + 1]))
                        {
                            //TOKENS.Add(new KeyValuePair<string, int>(token, line));
                            tokens.Add(token);
                            token = "";
                            token += str[i];
                            token += str[i + 1];
                            i++;
                            tokens.Add(token);
                            //TOKENS.Add(new KeyValuePair<string, int>(token, line));
                            token = "";
                        }
                        else if (isSingleOperator(str[i]))
                        {
                            //TOKENS.Add(new KeyValuePair<string, int>(token, line));
                            tokens.Add(token);
                            token = "";
                            token += str[i];
                            tokens.Add(token);
                            //TOKENS.Add(new KeyValuePair<string, int>(token, line));
                            token = "";
                        }
                        else
                            token += str[i];
                    }

                }
                tokens.Add(token);
                //TOKENS.Add(new KeyValuePair<string, int>(token, line));
                token = "";

            }
            return tokens;
        }

        // check for identifier
        public static Boolean isIdentifier(string token)
        {

            foreach (char c in token)
            {
                if (!(c >= 'A' && c <= 'Z') &&
                    !(c >= 'a' && c <= 'z') &&
                    !(c >= '0' && c <= '9') &&
                    !(c == '_'))
                {
                    return false;
                }
            }
            return true;
        }

        // check for check for rules
        public Boolean isRule(string token, int line)
        {
            foreach (var rule in RULES)
            {
                if (token == rule.Key)
                {
                    Console.WriteLine("line: " + line + " █ " + token + "→" + rule.Value);
                    ViewBag.tokens += "line: " + line + " █ " + token + " → " + rule.Value + " <br>";
                    return true;
                }
            }
            return false;
        }
        // check for constants
        public static Boolean isConstant(string token)
        {
            double num = 0;
            if (Double.TryParse(token, out num))
            {
                return true;
            }
            return false;
        }
        public static Boolean isSingleOperator(char str)
        {
            string snglop = "";
            snglop += str;
            List<string> singleop = new List<string>()
       {
         "@","^","$","#","+","-","*","/","~","<",">","{","}", "[","]",",",";","=","~"
       };

            if (singleop.Contains(snglop))
            {
                return true;
            }
            return false;
        }

        public static Boolean isDoubleOperator(char str1, char str2)
        {
            string dblop = "";
            dblop += str1;
            dblop += str2;
            List<string> doubleop = new List<string>()
      {
        "&&", "||", "==","!=","<=", ">=","->"
      };
            if (doubleop.Contains(dblop))
            {
                return true;
            }
            return false;
        }
    }
}
