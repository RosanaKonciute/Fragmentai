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
                    if (!unikalusVardai.Contains(out0[j].Item2))
                    {
                        unikalusVardai.Add(out0[j].Item2);
                    }
                }
                Console.WriteLine("Unikalus ivykiu vardai zurnale \n");
                for (var j = 0; j < unikalusVardai.Count(); j++)
                {
                    Console.WriteLine(unikalusVardai[j] + "\n");
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




                //TITO PASIULYMAS
                Matrix priklausomybiuMatrica = new Matrix();

                //tuscia matrica uzpildome
                foreach (var ivykioPavadinimas in unikalusVardai)
                {
                    foreach (var ivykioPavadinimas2 in unikalusVardai)
                    {
                        priklausomybiuMatrica.GetElement(ivykioPavadinimas, ivykioPavadinimas2);
                    }
                }



                //sukuriame zurnalo reprezentacija objektais
                var tmp = out0[0];
                var zurnalas = new Log();
                var currEgz = new Trace() { Id = tmp.Item1 };
                currEgz.Events.Add(new Event() { Name = out0[0].Item2 });
                zurnalas.Traces.Add(currEgz);
                for (var i = 1; i < out0.Count; i++)
                {
                    var tmp2 = out0[i];
                    if (tmp2.Item1 != currEgz.Id)
                    {
                        currEgz = new Trace() { Id = tmp2.Item1 };
                        zurnalas.Traces.Add(currEgz);
                    }
                    currEgz.Events.Add(new Event() { Name = out0[i].Item2 });
                }

                SuskaiciuokColumnCycling(priklausomybiuMatrica, zurnalas);
                SuskaiciuokCycleIrDirectlyFollows(unikalusVardai, priklausomybiuMatrica, zurnalas);
                SuskaiciuokKoreliacijas(unikalusVardai, priklausomybiuMatrica, zurnalas);
                SuskaiciuokEventuallyFollows(unikalusVardai, priklausomybiuMatrica, zurnalas);

                //double followsSuma = priklausomybiuMatrica.Elements.Sum(x => x.Follows);
                //double eventuallyFollowsSuma = priklausomybiuMatrica.Elements.Sum(x => x.EventuallyFollows);
                //double cycleCountSuma = priklausomybiuMatrica.Elements.Sum(x => x.CycleCount);
                //double ColumnCycleSuma = priklausomybiuMatrica.Elements.Sum(x => x.ColumnCycling);
                //double CorrelationSuma = priklausomybiuMatrica.Elements.Sum(x => x.Correlation);

                //Console.WriteLine("{0},{1},{2},{3},{4}", followsSuma, eventuallyFollowsSuma, cycleCountSuma, ColumnCycleSuma, CorrelationSuma);

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(Environment.CurrentDirectory + @"\InputTinklui.csv", false))
                {
                    file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                                 "Veikla1", "Veikla2",
                               "r1", "r2", "r3", "r4", "r5", "r21", "r22", "r23", "r24", "r25", "class");
                    int EgzemplioriuSkaiciusLaikinas = unikalusEgzemplioriai.Count();
                    double EgzemplioriuSkaicius = Convert.ToDouble(EgzemplioriuSkaiciusLaikinas);
                    List<string> done = new List<string>();

                    for (var i = 0; i < unikalusVardai.Count; i++)
                    {
                        var unikalusVardas = unikalusVardai[i];
                        for (var j = i; j < unikalusVardai.Count; j++)
                        {
                            var unikalusVardasPora = unikalusVardai[j];
                            if (unikalusVardas == unikalusVardasPora)
                            {
                                continue;
                            }
                            var pair = String.Format("{0}{1}", unikalusVardas, unikalusVardasPora);
                            var pair2 = String.Format("{0}{1}", unikalusVardasPora, unikalusVardas);
                            if (!done.Contains(pair) && !done.Contains(pair2))
                            {
                                var matrixElement = priklausomybiuMatrica.GetElement(unikalusVardas, unikalusVardasPora);
                                var matrixElementInverse = priklausomybiuMatrica.GetElement(unikalusVardasPora, unikalusVardas);


                                double followsSuma = matrixElement.Follows + matrixElementInverse.Follows;
                                double eventuallyFollowsSuma = matrixElement.EventuallyFollows + matrixElementInverse.EventuallyFollows;
                                double cycleCountSuma = matrixElement.CycleCount + matrixElementInverse.CycleCount;
                                double ColumnCycleSuma = matrixElement.ColumnCycling + matrixElementInverse.ColumnCycling;
                                double CorrelationSuma = matrixElement.OccuredRow + matrixElementInverse.OccuredColumn;


                                Console.WriteLine("{0}", matrixElement.OccuredColumn);
                                Console.WriteLine("{0}", matrixElement.OccuredRow);

                                double r1 = Convert.ToDouble(matrixElement.Follows);
                                double r2 = Convert.ToDouble(matrixElement.EventuallyFollows);
                                double r3 = Convert.ToDouble(matrixElement.CycleCount);
                                double r4 = Convert.ToDouble(matrixElement.ColumnCycling);
                                double R5 = Convert.ToDouble(matrixElement.Correlation);
                                double r21 = Convert.ToDouble(matrixElementInverse.Follows);
                                double r22 = Convert.ToDouble(matrixElementInverse.EventuallyFollows);
                                double r23 = Convert.ToDouble(matrixElementInverse.CycleCount);
                                double r24 = Convert.ToDouble(matrixElementInverse.ColumnCycling);
                                double R25 = Convert.ToDouble(matrixElementInverse.Correlation);



                                var _r1 = r1 / followsSuma;
                                var _r21 = r21 / followsSuma;

                                double r1DivideFollowSuma = r1 / followsSuma;
                                double r2DivideeventuallyFollowsSuma = r2 / eventuallyFollowsSuma;
                                double r3DividecycleCountSuma = r3 / cycleCountSuma;
                                double r4DivideColumnCycleSuma = r4 / ColumnCycleSuma;
                                double r5 = R5 / EgzemplioriuSkaicius;

                                double r21DivideFollowSuma = r21 / followsSuma;
                                double r22DivideeventuallyFollowsSuma = r22 / eventuallyFollowsSuma;
                                double r23DividecycleCountSuma = r23 / cycleCountSuma;
                                double r24DivideColumnCycleSuma = r24 / ColumnCycleSuma;
                                double r25 = R25 / EgzemplioriuSkaicius;

                                //PatikrinkArDalmuosNulis(r2, r2DivideeventuallyFollowsSuma);
                                //PatikrinkArDalmuosNulis(r3, r3DividecycleCountSuma);
                                //PatikrinkArDalmuosNulis(r4, r4DivideeventuallyFollowsSuma);
                                //PatikrinkArDalmuosNulis(r5, r5DivideeventuallyFollowsSuma);
                                //PatikrinkArDalmuosNulis(r21, r21DivideFollowSuma);
                                //PatikrinkArDalmuosNulis(r22, r22DivideeventuallyFollowsSuma);
                                //PatikrinkArDalmuosNulis(r23, r23DividecycleCountSuma);
                                //PatikrinkArDalmuosNulis(r24, r24DivideeventuallyFollowsSuma);
                                //PatikrinkArDalmuosNulis(r25, r25DivideeventuallyFollowsSuma);



                                r1DivideFollowSuma = r1DivideFollowSuma.MakeZeroIfNan();

                                //if (r1 == 0)
                                //{
                                //    r1DivideFollowSuma = 0;
                                //}
                                if (r21 == 0)
                                {
                                    r21DivideFollowSuma = 0;
                                }
                                if (r2 == 0)
                                {
                                    r2DivideeventuallyFollowsSuma = 0;
                                }
                                if (r22 == 0)
                                {
                                    r22DivideeventuallyFollowsSuma = 0;
                                }
                                if (r3 == 0)
                                {
                                    r3DividecycleCountSuma = 0;
                                }
                                if (r23 == 0)
                                {
                                    r23DividecycleCountSuma = 0;
                                }
                                if (r4 == 0)
                                {
                                    r4DivideColumnCycleSuma = 0;
                                }
                                if (r24 == 0)
                                {
                                    r24DivideColumnCycleSuma = 0;
                                }
                                if (r5 == 0)
                                {
                                    r5 = 0;
                                }
                                if (r25 == 0)
                                {
                                    r25 = 0;
                                }


                                double tau = 0.05;
                                string klase = " ";
                                List<string> klases = new List<string>();
                                if ((r1DivideFollowSuma + r21DivideFollowSuma) > (1 - tau)
                                    && r1DivideFollowSuma > tau && r21DivideFollowSuma > tau
                                    && (r5 + r25) < (0 + tau))
                                {
                                    klases.Add("AND");
                                }
                                if ((r1DivideFollowSuma + r21DivideFollowSuma) < (0 + tau) && (r5 + r25) > (1 - tau))
                                {
                                    klase = "XOR";
                                }
                                if ((0 + tau) < (r1DivideFollowSuma + r21DivideFollowSuma) && (r1DivideFollowSuma + r21DivideFollowSuma) < (1 - tau) && (0 + tau) < (r5 + r25) && (r5 + r25) < (1 - tau))
                                {
                                    klase = "OR";
                                }
                                if (r3DividecycleCountSuma > (0 + tau))
                                {
                                    klase = "w_i_ciklas";
                                }
                                if (r23DividecycleCountSuma > (0 + tau))
                                {
                                    klase = "w_j_ciklas";
                                }
                                if (r4DivideColumnCycleSuma > (0 + tau))
                                {
                                    klase = "w_i>>w_j ciklas";
                                }
                                if (r24DivideColumnCycleSuma > (0 + tau))
                                {
                                    klase = "w_j>>w_i ciklas";
                                }
                                if (r1DivideFollowSuma > tau && r21DivideFollowSuma <= tau && r5 <= tau && r25 <= tau)
                                {
                                    klase = "w_i w_j direct follow";
                                }
                                if (r1DivideFollowSuma < tau && r21DivideFollowSuma >= tau && r5 <= tau && r25 <= tau)
                                {
                                    klase = "w_j w_i direct follow";
                                }
                                if (r2DivideeventuallyFollowsSuma > tau && r1DivideFollowSuma <= tau)
                                {
                                    klase = "w_i w_j eventually follow";
                                }
                                if (r22DivideeventuallyFollowsSuma > tau && r21DivideFollowSuma <= tau)
                                {
                                    klase = "w_j w_i eventually follow";
                                }


                                //jei priskirta daugiau nei viena klase, panaikinam neleidziamas poras
                                IstrinkNeleidziamasKlasiuPoras(klases);

                                file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                                     matrixElement.Column,
                                     matrixElement.Row,
                                     r1DivideFollowSuma,
                                     r2DivideeventuallyFollowsSuma,
                                     r3DividecycleCountSuma,
                                     r4DivideColumnCycleSuma,
                                     r5,
                                     r21DivideFollowSuma,
                                     r22DivideeventuallyFollowsSuma,
                                     r23DividecycleCountSuma,
                                     r24DivideColumnCycleSuma,
                                     r25,
                                     String.Join(",", klases));

                                done.Add(pair);
                                done.Add(pair2);



                            }
                            //file.WriteLine("\r\n\r\n");
                        }
                    }
                }

                using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\tracelist.txt"))
                {
                    foreach (var egz in zurnalas.Traces)
                    {
                        sw.Write(egz.Id + " ");
                        foreach (var ev in egz.Events)
                        {
                            sw.Write(ev.Name);
                        }
                        sw.Write("\r\n");
                    }
                }

                Console.WriteLine("\n");

                Console.ReadLine();


            }

        }

        private static void IstrinkNeleidziamasKlasiuPoras(List<string> klases)
        {
            if (klases.Count > 1)
            {


                //iteruojam pro priskirtas klases
                var found = false;
                for (var k = 0; k < klases.Count; k++)
                {

                    //ieskom leidziamu klasiu porininkiu
                    var wijIdx = klases.IndexOf("w_j_ciklas");
                    var wjiIdx = klases.IndexOf("w_j_ciklas");
                    var WJIIdx = klases.IndexOf("w_i>>w_j ciklas");
                    var WIJIdx = klases.IndexOf("w_j>>w_i ciklas");

                    //jei leidziama pora, nedarom nieko
                    if (wijIdx == k || wjiIdx == k || WJIIdx == k || WIJIdx == k)
                    {
                        continue;
                    }
                    //jei pirma karta surandam betokia kita pora, nedarom nieko
                    if (!found)
                    {
                        found = true;
                        continue;
                    }
                    //jei jau buvo paprapsta klase ir priskirta klase nera galima, istrinam
                    else
                    {
                        klases.RemoveAt(k);
                        k--;
                    }
                }

            }
        }

        private static double PatikrinkArDalmuosNulis(double dalmuo, double naujaReiksme)
        {
            if (dalmuo == 0)
            {
                naujaReiksme = 0;
            }
            return naujaReiksme;

        }

        private static void SuskaiciuokEventuallyFollows(List<string> unikalusVardai, Matrix priklausomybiuMatrica, Log zurnalas)
        {
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
                        if (unikalusVardas == unikalusVardas2)
                        {
                            continue;
                        }
                        Event antras = null;
                        int antroidx = -1;
                        for (var i = 0; i < egzempliorius.Events.Count; i++)
                        {
                            if (egzempliorius.Events[i].Name == unikalusVardas2)
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
        }

        private static void SuskaiciuokKoreliacijas(List<string> unikalusVardai, Matrix priklausomybiuMatrica, Log zurnalas)
        {
            //isgauname koreliacijas
            foreach (var egzempliorius in zurnalas.Traces)
            {
                //surenkame rastus egzemplioriaus ivykius
                List<string> egzemplioriausIvykiai = new List<string>();
                for (var i = 0; i < egzempliorius.Events.Count; i++)
                {
                    var ivykis = egzempliorius.Events[i];
                    if (egzemplioriausIvykiai.Contains(ivykis.Name))
                    {
                        continue;
                    }
                    else
                    {
                        egzemplioriausIvykiai.Add(ivykis.Name);
                    }
                }

                //atrenkame ivykius, kuriu nebuvo egzemplioriuje, nors buvo rasti zurnale
                List<string> nerastiIvykiai = new List<string>();
                foreach (var unikalusVardas in unikalusVardai)
                {
                    if (egzemplioriausIvykiai.Contains(unikalusVardas))
                    {
                        continue;
                    }
                    else
                    {
                        nerastiIvykiai.Add(unikalusVardas);
                    }
                }

                //matricoje padidiname Correlation dydi {rastasIvykis,nerastasIvykis} porai
                foreach (var nerastasIvykis in nerastiIvykiai)
                {
                    foreach (var egzemplioriausIvykis in egzemplioriausIvykiai)
                    {
                        priklausomybiuMatrica.GetElement(egzemplioriausIvykis, nerastasIvykis).Correlation++;
                    }
                }

                foreach (var uniklausVardas in unikalusVardai)
                {
                    foreach (var egzemplioriausIvykis in egzemplioriausIvykiai)
                    {
                        priklausomybiuMatrica.GetElement(egzemplioriausIvykis, uniklausVardas).OccuredColumn++;
                    }
                }

                foreach (var nerastasIvykis in nerastiIvykiai)
                {
                    foreach (var uniklausVardas in unikalusVardai)
                    {
                        priklausomybiuMatrica.GetElement(uniklausVardas, nerastasIvykis).OccuredRow++;
                    }
                }
            }
        }

        private static void SuskaiciuokCycleIrDirectlyFollows(List<string> unikalusVardai, Matrix priklausomybiuMatrica, Log zurnalas)
        {
            foreach (var egzempliorius in zurnalas.Traces)
            {
                for (var i = 0; i < egzempliorius.Events.Count - 1; i++)
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
                    if (pirmasIvykis != antrasivykis)
                    {
                        priklausomybiuMatrica.GetElement(pirmasIvykis, antrasivykis).Follows++;
                    }
                }
            }
        }

        private static void SuskaiciuokColumnCycling(Matrix priklausomybiuMatrica, Log zurnalas)
        {
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

                }
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

    public static class  Extensions{

        public static double MakeZeroIfNan(this double number)
        {
            if (double.IsNaN(number))
            {
                return 0;
            }
            else
            {
                return number;
            }
        }

    }

}

