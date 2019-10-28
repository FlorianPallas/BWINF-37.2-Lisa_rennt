using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Aufgabe1
{
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // KLASSE Pfadberechnung
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    class Pfadberechnung
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // VARIABLEN
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        MainWindow Window;
        public Hindernis[] Hindernisse;
        public Strecke[] Strecken;
        public Point Startpunkt;
        public Point Endpunkt;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // KONSTRUKTOR
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public Pfadberechnung(MainWindow _Window)
        {
            Window = _Window;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // METHODEN
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void SetzeHindernisse(Hindernis[] _Hindernisse)
        {
            Hindernisse = _Hindernisse;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void SetzeStartpunkt(Point _Startpunkt)
        {
            Startpunkt = _Startpunkt;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void SetzeInnenstrecken()
        {
            // Für jedes Hindernis
            foreach(Hindernis H in Hindernisse)
            {
                List<Strecke> _Strecken = new List<Strecke>();

                foreach (Point A in H.Eckpunkte)
                {
                    foreach (Point B in H.Eckpunkte)
                    {
                        // Gleiche Start- & Zielpunkte vermeiden
                        if (A == B) { continue; } 

                        // Umgekehrte Strecken ignorieren (A -> B | B -> A)
                        if (_Strecken.FindIndex(X => X.A == B && X.B == A) != -1) { continue; } 

                        // Strecke erstellen
                        Strecke S = new Strecke(A, B, H);

                        // Kanten bestimmen
                        S.Kante = H.IstNachbar(A, B);

                        // Strecke hinzufügen
                        _Strecken.Add(S);
                    }
                }

                // Strecken für das Hindernis setzen
                H.Strecken = _Strecken.ToArray();
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void SetzeAussenstrecken()
        {
            List<Strecke> _Strecken = new List<Strecke>();

            // Strecken zwischen Hindernispunkten
            foreach (Hindernis H1 in Hindernisse)
            {
                foreach (Point A in H1.Eckpunkte)
                {
                    foreach (Hindernis H2 in Hindernisse)
                    {
                        // Selbe Hindernisse ignorieren
                        if (H1 == H2) { continue; }

                        foreach (Point B in H2.Eckpunkte)
                        { 
                            // Umgekehrte Strecken ignorieren (A -> B | B -> A)
                            if (_Strecken.FindIndex(X => X.A == B && X.B == A) != -1) { continue; }

                            _Strecken.Add(new Strecke(A, B));
                        }
                    }
                }
            }

            // Strecken vom Startpunkt
            foreach (Hindernis H in Hindernisse)
            {
                foreach (Point B in H.Eckpunkte)
                {
                    _Strecken.Add(new Strecke(Startpunkt, B));
                }
            }

            // Strecken im Optimalwinkel von jedem Punkt
            _Strecken.Add(new Strecke(Startpunkt, ZielpunktMitOptimalwinkel(Startpunkt)));

            foreach (Hindernis H in Hindernisse)
            {
                foreach (Point A in H.Eckpunkte)
                {
                    Point Endpunkt = ZielpunktMitOptimalwinkel(A);
                    _Strecken.Add(new Strecke(A, Endpunkt) { Endstrecke = true });
                }
            }

            Strecken = _Strecken.ToArray();
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private Point ZielpunktMitOptimalwinkel(Point Startpunkt)
        {
            return new Point(0, Startpunkt.Y + (1 / Math.Sqrt(3) * Startpunkt.X));
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void TesteAussenstrecken()
        {
            // Andere Strecken
            List<Strecke> _Strecken = Strecken.ToList();

            foreach (Strecke AB in Strecken)
            {
                foreach (Hindernis H in Hindernisse)
                {
                    foreach(Strecke S in H.Strecken)
                    {
                        if(!S.Kante) { continue; }

                        Vector ST;
                        if (Kollision(AB, S, out ST))
                        {
                            _Strecken.Remove(AB);
                        }
                    }
                }
            }

            Strecken = _Strecken.ToArray();
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void TesteInnenstreckenInnerhalb()
        {
            // Alle Strecken die innerhalb eines Polygons mit einer Kante kollidieren
            foreach (Hindernis H in Hindernisse)
            {
                List<Strecke> _Strecken = H.Strecken.ToList();

                foreach (Strecke AB in H.Strecken)
                {
                    // Kanten sind immer valide
                    if (AB.Kante) { continue; }
                    foreach (Strecke PQ in H.Strecken)
                    {
                        // Nur mit Kanten Kollision testen
                        if (AB == PQ) { continue; }
                        if (!PQ.Kante) { continue; }

                        Vector ST;
                        if (Kollision(AB, PQ, out ST))
                        {
                            _Strecken.Remove(AB);
                        }
                    }
                }

                H.Strecken = _Strecken.ToArray();
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void TesteInnenstreckenAusserhalb()
        {
            // Alle Strecken die mit anderen Polygonkanten kollidieren
            foreach (Hindernis H1 in Hindernisse)
            {
                List<Strecke> _Strecken = H1.Strecken.ToList();

                foreach (Strecke AB in H1.Strecken)
                {
                    // Kanten sind immer valide
                    if (AB.Kante) { continue; }
                    foreach (Hindernis H2 in Hindernisse)
                    {
                        foreach (Strecke PQ in H2.Strecken)
                        {
                            // Nur mit Kanten Kollision testen
                            if (H1 == H2) { continue; }
                            if (!PQ.Kante) { continue; }

                            Vector ST;
                            if (Kollision(AB, PQ, out ST))
                            {
                                _Strecken.Remove(AB);
                            }
                        }
                    }
                }

                H1.Strecken = _Strecken.ToArray();
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void PunktInPolygon()
        {
            // Punkt-in-Polygon-Test nach Jordan
            const double Epsilon = 0.000001;
            const double Laenge = 100000;
            Random R = new Random();

            foreach (Hindernis H in Hindernisse)
            {
                List<Strecke> _Strecken = H.Strecken.ToList();

                foreach(Strecke AB in H.Strecken)
                {
                    // Kanten sind valide
                    if(AB.Kante) { continue; }

                    int Anzahl = 0;

                    // Startpunkt - Epsilon * Vektor von Punkt aus
                    Point X = AB.A + Epsilon * AB.Vektor();

                    // Richtung bestimmen
                    Vector Richtung;
                    Vector VAB = AB.Vektor();
                    Vector VBA = new Vector(VAB.Y, VAB.X);

                    Richtung = new Vector(1, 0);
                    bool IstGueltig;

                    // Richtung so lange variieren bis sie gültig ist
                    while (true)
                    {
                        IstGueltig = true;

                        foreach (Strecke PQ in H.Strecken)
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
                    
                    // Strahlstrecke erstellen
                    Strecke Strahl = new Strecke(X, X + Richtung * Laenge);

                    // Kollision testen
                    foreach (Strecke PQ in H.Strecken)
                    {
                        if (!PQ.Kante) { continue; }
                        if (AB == PQ) { continue; }

                        Vector ST;
                        if (Kollision(Strahl, PQ, out ST))
                        {
                            Anzahl++;
                        }
                    }

                    // Wenn die Anzahl and Treffern nicht gerade ist -> im Polygon
                    if (Anzahl % 2 != 0)
                    {
                        _Strecken.Remove(AB);
                    }
                }

                H.Strecken = _Strecken.ToArray();
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public void SetzeEndstrecken()
        {
            List<Strecke> _Strecken = Strecken.ToList();

            // Höchsten Endpunkt bestimmen
            double HoechsterEndpunkt = double.NegativeInfinity;
            foreach (Strecke S in Strecken)
            {
                if (!S.Endstrecke) { continue; }

                if(S.B.Y >= HoechsterEndpunkt)
                {
                    HoechsterEndpunkt = S.B.Y;
                }
            }

            // Festen Betrag setzen
            foreach (Strecke S in Strecken)
            {
                if(!S.Endstrecke) { continue; }

                Endpunkt = new Point(-10, 0);
                Strecke SEndpunkt = new Strecke(S.B, Endpunkt);
                SEndpunkt.HatFestenBetrag = true;
                SEndpunkt.FesterBetrag = (HoechsterEndpunkt - S.B.Y) / 2;

                _Strecken.Add(SEndpunkt);
            }

            Strecken = _Strecken.ToArray();
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private bool Kollision(Strecke SAB, Strecke SPQ, out Vector ST)
        {
            // Matrix erstellen
            Vector BA = new Vector(SAB.A.X - SAB.B.X, SAB.A.Y - SAB.B.Y);
            Vector PQ = new Vector(SPQ.B.X - SPQ.A.X, SPQ.B.Y - SPQ.A.Y);
            Vector PA = new Vector(SAB.A.X - SPQ.A.X, SAB.A.Y - SPQ.A.Y);

            Matrix M = new Matrix(PQ.X, BA.X, PQ.Y, BA.Y);

            // Eindeutig lösbar?
            // Wenn Determinante 0 -> Keine Kollision / Macht invertieren möglich
            if (M.Determinante() == 0.0)
            {
                ST = new Vector();
                return false;
            }

            // Schnittvektor - Möglich weil Determinante != 0
            Vector X = M.Invertiert().VektorMult(PA);

            double s = X.X;
            double t = X.Y;

            ST = new Vector(s, t);

            // Typ double kann zu Ungenauigkeiten führen
            const double Eps = 1E-12;

            // t: 0 und 1 -> Keine Kollision (Epsilon)
            if ((s < 0.0) || (s > 1.0) || (t <= Eps) || (t >= 1.0 - Eps)) { return false; }

            return true;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }

    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // HILFSKLASSEN
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    class Hindernis
    {
        public Hindernis() {}
        public Hindernis(Point[] _Eckpunkte)
        {
            Eckpunkte = _Eckpunkte;
        }

        public Point[] Eckpunkte;
        public Strecke[] Strecken;

        public bool IstNachbar(Point A, Point B)
        {
            // Indeces der Punkte bestimmen
            int IndexA = Array.IndexOf(Eckpunkte, A);
            int IndexB = Array.IndexOf(Eckpunkte, B);

            // Verbindungen vom ersten und letztem Endpunkt zählen als Kante
            if(
                (IndexA == 0 && IndexB == Eckpunkte.Length - 1) || 
                (IndexB == 0 && IndexA == Eckpunkte.Length - 1))
            { return true; }

            // Nachbar Indeces bedeuten eine Kante
            if(IndexA - IndexB == 1 || IndexA - IndexB == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    class Strecke
    {
        public Strecke(Point _A, Point _B)
        {
            A = _A;
            B = _B;
            Hindernis = null;
        }

        public Strecke(Point _A, Point _B, Hindernis _Hindernis)
        {
            A = _A;
            B = _B;
            Hindernis = _Hindernis;
        }

        public Vector Vektor()
        {
            return new Vector()
            {
                X = B.X - A.X,
                Y = B.Y - A.Y
            };
        }

        public double Betrag()
        {
            if (HatFestenBetrag)
            {
                return FesterBetrag;
            }

            Vector AB = Vektor();
            return Math.Sqrt(AB.X * AB.X + AB.Y * AB.Y);
        }

        public bool Endstrecke = false;
        public bool HatFestenBetrag = false;
        public double FesterBetrag;
        public bool Kante = false;
        public Point A;
        public Point B;
        public Hindernis Hindernis;
    }

    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    class Matrix
    {
        public Matrix(double _A, double _B, double _C, double _D)
        {
            A = _A;
            B = _B;
            C = _C;
            D = _D;
        }

        public double A;
        public double B;
        public double C;
        public double D;

        public Vector VektorMult(Vector V)
        {
            return new Vector(A * V.X + B * V.Y, C * V.X + D * V.Y);
        }

        public Matrix Mult(double X)
        {
            return new Matrix(A * X, B * X, C * X, D * X);
        }

        public double Determinante()
        {
            return A * D - B * C;
        }

        public Matrix Invertiert()
        {
            Matrix M = new Matrix(D, -B, -C, A);
            return M.Mult(1 / Determinante());
        }

        public string Zeige()
        {
            return "A: " + A + " B: " + B + Environment.NewLine + "C: " + C + " D: " + D;
        }
    }

    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
}
