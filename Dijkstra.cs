using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aufgabe1
{
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // KLASSE Dijkstra
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    class Dijkstra
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // VARIABLEN
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private List<Strecke> Strecken;
        private Point Startpunkt;
        private Point Endpunkt;
        private DijkstraVertex Startvertex;
        private DijkstraVertex Endvertex;
        private List<DijkstraVertex> Vertices;
        public List<DijkstraVertex> Sequenz;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // KONSTRUKTOR
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public Dijkstra(List<Strecke> _Strecken, Point _Startpunkt, Point _Endpunkt)
        {
            Strecken = _Strecken;
            Startpunkt = _Startpunkt;
            Endpunkt = _Endpunkt;
            Vertices = new List<DijkstraVertex>();

            Setup();
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // METHODEN
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private void Setup()
        {
            // Alle Vertices erstellen
            foreach(Strecke S in Strecken)
            {
                // Vertex nur hinzufügen wenn sie nicht schon existiert
                if(!Vertices.Exists(x => x.Pos == S.A))
                {
                    Vertices.Add(new DijkstraVertex() { Pos = S.A });
                }

                if (!Vertices.Exists(x => x.Pos == S.B))
                {
                    Vertices.Add(new DijkstraVertex() { Pos = S.B });
                }
            }

            // Nachbarn setzen
            foreach (DijkstraVertex V in Vertices)
            {
                List<DijkstraVertex> Nachbarn = new List<DijkstraVertex>();
                List<double> NachbarWerte = new List<double>();

                // Strecken finden die mit diesem Knoten verbunden sind (Am Punkt A)
                List<Strecke> NachbarStreckenA = Strecken.FindAll(x => x.A == V.Pos);
                foreach(Strecke S in NachbarStreckenA)
                {
                    // Nachbarknoten setzen
                    Nachbarn.Add(Vertices.Find(x => x.Pos == S.B));
                    // Weg zu Nachbarknoten setzen
                    NachbarWerte.Add(S.Betrag());
                }

                // Strecken finden die mit diesem Knoten verbunden sind (Am Punkt B)
                List<Strecke> NachbarStreckenB = Strecken.FindAll(x => x.B == V.Pos);
                foreach (Strecke S in NachbarStreckenB)
                {
                    // Nachbarknoten setzen
                    Nachbarn.Add(Vertices.Find(x => x.Pos == S.A));
                    // Weg zu Nachbarknoten setzen
                    NachbarWerte.Add(S.Betrag());
                }

                // Listen setzen
                V.Nachbarn = Nachbarn;
                V.NachbarWerte = NachbarWerte;
            }

            // Start- und Endvertex bestimmen
            Startvertex = Vertices.Find(x => x.Pos == Startpunkt);
            Endvertex = Vertices.Find(x => x.Pos == Endpunkt);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public DijkstraVertex[] Berechnung()
        {
            // Q - Alle unbesuchten Vertices
            List<DijkstraVertex> Q = new List<DijkstraVertex>();

            // Vertx Startwerte setzen
            foreach(DijkstraVertex V in Vertices)
            {
                V.Distanz = double.PositiveInfinity;
                V.Vorgaenger = null;

                // Alle zu Q hinzufügen
                Q.Add(V);
            }

            Startvertex.Distanz = 0;

            while(Q.Count > 0)
            {
                // Nächste Vertex
                DijkstraVertex U = VertexMitGeringsterDistanz(Q);

                // Als besucht markieren
                Q.Remove(U);

                // Abbruchbedingung
                if (U == Endvertex)
                {
                    Berchnung_SchnellsterWeg();
                    return Sequenz.ToArray();
                }

                // Nachbarwerte setzen
                foreach (DijkstraVertex V in U.Nachbarn)
                {
                    double Alt = U.Distanz + U.WegZu(V);
                    if(Alt < V.Distanz)
                    {
                        V.Distanz = Alt;
                        V.Vorgaenger = U;
                    }
                }
            }

            return null;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private void Berchnung_SchnellsterWeg()
        {
            List<DijkstraVertex> S = new List<DijkstraVertex>();
            DijkstraVertex U = Endvertex;

            if(U.Vorgaenger != null || U == Startvertex)
            {
                while(U != null)
                {
                    S.Add(U);
                    U = U.Vorgaenger;
                }
            }

            S.Reverse();

            Sequenz = S;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private DijkstraVertex VertexMitGeringsterDistanz(List<DijkstraVertex> Q)
        {
            DijkstraVertex BesteVertex = null;
            foreach(DijkstraVertex V in Q)
            {
                if(BesteVertex == null || V.Distanz < BesteVertex.Distanz)
                {
                    BesteVertex = V;
                }
            }

            return BesteVertex;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }

    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // KLASSE DijkstraVertex
    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    class DijkstraVertex
    {
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // VARIABLEN
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public List<DijkstraVertex> Nachbarn;
        public List<double> NachbarWerte;
        public Point Pos;
        public DijkstraVertex Vorgaenger;
        public double Distanz;

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // METHODEN
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public double WegZu(DijkstraVertex _Nachbar)
        {
            int Index = Nachbarn.FindIndex(x => x == _Nachbar);

            return NachbarWerte[Index];
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }
}
