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
    public partial class KollisionDemo : Window
    {
        private double LineThickness;
        private Line TempLine;
        private int Mode;

        public KollisionDemo()
        {
            InitializeComponent();

            TempLine = null;
            Mode = 0;
            LineThickness = 2;
        }

        private void CheckCollision()
        {
            foreach (UIElement El1 in ZeichenCanvas.Children)
            {
                if (!(El1 is Line)) { continue; }
                Line L1 = (Line)El1;

                L1.Stroke = Brushes.Black;
                L1.StrokeThickness = LineThickness;

                foreach (UIElement El2 in ZeichenCanvas.Children)
                {
                    if (!(El1 is Line)) { continue; }
                    Line L2 = (Line)El2;

                    Strecke S1 = new Strecke(new Point(L1.X1, L1.Y1), new Point(L1.X2, L1.Y2));
                    Strecke S2 = new Strecke(new Point(L2.X1, L2.Y1), new Point(L2.X2, L2.Y2));
                    Vector ST;

                    bool Collides = Kollision(S1, S2, out ST);

                    if(Collides)
                    {
                        L1.Stroke = Brushes.Red;
                    }
                }

                if(L1.Stroke != Brushes.Red)
                {
                    L1.Stroke = Brushes.Green;
                }
            }
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

            switch (Mode)
            {
                case 0:
                    {
                        TempLine = new Line();
                        ZeichenCanvas.Children.Add(TempLine);
                        TempLine.X1 = Pos.X;
                        TempLine.Y1 = Pos.Y;
                        TempLine.Stroke = Brushes.Black;
                        TempLine.StrokeThickness = LineThickness;
                        Mode = 1;
                        break;
                    }
                case 1:
                    {
                        TempLine.X2 = Pos.X;
                        TempLine.Y2 = Pos.Y;
                        Mode = 0;
                        break;
                    }
            }

            CheckCollision();
        }

        private void MenuItemClear_Click(object sender, RoutedEventArgs e)
        {
            ZeichenCanvas.Children.Clear();
        }

        private void ZeichenCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point Pos = e.GetPosition(ZeichenCanvas);

            if (Mode == 1)
            {
                TempLine.X2 = Pos.X;
                TempLine.Y2 = Pos.Y;
                CheckCollision();
            }
        }

        private void MenuItemIncrease_Click(object sender, RoutedEventArgs e)
        {
            LineThickness += 1;
            CheckCollision();
        }

        private void MenuItemDecrease_Click(object sender, RoutedEventArgs e)
        {
            LineThickness -= 1;
            CheckCollision();
        }
    }
}
