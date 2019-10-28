using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Aufgabe1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double Scale;
        private double KleinstesY;
        private double GroesstesY;
        private double KleinstesX;
        private double GroesstesX;
        private const double Rand = 300;
        private double OffsetX = 0;
        private double OffsetY = 0;
        private Pfadberechnung Berechnung;
        private bool bZeichneHindernisse = true;
        private bool bZeichneInnenStrecken = false;
        private bool bZeichneAussenStrecken = false;
        private bool bZeichneWeg = true;
        DijkstraVertex[] Sequenz;

        public MainWindow()
        {
            InitializeComponent();

            Berechnung = new Pfadberechnung(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MenuItemAnsichtHindernisse.IsChecked = bZeichneHindernisse;
            MenuItemAnsichtWeg.IsChecked = bZeichneWeg;
            MenuItemAnsichtInnenStrecken.IsChecked = bZeichneInnenStrecken;
            MenuItemAnsichtAussenStrecken.IsChecked = bZeichneAussenStrecken;

            MenuItemAnsichtStrecken.IsChecked = (bZeichneInnenStrecken && bZeichneAussenStrecken);

            Zeichne();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Zeichne();
        }

        private void ZeichneHindernisse()
        {
            if(Berechnung.Hindernisse == null) { return; }
            Hindernis[] Hindernisse = Berechnung.Hindernisse;

            foreach (Hindernis Hindernis in Hindernisse)
            {
                Polygon Polygon = new Polygon()
                {
                    Fill = Brushes.DarkGray
                };

                foreach (Point P in Hindernis.Eckpunkte)
                {
                    Polygon.Points.Add(ZuCanvas(P));
                }

                ZeichenCanvas.Children.Add(Polygon);
            }
        }

        public void ZeichneLinie(Point A, Point B, Brush Color, double Thickness)
        {
            A = ZuCanvas(A);
            B = ZuCanvas(B);

            Line Linie = new Line()
            {
                Stroke = Color,
                X1 = A.X,
                Y1 = A.Y,
                X2 = B.X,
                Y2 = B.Y,
                StrokeThickness = Scale * Thickness
            };

            ZeichenCanvas.Children.Add(Linie);
        }

        private void SetzeSkalierung()
        {
            if (Berechnung.Hindernisse != null)
            {
                KleinstesY = double.PositiveInfinity;
                GroesstesY = double.NegativeInfinity;
                KleinstesX = double.PositiveInfinity;
                GroesstesX = double.NegativeInfinity;

                foreach (Hindernis H in Berechnung.Hindernisse)
                {
                    foreach (Point P in H.Eckpunkte)
                    {
                        if (P.Y > GroesstesY) { GroesstesY = P.Y; }
                        if (P.Y < KleinstesY) { KleinstesY = P.Y; }
                        if (P.X > GroesstesX) { GroesstesX = P.X; }
                        if (P.X < KleinstesX) { KleinstesX = P.X; }
                    }
                }

                double ScaleX = ZeichenCanvas.ActualWidth / (GroesstesX - KleinstesX + Rand);
                double ScaleY = ZeichenCanvas.ActualHeight / (GroesstesY - KleinstesY + Rand);

                Scale = Math.Min(ScaleX, ScaleY);

                OffsetX = ZeichenCanvas.ActualWidth - (GroesstesX - KleinstesX) * Scale;
                OffsetY = ZeichenCanvas.ActualHeight - (GroesstesY - KleinstesY) * Scale;
            }
        }

        private Point ZuCanvas(Point P)
        {
            return new Point((P.X - KleinstesX) * Scale + OffsetX / 2, ZeichenCanvas.ActualHeight - P.Y * Scale - OffsetY / 2);
        }

        private void Zeichne()
        {
            ZeichenCanvas.Children.Clear();

            SetzeSkalierung();

            if(Berechnung.Startpunkt != null)
            {
                ZeichneKreis(Berechnung.Startpunkt, 5, Brushes.Blue);
            }

            ZeichneStrasse();
            if(bZeichneHindernisse) { ZeichneHindernisse(); }
            if(bZeichneInnenStrecken) { ZeichneInnenStrecken(); }
            if(bZeichneAussenStrecken) { ZeichneAussenStrecken(); }
            if(bZeichneWeg) { ZeichneWeg(); }
        }

        private void ZeichneStrasse()
        {
            Point P = ZuCanvas(new Point(0, 0));
            Point A = new Point(P.X, 0);
            Point B = new Point(P.X, ZeichenCanvas.ActualHeight);

            Line Linie = new Line()
            {
                Stroke = Brushes.Black,
                X1 = A.X,
                Y1 = A.Y,
                X2 = B.X,
                Y2 = B.Y,
                StrokeThickness = Scale * 1
            };

            ZeichenCanvas.Children.Add(Linie);
        }

        private void ZeichneWeg()
        {
            if(Sequenz == null) { return; }
            Point? LastPoint = null;
            foreach (DijkstraVertex V in Sequenz)
            {
                Point A;
                if (LastPoint == null)
                {
                    A = Berechnung.Startpunkt;
                }
                else
                {
                    A = (Point)LastPoint;
                }

                if (V.Pos != new Point(-10, 0))
                {
                    ZeichneLinie(A, V.Pos, Brushes.Blue, 1);
                }

                LastPoint = V.Pos;
            }
        }

        private void ZeichneAussenStrecken()
        {
            if (Berechnung.Strecken == null) { return; }
            foreach (Strecke S in Berechnung.Strecken)
            {
                if(S.B == new Point(-10,0)) { continue; }

                ZeichneLinie(S.A, S.B, Brushes.Green, 0.5);
            }
        }

        private void ZeichneInnenStrecken()
        {
            if (Berechnung.Hindernisse == null) { return; }
            foreach (Hindernis H in Berechnung.Hindernisse)
            {
                foreach (Strecke S in H.Strecken)
                {
                    if(S.Kante)
                    {
                        ZeichneLinie(S.A, S.B, Brushes.Red, 0.5);
                    }
                    else
                    {
                        ZeichneLinie(S.A, S.B, Brushes.Green, 0.5);
                    }
                }
            }
        }

        private void ZeichneKreis(Point P, double Radius, Brush Farbe)
        {
            P = ZuCanvas(P);
            Radius *= Scale;

            Ellipse E = new Ellipse()
            {
                Width = Radius,
                Height = Radius,
                Fill = Farbe
            };

            ZeichenCanvas.Children.Add(E);

            Canvas.SetLeft(E, P.X - Radius * 0.5);
            Canvas.SetTop(E, P.Y - Radius * 0.5);
        }

        private void MenuItemOeffnen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Dialog = new OpenFileDialog()
            {
                Filter = "\"Lisa rennt\"-Datei (*.txt)|*.txt|Alle Dateien (*.*)|*.*",
                FilterIndex = 0
            };

            if (Dialog.ShowDialog() != true)
            {
                return;
            }

            FileStream Datei = new FileStream(Dialog.FileName, FileMode.Open);
            string DateiString = String.Empty;

            try
            {
                StreamReader Reader = new StreamReader(Datei);
                DateiString = Reader.ReadToEnd(); // -> Gesamter Textinhalt
            }
            finally
            {
                Datei.Close();
            }

            DateiEinlesen(DateiString);
        }

        private void DateiEinlesen(string Datei)
        {
            // Text in Zeilen unterteilen
            string[] Zeilen = Datei.Split('\n');

            string[] StartpunktStrings = Zeilen[Zeilen.Length - 1].Split(' ');

            int HindernissAnzahl;
            Point _Startpunkt;
            Hindernis[] _Hindernisse;

            try
            {
                // Hindernisanzahl bestimmen
                HindernissAnzahl = int.Parse(Zeilen[0]);
                _Hindernisse = new Hindernis[HindernissAnzahl];

                // Hindernisse bestimmen
                for (int I = 0; I < HindernissAnzahl; I++)
                {
                    // An Leerzeichen trennen -> Array
                    string[] HindernisString = Zeilen[I + 1].Split(' ');

                    // Punktanzahl bestimmen
                    int PunktAnzahl = int.Parse(HindernisString[0]);
                    Point[] Punkte = new Point[PunktAnzahl];

                    // Koordinaten bestimmen
                    for (int J = 0; J < PunktAnzahl; J++)
                    {
                        double X = double.Parse(HindernisString[J * 2 + 1]);
                        double Y = double.Parse(HindernisString[J * 2 + 2]);

                        Punkte[J] = new Point(X, Y);
                    }

                    // Hindernis-Objekt erstellen
                    _Hindernisse[I] = new Hindernis(Punkte);
                }

                // Startpunkt bestimmen
                _Startpunkt = new Point(double.Parse(StartpunktStrings[0]), double.Parse(StartpunktStrings[1]));
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Die Datei konnte nicht korrekt eingelesen werden!" + Environment.NewLine + Ex.Message);
                return;
            }

            Berechnung.SetzeHindernisse(_Hindernisse);
            Berechnung.SetzeStartpunkt(_Startpunkt);

            Berechnen();

            Zeichne();
        }

        private void Berechnen()
        {
            Berechnung.SetzeInnenstrecken();
            Berechnung.SetzeAussenstrecken();
            Berechnung.TesteAussenstrecken();
            Berechnung.TesteInnenstreckenInnerhalb();
            Berechnung.TesteInnenstreckenAusserhalb();
            Berechnung.PunktInPolygon();
            Berechnung.SetzeEndstrecken();

            // Dijkstra
            List<Strecke> Strecken = Berechnung.Strecken.ToList();
            foreach (Hindernis H in Berechnung.Hindernisse)
            {
                Strecken.AddRange(H.Strecken);
            }

            Dijkstra D = new Dijkstra(Strecken, Berechnung.Startpunkt, Berechnung.Endpunkt);
            Sequenz = D.Berechnung();

            DateTime BusStartzeit = new DateTime(2019, 4, 1, 7, 30, 0);

            double BusStreckeKm = Sequenz[Sequenz.Length - 2].Pos.Y / 1000;
            double BusFahrtzeitStunden = BusStreckeKm / 30.0;
            double BusFahrtzeitSekunden = BusFahrtzeitStunden * 60 * 60;

            double LisaStreckeM = Sequenz[Sequenz.Length - 2].Distanz;
            double LisaStreckeKm = LisaStreckeM / 1000;
            double LisaLaufzeitStunden = LisaStreckeKm / 15.0;
            double LisaLaufzeitSekunden = LisaLaufzeitStunden * 60 * 60;
            TimeSpan LisaLaufzeit = new TimeSpan(0, 0, 0, (int)Math.Round(LisaLaufzeitSekunden));

            DateTime BusEinstieg = BusStartzeit.AddSeconds(BusFahrtzeitSekunden);

            DateTime LisaStartzeit = BusEinstieg.Subtract(LisaLaufzeit);

            int StreckeInt = (int)Math.Round(LisaStreckeM);

            StatusBarItemInfo.Content = "Strecke: " + StreckeInt + "m" + " | " + "Lisa muss um " + LisaStartzeit.ToLongTimeString() + " loslaufen.";
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // EVENT HANDLER
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private void MenuItemAnsicht_Click(object sender, RoutedEventArgs e)
        {
            MenuItem Item = (MenuItem)sender;
            Item.IsChecked = !Item.IsChecked;

            if(Item == MenuItemAnsichtHindernisse)
            {
                bZeichneHindernisse = Item.IsChecked;
            }

            if (Item == MenuItemAnsichtWeg)
            {
                bZeichneWeg = Item.IsChecked;
            }

            if (Item == MenuItemAnsichtStrecken)
            {
                bZeichneInnenStrecken = Item.IsChecked;
                bZeichneAussenStrecken = Item.IsChecked;

                MenuItemAnsichtInnenStrecken.IsChecked = bZeichneInnenStrecken;
                MenuItemAnsichtAussenStrecken.IsChecked = bZeichneAussenStrecken;
            }

            if (Item == MenuItemAnsichtInnenStrecken)
            {
                bZeichneInnenStrecken = Item.IsChecked;

                MenuItemAnsichtStrecken.IsChecked = (bZeichneInnenStrecken && bZeichneAussenStrecken);
            }

            if (Item == MenuItemAnsichtAussenStrecken)
            {
                bZeichneAussenStrecken = Item.IsChecked;

                MenuItemAnsichtStrecken.IsChecked = (bZeichneInnenStrecken && bZeichneAussenStrecken);
            }

            Zeichne();
        }

        private void MenuItemBeenden_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemKollision_Click(object sender, RoutedEventArgs e)
        {
            KollisionDemo Window = new KollisionDemo();
            Window.Show();
        }

        private void MenuItemPIP_Click(object sender, RoutedEventArgs e)
        {
            PIPDemo Window = new PIPDemo();
            Window.Show();
        }
    }
}
