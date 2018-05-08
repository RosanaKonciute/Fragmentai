using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XESApdorojimas
{
    class Program
    {

        //XML elementu informacija 
        private static XNamespace ns = "http://www.xes-standard.org/";
        private static XName egzemplioriausElPav = ns + "trace";
        private static XName ivykioElPav = ns + "event";
        private static XName stringElementoPav = ns + "string";
        private static string keyAtributoPav = "key";
        private static string vardoAtributoReiksme = "concept:name";
        private static string valueAtributoPav = "value";

        //ISTRINTI COMMAND LINE ARGUMENTS IN PROJEKTO->RMB->PROPERTIES->DEBUG
        static void Main(string[] args)
        {


            //drag-droppinami failai yra kaip argumentai programai
            foreach (var filename in args)
            {
                //uzkraunam XML faila
                XDocument dok = null;
                try
                {
                    dok = XDocument.Load(filename);
                }
                catch (Exception exc)
                {
                    Pause("Nepavyko nuskaityt.\r\n{0}", exc.ToString());
                    return;
                }

                //cia bus suagomi rezultatai
                //kiekvienas masyvo elementas bus formatu "{egzemplioriausId}\t{ivykiopavadinimas}" (\t yra tab'as)
                List<string> result = new List<string>();

                //nuskaitom visus egzempliorius
                var egzemplioriai = dok.Descendants(egzemplioriausElPav);

                //iteruojam pro visus egzempliorius
                foreach (var egzempliorius in egzemplioriai)
                {
                    //isgaunam egzemplioriaus pavadinima
                    string egzemplioriausPavadinimas = IsgaukElementoPavadinima(egzempliorius);



                    #region apdorojam kiekviena ivyki

                    var ivykiuElementai = egzempliorius.Elements(ivykioElPav);

                    foreach (var ivykioElementas in ivykiuElementai)
                    {
                        //isgaunam ivykio pavadinima
                        var ivykioPavadinimas = IsgaukElementoPavadinima(ivykioElementas);

                        //sukuriam  "{egzemplioriausId}{ivykiopavadinimas}"
                        var rezultatas = String.Format("{0}\t{1}", egzemplioriausPavadinimas, ivykioPavadinimas);

                        //pridedam ivykio rezultata prie bendro rezultatu masyvo
                        result.Add(rezultatas);
                    }
                    #endregion
                }

                //issaugomo failo pavadinimas bus toks pat kaip zurnalo failo, tik baigsis .csv galu
                var failoPav = String.Format("{0}.txt", Path.GetFileNameWithoutExtension(filename));
                //sukuriam zurnalo failo rezultatu irasymo srauta
                using (StreamWriter sw = new StreamWriter(failoPav))
                {
                    //irasom i rezultatu faila kiekviena is rezultatu eilute
                    foreach (var eilute in result)
                    {
                        sw.WriteLine(eilute);
                    }
                }


                var in0 = File.ReadAllLines(failoPav).ToList();
                //var in0 = new List<string>() {"aaa\tbbb", "AA\tBB", "aaa\tccc"};

                //list of tuple<string,string>
                var out0 = in0.Select(d => d.Split('\t')).Select(d => Tuple.Create(d[0], d[1])).ToList();
                // kiek yra elementu 
                int IsVisoElementu = out0.Count;
                int a = IsVisoElementu - 1;

                //unikaliu vardu aibe
                List<string> unikalusVardai = new List<string>();
                for (var j = 0; j < a; j++)
                {
                    if (!unikalusVardai.Contains(out0[j].Item2)) {
                        unikalusVardai.Add(out0[j].Item2);
                    }
                }
                Console.WriteLine("Unikalus ivykiu vardai zurnale \n");
                for (var j=0;j<unikalusVardai.Count();j++)
                {
                Console.WriteLine(unikalusVardai[j]+"\n");
                }
                Console.WriteLine("Is viso yra " + unikalusVardai.Count() + " unikaliu vardu \n");

                //unikaliu egzemplioriu aibe
                List<string> unikalusEgzemplioriai = new List<string>();
                for (var x = 0; x < a; x++)
                {
                    if (!unikalusEgzemplioriai.Contains(out0[x].Item1))
                    {
                        unikalusEgzemplioriai.Add(out0[x].Item1);
                    }
                }
                Console.WriteLine("Unikalus egzemplioriai zurnale \n");
                for (var x = 0; x < unikalusEgzemplioriai.Count(); x++)
                {
                    Console.WriteLine(unikalusEgzemplioriai[x] + "\n");
                }
                Console.WriteLine("Is viso yra " + unikalusEgzemplioriai.Count() + " unikaliu egzemplioriu \n");

                /*
                // visu veiklu vardu ciklai
                for (var j = 0; j < unikalusVardai.Count(); j++)
                {
                    Console.WriteLine(unikalusVardai[j] + " Veiklos ciklo atvejai zurnale: ");
                    var kintamasis = unikalusVardai[j];
                    double ciklas = 0;
                    for (var i = 0; i < a; i++)
                    {
                        if (out0[i].Item1 == out0[i + 1].Item1 && out0[i].Item2 == out0[i + 1].Item2 && out0[i].Item2.Contains(kintamasis))
                        {
                            ciklas++;
                            
                            Console.WriteLine("{0},{1}", out0[i], out0[i + 1] + "\n");
                        }
                    }
                    Console.WriteLine("Veiklos " + unikalusVardai[j] + " ciklas ivyko  " + ciklas + "  kartus" + "\n");
                    double CikloSantykis = ciklas / IsVisoElementu;
                    Console.WriteLine("Procentaliai " + unikalusVardai[j] + " veiklos ciklas  " + CikloSantykis + "\n");
                }
               
                */


                /*
                 // tiesiogine seka 
                 for (var j = 0; j < unikalusVardai.Count(); j++) {
                     for (var k = 0; k < unikalusVardai.Count(); k++) {
                     Console.WriteLine(unikalusVardai[j] + " ir " + unikalusVardai[k]+ " tiesiogines sekos atvejai \n");
                     double tiesioginis = 0;
                     for (var i = 0; i < a; i++) {

                         if (out0[i].Item1 == out0[i + 1].Item1 && out0[i].Item2.Contains(unikalusVardai[j]) && out0[i + 1].Item2.Contains(unikalusVardai[k])) {
                             tiesioginis++;

                             Console.WriteLine("{0},{1}", out0[i], out0[i + 1] + "\n");
                         }
                     }
                     Console.WriteLine("Po veiklos " + unikalusVardai[j] + " is karto seke veikla " + unikalusVardai[k] + " " + tiesioginis + "  kartus" + "\n");
                     double TiesioginesSantykis = tiesioginis / IsVisoElementu;
                     Console.WriteLine("Procentaliai po veiklos " + unikalusVardai[j] + " is karto seke  " + unikalusVardai[k] + "   " + TiesioginesSantykis + "\n");
                 }
                     }
                     
                 */


                //TITO PASIULYMAS
                Matrix priklausomybiuMatrica = new Matrix();

                //tuscia matrica uzpildome
                foreach (var ivykioPavadinimas in unikalusVardai)
                {
                    foreach(var ivykioPavadinimas2 in unikalusVardai)
                    {
                        priklausomybiuMatrica.GetElement(ivykioPavadinimas, ivykioPavadinimas2);
                    }
                }



                //sukuriame zurnalo reprezentacija objektais
                var tmp = out0[0];
                var zurnalas = new Log();
                var currEgz = new Trace() { Id = tmp.Item1 };
                zurnalas.Traces.Add(currEgz);
                for (var i = 1; i < out0.Count; i++)
                {
                    var tmp2 = out0[i];
                    if(tmp2.Item1 != currEgz.Id)
                    {
                        currEgz = new Trace() { Id = tmp2.Item1 };
                        zurnalas.Traces.Add(currEgz);
                    }
                    currEgz.Events.Add(new Event(){ Name= out0[i].Item2 });
                }



                //ieskome ciklu
                foreach (var egzempliorius in zurnalas.Traces)
                {
                    for (var i = 0; i < egzempliorius.Events.Count - 3; i++)
                    {
                        var pirmasIvykis = egzempliorius.Events[i].Name;
                        var antrasivykis = egzempliorius.Events[i + 1].Name;
                        var treciasIvykis = egzempliorius.Events[i + 2].Name;
                        var ketvirtasIvykis = egzempliorius.Events[i + 3].Name;


                        //cikliskumas abab
                        if (pirmasIvykis == treciasIvykis && antrasivykis == ketvirtasIvykis)
                        {
                            priklausomybiuMatrica.GetElement(pirmasIvykis, antrasivykis).ColumnCycling++;
                        }

                        //cikliskumas aa
                        if (pirmasIvykis == antrasivykis)
                        {
                            foreach(var unikalusVardas in unikalusVardai)
                            {
                                priklausomybiuMatrica.GetElement(pirmasIvykis, unikalusVardas).CycleCount++;
                            }
                        }
                    }
                }

                foreach (var egzempliorius in zurnalas.Traces)
                {
                    for (var i = 0; i < egzempliorius.Events.Count -1; i++)
                    {
                        var pirmasIvykis = egzempliorius.Events[i].Name;
                        var antrasivykis = egzempliorius.Events[i + 1].Name;

                        //cikliskumas aa
                        if (pirmasIvykis == antrasivykis)
                        {
                            foreach (var unikalusVardas in unikalusVardai)
                            {
                                priklausomybiuMatrica.GetElement(pirmasIvykis, unikalusVardas).CycleCount++;
                            }
                        }
                        //directly follows
                        priklausomybiuMatrica.GetElement(pirmasIvykis, antrasivykis).Follows++;
                    }
                }

             
                foreach (var egzempliorius in zurnalas.Traces)
                {
                    foreach (var unikalusVardas in unikalusVardai)
                    {
                        Event pirmas = null;
                        int pirmoidx = -1;
                        for (var i = 0; i < egzempliorius.Events.Count; i++)
                        {
                            if (egzempliorius.Events[i].Name == unikalusVardas)
                            {
                                pirmas = egzempliorius.Events[i];
                                pirmoidx = i;
                                break;
                            }
                        }
                        if (pirmas == null)
                        {
                            continue;
                        }

                        foreach (var unikalusVardas2 in unikalusVardai)
                        {
                            Event antras = null;
                            int antroidx = -1;
                            for (var i = 0; i < egzempliorius.Events.Count; i++)
                            {
                                if (egzempliorius.Events[i].Name == unikalusVardas)
                                {
                                    antras = egzempliorius.Events[i];
                                    antroidx = i;
                                    if (antroidx - pirmoidx > 1)
                                    {
                                        priklausomybiuMatrica.GetElement(unikalusVardas, unikalusVardas2).EventuallyFollows++;
                                    }
                                }
                            }

                        }
                       
                    }

                }

                foreach (var matrixElement in priklausomybiuMatrica.Elements)
                {
                    Console.WriteLine("{0}\t{1}\t{{{2},{3},{4}}}",matrixElement.Column,matrixElement.Row,matrixElement.CycleCount,matrixElement.ColumnCycling, matrixElement.Follows);
                    //Console.WriteLine("{0}\t{1}\t{2}", matrixElement.Column, matrixElement.Row, matrixElement.EventuallyFollows);
                }

                /*
                //ciklas dvieju veiklu
                for (var j = 0; j < unikalusVardai.Count()-1; j++)
                {
                    Console.WriteLine(unikalusVardai[j] + " ir " + unikalusVardai[j + 1] + " dvieju veiklu ciklo atvejai \n");
                    double DviejuCiklas = 0;
                    for (var i = 0; i < a-4; i++)
                    {
                        if (out0[i].Item1 == out0[i + 1].Item1 && out0[i + 1].Item1 == out0[i + 2].Item1 && out0[i + 2].Item1 == out0[i + 3].Item1 &&
                            out0[i].Item2.Contains(unikalusVardai[j]) && out0[i + 1].Item2.Contains(unikalusVardai[j + 1]) &&
                            out0[i + 2].Item2.Contains(unikalusVardai[j]) && out0[i + 3].Item2.Contains(unikalusVardai[j + 1]))
                        {
                            DviejuCiklas++;

                            Console.WriteLine("{0},{1},{2},{3}", out0[i], out0[i + 1], out0[i + 2], out0[i + 3] + "\n");
                        }
                    }
                    Console.WriteLine("Veiklu " + unikalusVardai[j] + " ir " + unikalusVardai[j + 1] + " ciklas ivyko " + DviejuCiklas + "  kartus" + "\n");
                    double TiesioginesSantykis = DviejuCiklas / IsVisoElementu;
                    Console.WriteLine("Procentaliai siu veiklu ciklas ivyko " + TiesioginesSantykis + "\n");
                }
                */

                /*
                 // koreliacija
                 for (var x = 0; x < unikalusEgzemplioriai.Count() - 1; x++)
                 {
                     for (var k = 0; k < unikalusVardai.Count()- 1; k++)
                     {
                         double koreliacija = 0;
                         for (var j = 0; j < unikalusVardai.Count()-1; j++)
                         {
                             //Console.WriteLine(unikalusVardai[j] + " ir " + unikalusVardai[k] + " koreliacijos atvejai \n");
                             for (var i = 0; i < a; i++)
                             {
                                     //if (out0[i].Item1 == out0[i+1].Item1 && out0[i].Item1.Equals(unikalusEgzemplioriai[x]) && out0[i+1].Item1.Equals(unikalusEgzemplioriai[x])  )

                                       if (out0[i].Item1 == unikalusEgzemplioriai[x] && out0[i].Item2 == unikalusVardai[j]
                                         && out0[i+1].Item1 == unikalusEgzemplioriai[x]
                                         && out0[i+1].Item2 != unikalusVardai[j])
                                       {
                                         koreliacija++;
                                       }
                                 //double KoreliacijosSantykis = koreliacija / IsVisoElementu;
                             }
                             Console.WriteLine("Esant veiklai" + unikalusVardai[j] + " veikla " + unikalusVardai[k] + " nepasirode " + koreliacija + "  egzemplioriuose" + "\n");
                         }
                         //Console.WriteLine("Procentaliai po veiklos " + unikalusVardai[j] + " is karto seke  " + unikalusVardai[k] + "   " + KoreliacijosSantykis + "\n");
                     }
                 }
                 */

                /*
                // netiesiogine seka
                for (var j = 0; j < unikalusVardai.Count() - 1; j++)
                {
                    for (var k = 0; k < unikalusVardai.Count() - 1; k++)
                    {
                        Console.WriteLine(unikalusVardai[j] + " ir " + unikalusVardai[k] + " netiesiogines sekos atvejai \n");
                        double netiesioginis = 0;
                        for (var i = 0; i < a; i++)
                        {

                            if (out0[i].Item1 == out0[i + 1].Item1 && out0[i].Item2.Contains(unikalusVardai[j]) && !out0[i + 1].Item2.Contains(unikalusVardai[k]))
                            {
                                netiesioginis++;

                                Console.WriteLine("{0},{1}", out0[i], out0[i + 1] + "\n");
                            }
                        }
                        Console.WriteLine("Po veiklos " + unikalusVardai[j] + " ne is karto seke veikla " + unikalusVardai[k] + " " + netiesioginis + "  kartus" + "\n");
                        double NeTiesioginesSantykis = netiesioginis / IsVisoElementu;
                        Console.WriteLine("Procentaliai po veiklos " + unikalusVardai[j] + " ne is karto seke  " + unikalusVardai[k] + "   " + NeTiesioginesSantykis + "\n");
                    }
                }
                */

                Console.ReadLine();


            }

        }



        private static string IsgaukElementoPavadinima(XElement elementas)
        {
            //isgaunam visus <string> elementus
            var stringElementai = elementas.Elements(stringElementoPav);

            //surandam pirma <string> elementa, kuris yra <string key="concept:name"....
            var vardoElementas = stringElementai.Where(x => x.Attribute(keyAtributoPav).Value == vardoAtributoReiksme).FirstOrDefault();

            //paimam surasto elemento value atributo reiksme, kuri yra egzemplioriaus identifikatorius
            var pavadinimas = vardoElementas.Attribute(valueAtributoPav).Value;
            return pavadinimas;
        }

        public static void Pause(string message, params string[] args)
        {
            Console.WriteLine(String.Format(message, args: args));
            Console.WriteLine("Iveskite betkoki simboli...");
            Console.ReadKey();
        }
    }

}

//veikia?