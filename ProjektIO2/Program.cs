using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektIO2
{
    public class Osobnik//Klasa pomocnicza, dzięki której zapisywana jest kolejność zadań (Tab) oraz termin zakończenia ostatniego zadania (suma)
    {
        public int[] Tab = new int[1000];
        public int suma;
        public int[] Tab1 { get => Tab; set => Tab = value; }
        public int Suma { get => suma; set => suma = value; }
        public Osobnik()
        {
            suma = 0;
        }
        public Osobnik(int[] Tab, int suma)
        {
            this.Tab = Tab;
            this.suma = suma;
        }

        public static IComparer SortBySum
        { get { return (IComparer)new OsobnikSumComparer(); } }
    }

    public class OsobnikSumComparer : IComparer//Interfejs pozwalający na posortowanie tablicy (w rankingu liniowym)
    {
        int IComparer.Compare(Object o1, Object o2)
        {
            Osobnik os1 = o1 as Osobnik;
            Osobnik os2 = o2 as Osobnik;
            if (os1 != null && os2 != null)
                return os2.suma.CompareTo(os1.suma);
            else
                throw new ArgumentException("Parametr nie jest osobnikiem!");
        }
    }

    class Program
    {


        static void Main(string[] args)
        {
            string plik;
            string rodzaj;
            Console.WriteLine("Wybierz plik do wczytania podając numer opcji:");//Switch służacy wybieraniu pliku z jakiego mają być odczytywane dane
            Console.WriteLine("1.67 zadań na 7 maszynach");
            rodzaj = Console.ReadLine();
            switch (rodzaj)
            {
                case "1":
                {
                    plik = "Dane.xls";
                    break;
                }
                default:
                {
                    plik = "Dane.xls";
                    break;
                }
            }

            //Zczytywanie danych +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            string PathConn = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + plik + "; Extended Properties=\"Excel 8.0;HDR=Yes;\";";
            OleDbConnection conn = new OleDbConnection(PathConn);
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter("Select * from [Dane$]", conn);
            DataSet ds = new DataSet();
            myDataAdapter.Fill(ds);
            int rowsize = ds.Tables[0].Rows.Count;
            int colsize = ds.Tables[0].Columns.Count;
            DataTable dataTab = new DataTable();
            int[,] data = new int[rowsize, colsize];
            //int[,] dane = new int[rowsize, colsize];
            for (int i=0;i<rowsize; i++)
            {
                for(int j=0;j<colsize; j++)
                {
                    data[i, j] = Int32.Parse(ds.Tables[0].Rows[i][j].ToString());
                }
                //Console.WriteLine(data[i, 0] + " " + data[i, 1] + " " + data[i, 2] + " " + data[i, 3] + " " + data[i, 4] + " " + data[i, 5] + " " + data[i, 6] + " " + data[i, 7]);
            }
            //Liczenie podstawowoych wartości +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            /*for (int i = 0; i < rowsize; i++)
            {
                dane[i, 0] = data[i, 0];
                for (int e = 1; e < colsize; e++)
                {
                    if (i == 0)
                    {
                        if (e == 1)
                            dane[i, e] = data[i, e];
                        else
                            dane[i, e] = dane[i, e - 1] + data[i, e];
                    }
                    else
                    {
                        if (e == 1)
                            dane[i, e] = dane[i - 1, e] + data[i, e];
                        else
                        {
                            if (dane[i, e - 1] > dane[i - 1, e])
                            {
                                dane[i, e] = dane[i, e - 1] + data[i, e];
                            }
                            else
                            {
                                dane[i, e] = dane[i - 1, e] + data[i, e];
                            }
                        }
                    }
                }
                Console.WriteLine(dane[i, 0] + " " + dane[i, 1] + " " + dane[i, 2] + " " + dane[i, 3] + " " + dane[i, 4] + " " + dane[i, 5] + " " + dane[i, 6] + " " + dane[i, 7]);
            }*/
            //Switch wybierający jaka metoda będzie wykonywana
            Console.WriteLine("Wybierz metodę podając numer opcji:");
            Console.WriteLine("1.Algorytm genetyczny: Turniej");
            Console.WriteLine("2.Algorytm genetyczny: Ruletka");
            Console.WriteLine("3.Algorytm genetyczny: Ranking Liniowy");
            rodzaj = Console.ReadLine();
            Random rnd = new Random();
            Random rdouble = new Random();
            Osobnik result = new Osobnik();
            string nazwa;
            switch (rodzaj)
            {
                case "1":
                    {
                        result=Turniej(data, rowsize, colsize,rnd);
                        nazwa = "Turniej";
                        break;
                    }
                case "2":
                    {
                        result = Ruletka(data, rowsize, colsize, rnd, rdouble);
                        nazwa = "Ruletka";
                        break;
                    }
                case "3":
                    {
                        result = RankingLiniowy(data, rowsize, colsize, rnd, rdouble);
                        nazwa = "RankingLiniowy";
                        break;
                    }
                default:
                    {
                        result = Turniej(data, rowsize, colsize, rnd);
                        nazwa = "Turniej";
                        break;
                    }
            }
            Zapis(result, nazwa);

            Console.ReadKey();
        }
        //Funkcja zliczająca czas ostatniego zadania na ostatniej maszynie
        static Osobnik Zlicz(int[] next,int[,] data, int rowsize, int colsize)
        {
            Osobnik result = new Osobnik();
            int[,] dane=new int[rowsize,colsize];
            int[,] pomoc = new int[rowsize, colsize];
            for (int i = 0; i < rowsize; i++)//Przepisywanie wartości do pomocniczej tablicy oraz tablicy wynikowej
            {
                result.Tab[i]  =dane[i,0]= next[i];
                for (int j = 0; j < colsize; j++)
                {
                    pomoc[i,j] = data[next[i]-1, j];
                }
            }
            for (int i = 0; i < rowsize; i++)//Zliczanie czasu zakończeń kolejnych zadań
            {
                for (int e = 1; e < colsize; e++)
                {
                    if (i == 0)
                    {
                        if (e == 1)
                            dane[i, e] = pomoc[i, e];
                        else
                            dane[i, e] = dane[i, e - 1] + pomoc[i,e];
                    }
                    else
                    {
                        if (e == 1)
                            dane[i, e] = dane[i - 1, e] + pomoc[i,e];
                        else
                        {
                            if (dane[i, e - 1] > dane[i - 1, e])
                            {
                                dane[i, e] = dane[i, e - 1] + pomoc[i,e];
                            }
                            else
                            {
                                dane[i, e] = dane[i - 1, e] + pomoc[i,e];
                            }
                        }
                    }
                }
                //Console.WriteLine(dane[i, 0] + " " + dane[i, 1] + " " + dane[i, 2] + " " + dane[i, 3] + " " + dane[i, 4] + " " + dane[i, 5] + " " + dane[i, 6] + " " + dane[i, 7]);
            }

            result.suma = dane[rowsize - 1, colsize - 1];//Zapisanie ostatniego zakończenia do wyniku

            return result;
        }

        static Osobnik Losowanie(int[,] data, int rowsize, int colsize, Random rnd)//Losowanie rozwiązania
        {
            Osobnik result = new Osobnik();
            int[,] dane = new int[rowsize, colsize];
            int[] next=new int[rowsize];
            int j = 0;
            int zamiana = 0;
            List<int> mieszalnik = new List<int>();
            List<int> lista = new List<int>();
            //Tworzenie poszczególnych ustawień dla n-rozwiązań
            for (int i = 0; i < rowsize; i++)
                lista.Add(i);
            for (int i = 0; i < rowsize; i++)
            {
                zamiana = rnd.Next(0, lista.Count);
                mieszalnik.Add(lista[zamiana]);
                lista.RemoveAt(zamiana);
            }
            for (int i = 0; i < rowsize; i++)
            {
                for (j = 0; j < colsize; j++)
                {
                    next[mieszalnik[i]] = data[i, 0];   
                }
            }
            result=Zlicz(next,data, rowsize, colsize);//Zliczanie sumy dla losowanego ustawienia
            Console.WriteLine(result.suma);
            return result;
        }
        //Tworzenie dzieci z dwóch najlepszych osobników lub przejście rodziców do pokolenia dzieci w razie mutacji
        static Osobnik[] Krzyzowanie(Osobnik a, Osobnik b, int[] przedzial, int rozmiar, int colsize, double mutacje, int[,] data, Random rdouble, Random rand)
        {
            Osobnik[] dzieci = new Osobnik[2];
            List<int> lista1 = new List<int>();
            List<int> lista2 = new List<int>();
            int k;
            int i;
            int numer;

            dzieci[0] = new Osobnik();
            dzieci[1] = new Osobnik();
            //Tworzenie 1. dziecka
            for (i = przedzial[1]; i < rozmiar; i++)//Trzeci przedział osobnika B
                lista1.Add(b.Tab[i]);
            for (i = 0; i < przedzial[1]; i++)//Pierwszy i drugi przedział osobnika B
                lista2.Add(b.Tab[i]);
            for (i = przedzial[0]; i < przedzial[1]; i++)//Przepisanie wartości drugiego przedziału osobnika A do 1. dziecka
            {
                dzieci[0].Tab[i] = a.Tab[i];
                if (lista1.Count!=0 && lista1.Contains(a.Tab[i]))//Usunięcie powtarzających się wartości dziecka i trzeciego przedziału osobnika B
                    lista1.Remove(a.Tab[i]);
                else if(lista2.Count != 0 && lista2.Contains(a.Tab[i]))//Usunięcie powtarzających się wartości dziecka i pierwszego przedziału osobnika B
                    lista2.Remove(a.Tab[i]);
            }
            k = lista1.Count;
            for (i=przedzial[1];i< przedzial[1]+k;i++)//Przepisania wartości trzeciego przedziału osobnika B do 1. dziecka
            {
                numer = rand.Next(0,lista1.Count);
                dzieci[0].Tab[i] = lista1[numer];
                lista1.RemoveAt(numer);
            }
            for (i= przedzial[1]+k;i<rozmiar;i++)//Przepisania wartości pierwszego i drugiego przedziału osobnika B do 1. dziecka
            {
                numer = rand.Next(0, lista2.Count);
                dzieci[0].Tab[i] = lista2[numer];
                lista2.RemoveAt(numer);
            }
            for (i=0;i<przedzial[0];i++)//Przepisania wartości pierwszego i drugiego przedziału osobnika B do 1. dziecka
            {
                numer = rand.Next(0, lista2.Count);
                dzieci[0].Tab[i] = lista2[numer];
                lista2.RemoveAt(numer);
            }
            //Tworzenie 2. dziecka
            for (i = przedzial[1]; i < rozmiar; i++)//Trzeci przedział osobnika A
                lista1.Add(a.Tab[i]);
            for (i = 0; i < przedzial[1]; i++)//Pierwszy i drugi przedział osobnika A
                lista2.Add(a.Tab[i]);
            for (i = przedzial[0]; i < przedzial[1]; i++)//Przepisanie wartości drugiego przedziału osobnika B do 2. dziecka
            {
                dzieci[1].Tab[i] = b.Tab[i];
                if (lista1.Count != 0 && lista1.Contains(b.Tab[i]))//Usunięcie powtarzających się wartości dziecka i trzeciego przedziału osobnika A
                    lista1.Remove(b.Tab[i]);
                else if (lista2.Count != 0 && lista2.Contains(b.Tab[i]))//Usunięcie powtarzających się wartości dziecka i pierwszego przedziału osobnika A
                    lista2.Remove(b.Tab[i]);
            }
            k = lista1.Count;
            for (i = przedzial[1]; i < przedzial[1] + k; i++)//Przepisania wartości trzeciego przedziału osobnika A do 2. dziecka
            {
                numer = rand.Next(0, lista1.Count);
                dzieci[1].Tab[i] = lista1[numer];
                lista1.RemoveAt(numer);
            }
            for (i = przedzial[1] + k; i < rozmiar; i++)//Przepisania wartości pierwszego i drugiego przedziału osobnika A do 2. dziecka
            {
                numer = rand.Next(0, lista2.Count);
                dzieci[1].Tab[i] = lista2[numer];
                lista2.RemoveAt(numer);
            }
            for (i = 0; i < przedzial[0]; i++)//Przepisania wartości pierwszego i drugiego przedziału osobnika A do 2. dziecka
            {
                numer = rand.Next(0, lista2.Count);
                dzieci[1].Tab[i] = lista2[numer];
                lista2.RemoveAt(numer);
            }

            numer = 0;
            if(rdouble.NextDouble()>mutacje)//Jeżeli liczba losowa jest większa niż badana wartość współczynnika mutacji, to ta nie nastąpi
            {
                for (i = 0; i < rozmiar; i++)
                {
                    if (dzieci[0].Tab[i] == dzieci[1].Tab[i])
                        numer++;
                }
                if(numer==rozmiar-2)//Jeżeli mutacja następuje, a losowa jest większa od współczynnika mutacji to rodzice przechodzą od razu pokolenia dzieci
                {
                    dzieci[0] = a;
                    dzieci[1] = b;
                }
                else
                {
                    dzieci[0] = Zlicz(dzieci[0].Tab,data, rozmiar, colsize);
                    dzieci[1] = Zlicz(dzieci[1].Tab,data, rozmiar, colsize);
                }
            }
            else
            {
                dzieci[0] = Zlicz(dzieci[0].Tab,data, rozmiar, colsize);
                dzieci[1] = Zlicz(dzieci[1].Tab,data, rozmiar, colsize);
            }

            return dzieci;
        }

        static void Zapis(Osobnik osobnik,string nazwa)
        {
            int suma = osobnik.suma;
            int[] data = new int[osobnik.Tab.Length];
            data = osobnik.Tab;
            string odp;
            Console.WriteLine("Czy chcesz zapisać to ustawienie? Wybierz numer opcji:");
            Console.WriteLine("1. TAK");
            Console.WriteLine("2. NIE");
            odp = Console.ReadLine();
            switch (odp)
            {
                case "1":
                    {
                        nazwa += suma.ToString();
                        nazwa += ".csv";
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < osobnik.Tab.Length; i++)
                        {
                            sb.Append(data[i]);
                            sb.Append("\n");
                        }
                        File.WriteAllText(nazwa, sb.ToString());
                        System.IO.File.WriteAllText(nazwa, sb.ToString());

                        Console.WriteLine("Plik został zapisany pod nazwą metody oraz wynikiem ustawienia i znajduje się w: ProjektIO\\bin\\Debug\\netcoreapp2.2");
                        Console.WriteLine("Potwierdź zakończenie programu");
                        break;
                    }
                case "2":
                    {
                        Console.WriteLine("Potwierdź zakończenie programu");
                        break;
                    }
            }
            Console.ReadKey();
        }

        static Osobnik FindMin(List<Osobnik> grupa,int glosowa)//Znajdowanie najmniejszej wartości z badanego ugrupowania
        {
            Osobnik min=grupa[0];
            for(int i=1;i<glosowa;i++)
            {
                if (min.suma > grupa[i].suma)
                    min = grupa[i];
            }
            return min;
        }

        static Osobnik Turniej(int[,] dane, int rowsize, int colsize, Random rnd)
        {
            List<Osobnik> wyniki = new List<Osobnik>();
            Osobnik result = new Osobnik();
            Random rdouble = new Random();
            Random rand = new Random();
            List<int> lista = new List<int>();
            List<Osobnik> LosowiOsobnicy = new List<Osobnik>();
            int zamiana;
            string odp;
            int powtorzenia = 10;
            int nrozwiazan = 10;
            int glosowa = (int)(nrozwiazan * 0.3);//Ugrupowanie, z którego wybierany jest najlepsy wynik ma wielkość 30% liczby rozwiązań
            double mutacje = 0.1;
            int[] przedzial = new int[2];
            przedzial[0] = 2;
            przedzial[1] = 5;

            Console.WriteLine("Podaj liczbę pokoleń:");
            odp = Console.ReadLine();
            Int32.TryParse(odp, out powtorzenia);

            Console.WriteLine("Podaj liczbę rozwiązań do analizy (musi być parzysta):");
            odp = Console.ReadLine();
            Int32.TryParse(odp, out nrozwiazan);

            Console.WriteLine("Podaj prawdopodobieństwo mutacji (liczba zmiennoprzecinkowa od 0 do 1):");
            odp = Console.ReadLine();
            mutacje = Double.Parse(odp);
            Console.WriteLine("Podaj przedział (dwie liczby całkowite od 0 do {0}):",nrozwiazan);
            odp = Console.ReadLine();
            Int32.TryParse(odp, out przedzial[0]);
            odp = Console.ReadLine();
            Int32.TryParse(odp, out przedzial[1]);

            Osobnik[] osobnicy = new Osobnik[nrozwiazan];
            Osobnik[] dzieci = new Osobnik[nrozwiazan];
            Osobnik[] pomocnicy = new Osobnik[2];
            List<int> mieszalnik = new List<int>();
            pomocnicy[0] = pomocnicy[1] = new Osobnik();
            //Losowanie początkowych ustawień zadań dla n-rozwiązań
            for (int j = 0; j < nrozwiazan; j++)
            {
                osobnicy[j] = Losowanie(dane, rowsize, colsize, rnd);
                //Console.WriteLine(osobnicy[j].Tab[0] + " " + osobnicy[j].suma);
            }
            for (int i = 0; i < powtorzenia; i++)
            {
                for (int j = 0; j < nrozwiazan; j += 2)
                {
                    mieszalnik = new List<int>();
                    LosowiOsobnicy = new List<Osobnik>();
                    lista = new List<int>();
                    for (int k = 0; k < nrozwiazan; k++)
                        lista.Add(k);
                    for (int k = 0; k < glosowa; k++)//Losowanie ugrupowania z którego powstaną rodzice
                    {
                        zamiana = rnd.Next(0, lista.Count);
                        mieszalnik.Add(lista[zamiana]);
                        lista.RemoveAt(zamiana);
                        LosowiOsobnicy.Add(osobnicy[mieszalnik[k]]);
                    }
                    pomocnicy[0] = FindMin(LosowiOsobnicy, glosowa);//Wybór pierwszego najlepszego wyniku
                    LosowiOsobnicy.Remove(pomocnicy[0]);
                    pomocnicy[1] = FindMin(LosowiOsobnicy, glosowa-1);//Wybór drugiego najlepszego wyniku
                    //Console.WriteLine("Rodzic 1: " + pomocnicy[0].suma);
                    //Console.WriteLine("Rodzic 2: " + pomocnicy[1].suma);

                    pomocnicy = Krzyzowanie(pomocnicy[0], pomocnicy[1], przedzial, rowsize, colsize, mutacje, dane, rdouble, rand);//Tworzenie dzieci z wybranych rodziców
                    osobnicy[j] = pomocnicy[0];
                    osobnicy[j + 1] = pomocnicy[1];
                    
                    //Console.WriteLine("Dziecko " + j + ": " + osobnicy[j].suma);
                    //Console.WriteLine("Dziecko " + j + 1 + ": " + osobnicy[j + 1].suma);
                }
                Console.WriteLine("\nKolejne pokolenie");
                for (int j = 0; j < nrozwiazan; j++)
                {
                    Console.WriteLine(osobnicy[j].suma);
                }
                    
            }
            for (int j = 0; j < nrozwiazan; j++)
            {
                wyniki.Add(osobnicy[j]);
            }
            result = FindMin(wyniki, nrozwiazan);
            Console.WriteLine("Wynik końcowy: " + result.suma);
            return result;
        }

        static Osobnik Ruletka(int[,] dane, int rowsize, int colsize, Random rnd,Random rdouble)
        {
            Random rand = new Random();
            List<Osobnik> wyniki = new List<Osobnik>();
            List<Osobnik> LosowiOsobnicy = new List<Osobnik>();
            Osobnik result = new Osobnik();
            int powtorzenia = 2;
            int nrozwiazan = 10;
            double mutacje = 0.1;
            string odp;
            Osobnik[] pomocnicy = new Osobnik[2];
            List<int> mieszalnik = new List<int>();
            pomocnicy[0] = pomocnicy[1] = new Osobnik();
            int glosowa = (int)(nrozwiazan * 0.3);//Ugrupowanie, z którego wybierany jest najlepszy wynik ma wielkość 30% liczby rozwiązań
            int[] przedzial = new int[2];
            przedzial[0] = 2;
            przedzial[1] = 5;

            
            Console.WriteLine("Podaj liczbę pokoleń:");
            odp = Console.ReadLine();
            Int32.TryParse(odp, out powtorzenia);

            Console.WriteLine("Podaj liczbę rozwiązań do analizy (musi być parzysta):");
            odp = Console.ReadLine();
            Int32.TryParse(odp, out nrozwiazan);

            Console.WriteLine("Podaj prawdopodobieństwo mutacji (liczba zmiennoprzecinkowa od 0 do 1):");
            odp = Console.ReadLine();
            mutacje = Double.Parse(odp);
            
            Console.WriteLine("Podaj przedział (dwie liczby całkowite od 0 do {0}):", nrozwiazan);
            odp = Console.ReadLine();
            Int32.TryParse(odp, out przedzial[0]);
            odp = Console.ReadLine();
            Int32.TryParse(odp, out przedzial[1]);
            Osobnik[] osobnicy = new Osobnik[nrozwiazan];
            //Losowanie początkowych ustawień zadań dla n-rozwiązań
            for (int j = 0; j < nrozwiazan; j++)
            {
                osobnicy[j] = Losowanie(dane, rowsize, colsize, rnd);
                //Console.WriteLine(osobnicy[j].Tab[0] + " " + osobnicy[j].suma);
            }
            
            double rozmiarWycinka = 100 / nrozwiazan;//wielkość pojedynczeko wycinka ruletki
            double losowa = 0.0;
            int znacznik;
            int mnoznik;
            List<int> lista = new List<int>();
            lista = new List<int>();
            
            for (int i = 0; i < powtorzenia; i++)
            {
                for (int j = 0; j < nrozwiazan; j += 2)
                {
                    for (int k = 0; k < nrozwiazan; k++)
                        lista.Add(k);
                    LosowiOsobnicy = new List<Osobnik>();
                    for (int k=0;k<glosowa;)//Losowanie chromosomów
                    {
                        losowa = rdouble.NextDouble() * 100;//Liczba losowa
                        mnoznik = Convert.ToInt32(Math.Floor(losowa / rozmiarWycinka));//numer indeksu wycinka ruletki
                        if (lista.Contains(mnoznik))//Dodawanie elementów ugrupowania do zbioru liczb poddawanych analizie
                        {
                            lista.Remove(mnoznik);
                            LosowiOsobnicy.Add(osobnicy[mnoznik]);
                            k++;
                        }
                    }
                    pomocnicy[0] = FindMin(LosowiOsobnicy, glosowa);//Wybór pierwszego najlepszego wyniku
                    LosowiOsobnicy.Remove(pomocnicy[0]);
                    pomocnicy[1] = FindMin(LosowiOsobnicy, glosowa-1);//Wybór drugiego najlepszego wyniku
                    //Console.WriteLine("Rodzic 1: " + pomocnicy[0].suma);
                    //Console.WriteLine("Rodzic 2: " + pomocnicy[1].suma);


                    pomocnicy = Krzyzowanie(pomocnicy[0], pomocnicy[1], przedzial, rowsize, colsize, mutacje, dane, rdouble, rand);//Tworzenie dzieci z wybranych rodziców
                    osobnicy[j] = pomocnicy[0];
                    osobnicy[j + 1] = pomocnicy[1];

                    //Console.WriteLine("Dziecko " + j + ": " + osobnicy[j].suma);
                    //Console.WriteLine("Dziecko " + j + 1 + ": " + osobnicy[j + 1].suma);

                }
                Console.WriteLine("\nKolejne pokolenie");
                for (int j = 0; j < nrozwiazan; j++)
                {
                    Console.WriteLine(osobnicy[j].suma);
                }



            }
            for (int j = 0; j < nrozwiazan; j++)
            {
                wyniki.Add(osobnicy[j]);
            }
            result = FindMin(wyniki, nrozwiazan);
            Console.WriteLine("Wynik końcowy: " + result.suma);

            return result;   
        }

        static Osobnik RankingLiniowy(int[,] dane, int rowsize, int colsize, Random rnd, Random rdouble)
        {
            Random rand = new Random();
            List<Osobnik> wyniki = new List<Osobnik>();
            List<Osobnik> LosowiOsobnicy = new List<Osobnik>();
            Osobnik result = new Osobnik();
            int powtorzenia = 2;
            int nrozwiazan = 10;
            double mutacje = 0.1;
            string odp;
            Osobnik[] pomocnicy = new Osobnik[2];
            List<int> mieszalnik = new List<int>();
            pomocnicy[0] = pomocnicy[1] = new Osobnik();
            int glosowa = (int)(nrozwiazan * 0.3);//Ugrupowanie, z którego wybierany jest najlepszy wynik ma wielkość 30% liczby rozwiązań
            int[] przedzial = new int[2];
            przedzial[0] = 2;
            przedzial[1] = 5;


            Console.WriteLine("Podaj liczbę pokoleń:");
            odp = Console.ReadLine();
            Int32.TryParse(odp, out powtorzenia);

            Console.WriteLine("Podaj liczbę rozwiązań do analizy (musi być parzysta):");
            odp = Console.ReadLine();
            Int32.TryParse(odp, out nrozwiazan);

            Console.WriteLine("Podaj prawdopodobieństwo mutacji (liczba zmiennoprzecinkowa od 0 do 1):");
            odp = Console.ReadLine();
            mutacje = Double.Parse(odp);

            Console.WriteLine("Podaj przedział (dwie liczby całkowite od 0 do {0}):", nrozwiazan);
            odp = Console.ReadLine();
            Int32.TryParse(odp, out przedzial[0]);
            odp = Console.ReadLine();
            Int32.TryParse(odp, out przedzial[1]);
            Osobnik[] osobnicy = new Osobnik[nrozwiazan];
            //Losowanie początkowych ustawień zadań dla n-rozwiązań
            for (int j = 0; j < nrozwiazan; j++)
            {
                osobnicy[j] = Losowanie(dane, rowsize, colsize, rnd);
                //Console.WriteLine(osobnicy[j].Tab[0] + " " + osobnicy[j].suma);
            }

            int rozmiarRuletki;//wielkość pojedynczeko wycinka ruletki
            double losowa = 0.0;
            int znacznik;
            int mnoznik;
            int g;
            List<int> lista = new List<int>();
            List<double> ruletkaList = new List<double>();
            lista = new List<int>();

            for (int i = 0; i < powtorzenia; i++)
            {
                for (int j = 0; j < nrozwiazan; j += 2)
                {
                    Array.Sort(osobnicy, Osobnik.SortBySum);//Sortowanie tablicy osobników
                    rozmiarRuletki = 0;
                    for (int k = 0; k < nrozwiazan; k++)
                    {
                        lista.Add(k);
                        rozmiarRuletki += (k+1);
                    }
                    ruletkaList.Add(0.0);
                    for (int k = 0; k < nrozwiazan; k++)//Nadawanie wielkości poszczególnym kawałkom ruletki
                    {
                        losowa = k+1.0;
                        losowa /= rozmiarRuletki;
                        losowa += ruletkaList[k];
                        ruletkaList.Add(losowa);
                    }

                    LosowiOsobnicy = new List<Osobnik>();
                    for (int k = 0; k < glosowa;)//Losowanie chromosomów
                    {
                        losowa = rdouble.NextDouble();//Liczba losowa
                        g = 0;
                        while(losowa>ruletkaList[g])
                            g++;
                        g--;
                        if (lista.Contains(g))//Dodawanie elementów ugrupowania do zbioru liczb poddawanych analizie
                        {
                            lista.Remove(g);
                            LosowiOsobnicy.Add(osobnicy[g]);
                            Console.WriteLine("Osobnicy " + osobnicy[g].suma);
                            k++;
                        }
                    }
                    pomocnicy[0] = FindMin(LosowiOsobnicy, glosowa);//Wybór pierwszego najlepszego wyniku
                    LosowiOsobnicy.Remove(pomocnicy[0]);
                    pomocnicy[1] = FindMin(LosowiOsobnicy, glosowa - 1);//Wybór drugiego najlepszego wyniku
                    //Console.WriteLine("Rodzic 1: " + pomocnicy[0].suma);
                    //Console.WriteLine("Rodzic 2: " + pomocnicy[1].suma);


                    pomocnicy = Krzyzowanie(pomocnicy[0], pomocnicy[1], przedzial, rowsize, colsize, mutacje, dane, rdouble, rand);//Tworzenie dzieci z wybranych rodziców
                    osobnicy[j] = pomocnicy[0];
                    osobnicy[j + 1] = pomocnicy[1];

                    //Console.WriteLine("Dziecko " + j + ": " + osobnicy[j].suma);
                    //Console.WriteLine("Dziecko " + j + 1 + ": " + osobnicy[j + 1].suma);

                }
                Console.WriteLine("\nKolejne pokolenie");
                for (int j = 0; j < nrozwiazan; j++)
                {
                    Console.WriteLine(osobnicy[j].suma);
                }



            }
            for (int j = 0; j < nrozwiazan; j++)
            {
                wyniki.Add(osobnicy[j]);
            }
            result = FindMin(wyniki, nrozwiazan);
            Console.WriteLine("Wynik końcowy: " + result.suma);




            return result;
        }



    }
}
