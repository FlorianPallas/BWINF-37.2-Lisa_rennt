using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aufgabe1
{
    /// <summary>
    /// Interaction logic for KollisionDemo.xaml
    /// </summary>
    public partial class PIPDemo : Window
    {
        private double LineThickness;
        private Line TempLine;
        private Polygon TempPoly;
        private Ellipse TempEllipse;
        private int Mode;
        private int Tool;

        public PIPDemo()
        {
            InitializeComponent();

            TempEllipse = null;
            Mode = 0;
            LineThickness = 2;

            TempLine = new Line();
            TempLine.Stroke = Brushes.Black;
            TempLine.StrokeThickness = LineThickness;
            ZeichenCanvas.Children.Add(TempLine);
        }

        private bool Kollision(Strecke SAB, Strecke SPQ, out Vector ST)
        {
            // Matrix erstellen
            Vector BA = new Vector(SAB.A.X - SAB.B.X, SAB.A.Y - SAB.B.Y);
            Vector PQ = new Vector(SPQ.B.X - SPQ.A.X, SPQ.B.Y - SPQ.A.Y);
            Vector PA = new Vector(SAB.A.X - SPQ.A.X, SAB.A.Y - SPQ.A.Y);

            Matrix M = new Matrix(PQ.X, BA.X, PQ.Y, BA.Y);

            // Wenn Determinante 0 -> Keine Kollision
            if (M.Determinante() == 0.0)
            {
                ST = new Vector();
                return false;
            }

            // Schnittvektor
            Vector X = M.Invertiert().VektorMult(PA);

            double s = X.X;
            double t = X.Y;

            ST = new Vector(s, t);

            // Typ double kann zu Ungenauigkeiten führen
            const double Eps = 1E-12;

            if ((s < 0.0) || (t <= Eps) || (s > 1.0) || (t >= 1.0 - Eps)) { return false; }

            return true;
        }

        private void ZeichenCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point Pos = e.GetPosition(ZeichenCanvas);

            if(Tool == 0)
            {
                switch (Mode)
                {
                    case 0:
                        {
                            TempEllipse = new Ellipse();
                            TempEllipse.Fill = Brushes.Black;
                            TempEllipse.Width = 10;
                            TempEllipse.Height = 10;

                            Canvas.SetTop(TempEllipse, Pos.Y - TempEllipse.Height / 2);
                            Canvas.SetLeft(TempEllipse, Pos.X - TempEllipse.Width / 2);

                            ZeichenCanvas.Children.Add(TempEllipse);

                            Mode = 1;
                            break;
                        }
                    case 1:
                        {
                            
                            Mode = 0;
                            break;
                        }
                }
            }

            if (Tool == 1)
            {
                switch (Mode)
                {
                    case 0:
                        {
                            TempPoly = new Polygon();
                            ZeichenCanvas.Children.Add(TempPoly);
                            TempPoly.Points.Add(Pos);
                            TempPoly.Points.Add(Pos);
                            TempPoly.Stroke = Brushes.Black;
                            TempPoly.StrokeThickness = LineThickness;
                            Mode = 1;
                            break;
                        }
                    case 1:
                        {
                            Point LastPoint = TempPoly.Points[TempPoly.Points.Count - 1];

                            LastPoint = Pos;
                            TempPoly.Points.Add(Pos);
                            break;
                        }
                }
            }
        }

        private void MenuItemClear_Click(object sender, RoutedEventArgs e)
        {
            ZeichenCanvas.Children.Clear();

            ZeichenCanvas.Children.Add(TempLine);
        }

        private void ZeichenCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point Pos = e.GetPosition(ZeichenCanvas);

            if (Mode == 1 && Tool == 0)
            {
                TempLine.X1 = Pos.X;
                TempLine.Y1 = Pos.Y;
                Canvas.SetTop(TempEllipse, Pos.Y - TempEllipse.Height / 2);
                Canvas.SetLeft(TempEllipse, Pos.X - TempEllipse.Width / 2);
                PunktInPolygon(Pos);
            }

            if (Mode == 1 && Tool == 1)
            {
                TempPoly.Points[TempPoly.Points.Count - 1] = Pos;
            }
        }

        private void MenuItemIncrease_Click(object sender, RoutedEventArgs e)
        {
            LineThickness += 1;
        }

        private void MenuItemDecrease_Click(object sender, RoutedEventArgs e)
        {
            LineThickness -= 1;
        }

        private void MenuItemPoint_Click(object sender, RoutedEventArgs e)
        {
            Tool = 0;
        }

        private void MenuItemPolygon_Click(object sender, RoutedEventArgs e)
        {
            Tool = 1;
        }

        private void ZeichenCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(Tool == 1 && Mode == 1)
            {
                Mode = 0;
            }
        }

        private bool PunktInPolygon(Point Pos)
        {
            List<Strecke> Strecken = new List<Strecke>();
            foreach (UIElement El in ZeichenCanvas.Children)
            {
                if(El is Polygon)
                {
                    Polygon Poly = (Polygon)El;
                    for (int I = 0; I < Poly.Points.Count - 1; I++)
                    {
                        Point P1 = Poly.Points[I];
                        Point P2 = Poly.Points[I + 1];

                        Strecken.Add(new Strecke(P1, P2));
                    }

                    Strecken.Add(new Strecke(Poly.Points[Poly.Points.Count - 1], Poly.Points[0]));
                }
            }

            const double Laenge = 100000;
            Random R = new Random();

            int Anzahl = 0;

            // Startpunkt - Epsilon * Vektor von Punkt aus
            Point X = Pos;

            Vector Richtung;

            Richtung = new Vector(1, 0);
            bool IstGueltig;

            // Richtung so lange variieren bis sie gültig ist
            while (true)
            {
                IstGueltig = true;

                foreach (Strecke PQ in Strecken)
                {
                    Vector VPQ = PQ.B - PQ.A;
                    Vector VQP = PQ.A - PQ.B;

                    // Wenn parallel
                    if (Vector.Determinant(Richtung, VPQ) == 0 || Vector.Determinant(Richtung, VQP) == 0)
                    {
                        IstGueltig = false;
                        break;
                    }
                }

                // Abbrechen wenn gültig
                if (!IstGueltig)
                {
                    Richtung = new Vector(R.NextDouble(), R.NextDouble());
                    continue;
                }
                else
                {
                    break;
                }
            }

            Point PEnd = X + Richtung * Laenge;

            // Strahlstrecke erstellen
            Strecke Strahl = new Strecke(X, PEnd);

            TempLine.X1 = X.X;
            TempLine.Y1 = X.Y;
            TempLine.X2 = PEnd.X;
            TempLine.Y2 = PEnd.Y;

            // Kollision testen
            foreach (Strecke PQ in Strecken)
            {
                Vector ST;
                if (Kollision(Strahl, PQ, out ST))
                {
                    Anzahl++;
                }
            }

            // Wenn die Anzahl and Treffern nicht gerade ist -> im Polygon
            if (Anzahl % 2 != 0)
            {
                TempLine.Stroke = Brushes.Red;
                TempEllipse.Fill = Brushes.Red;
                return true;
            }
            else
            {
                TempLine.Stroke = Brushes.Green;
                TempEllipse.Fill = Brushes.Green;
                return false;
            }
        }
    }
}
