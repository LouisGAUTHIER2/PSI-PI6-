using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    struct Node
    {
        public Tour tour;
        public Matrix m;
        public List<int> vertexColumnID;
        public List<int> vertexRowID;
        public float cost;
    }

    // Résout le problème de voyageur de commerce défini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private Graph graph;


        // Instancie le planificateur en spécifiant le graphe modélisant un problème de voyageur de commerce
        public Little(Graph graph)
        {
            this.graph = graph;
        }

        // Trouve la tournée optimale dans le graphe `this.graph`
        // (c'est à dire le cycle hamiltonien de plus faible coût)
        public Tour ComputeOptimalTour()
        {
            Tour tour = new Tour(graph, new List<(string source, string destination)>());
            Matrix m = GetCostMatrix();
            List<int> vertexColumnID = new List<int>();
            List<int> vertexRowID = new List<int>();

            Queue<Node> queue = new Queue<Node>();

            for (int k = 0; k < graph.Order; k++)
            {
                vertexColumnID.Add(k);
                vertexRowID.Add(k);
            }

            float reductionCost = ReduceMatrix(m);

            // on commence par tester en ajout
            float cost = reductionCost;

            while (m.NbRows > 2)
            {
                (int i, int j, float value) = GetMaxRegret(m);
                m.SetValue(i, j, float.PositiveInfinity);

                queue.Enqueue(new Node
                {
                    tour = new Tour(tour),
                    m = new Matrix(m),
                    vertexColumnID = new List<int>(vertexColumnID),
                    vertexRowID = new List<int>(vertexRowID),
                    cost = cost + value
                });

                // ajout du segment dans tour
                tour.AddSegment((graph.GetVertexName(vertexRowID[i]), graph.GetVertexName(vertexColumnID[j])));

                // suppression de la ligne i et de la colonne j
                m.RemoveColumn(j);
                m.RemoveRow(i);
                vertexColumnID.RemoveAt(j);
                vertexRowID.RemoveAt(i);

                reductionCost = ReduceMatrix(m);

                // suppression des segments interdits
                EliminateSubtours(vertexRowID, vertexColumnID, m, tour);

                cost += reductionCost;
            }
            EliminateSubtours(vertexRowID, vertexColumnID, m, tour);

            // on ajoute les deux derniers segments
            for (int a = 0; a < vertexRowID.Count; a++)
            {
                for (int b = 0; b < vertexColumnID.Count; b++)
                {
                    if (m.GetValue(a, b) != float.PositiveInfinity)
                    {
                        tour.AddSegment((graph.GetVertexName(vertexRowID[a]), graph.GetVertexName(vertexColumnID[b])));
                    }
                }
            }

            return tour;
        }

        // --- Méthodes utilitaires réalisant des étapes de l'algorithme de Little


        // Réduit la matrice `m` et revoie la valeur totale de la réduction
        // Après appel à cette méthode, la matrice `m` est *modifiée*.
        public static float ReduceMatrix(Matrix m)
        {
            float total = 0;
            for (int i = 0; i < m.NbRows; i++) // pour les lignes
            {
                float min = float.MaxValue;
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) < min)
                    {
                        min = m.GetValue(i, j);
                    }
                }
                for (int j = 0; j < m.NbColumns; j++)
                {
                    m.SetValue(i, j, m.GetValue(i, j) - min);
                }
                total += min;
            }
            for (int i = 0; i < m.NbColumns; i++) // pour les colonnes
            {
                float min = float.MaxValue;
                for (int j = 0; j < m.NbRows; j++)
                {
                    if (m.GetValue(j, i) < min)
                    {
                        min = m.GetValue(j, i);
                    }
                }
                for (int j = 0; j < m.NbRows; j++)
                {
                    m.SetValue(j, i, m.GetValue(j, i) - min);
                }
                total += min;
            }
            return total;
        }

        // Renvoie le regret de valeur maximale dans la matrice de coûts `m` sous la forme d'un tuple `(int i, int j, float value)`
        // où `i`, `j`, et `value` contiennent respectivement la ligne, la colonne et la valeur du regret maximale
        public static (int i, int j, float value) GetMaxRegret(Matrix m)
        {
            int maxi = 0;
            int a = 0;
            int b = 0;
            for (int i = 0; i < m.NbRows; i++)
            {
                for (int j = 0; j < m.NbColumns; j++)
                {
                    if (m.GetValue(i, j) == 0)
                    {
                        if (maxi < MinLigne(m, i, j) + MinColonne(m, i, j))
                        {
                            maxi = (MinLigne(m, i, j) + MinColonne(m, i, j));
                            a = i;
                            b = j;
                        }
                    }
                }
            }
            return (a, b, maxi);
        }

        public static int MinLigne(Matrix m, int i, int b)
        {
            float min = float.MaxValue;
            for (int j = 0; j < m.NbColumns; j++)
            {
                if (m.GetValue(i, j) < min && j != b)
                {
                    min = m.GetValue(i, j);
                }
            }
            return (int)min;
        }

        public static int MinColonne(Matrix m, int a, int j)
        {
            float min = float.MaxValue;
            for (int i = 0; i < m.NbRows; i++)
            {
                if (m.GetValue(i, j) < min && i != a)
                {
                    min = m.GetValue(i, j);
                }
            }
            return (int)min;
        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {
            (string source, string destination) currentSegment = segment;
            int nbCitiesVisited = 1;

            while (nbCitiesVisited < nbCities)
            {
                (string source, string destination) newSegment = currentSegment;

                if (currentSegment.destination == segment.source)
                {
                    break;
                }

                //on cherche un nouveau segment
                foreach ((string source, string destination) testSegment in includedSegments)
                {
                    if (testSegment.source == currentSegment.destination)
                    {
                        newSegment = testSegment;
                        break;
                    }
                }

                // si on ne trouve pas de nouveau segment, on arrête la recherche
                if (currentSegment == newSegment)
                {
                    return false;
                }

                currentSegment = newSegment;
                nbCitiesVisited++;
            }

            return nbCitiesVisited < nbCities;
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 
        Matrix GetCostMatrix()
        {
            Matrix m = new Matrix(graph.Order, graph.Order);
            for (int i = 0; i < graph.Order; i++)
            {
                for (int j = 0; j < graph.Order; j++)
                {
                    m.SetValue(i, j, graph.AdjacencyMatrix.GetValue(i, j));

                    if (i == j)
                    {
                        m.SetValue(i, j, float.PositiveInfinity);
                    }
                }
            }
            return m;
        }

        public void EliminateSubtours(List<int> vertexRowID, List<int> vertexColumnID, Matrix m, Tour tour)
        {
            for (int a = 0; a < vertexRowID.Count; a++)
            {
                for (int b = 0; b < vertexColumnID.Count; b++)
                {
                    if (IsForbiddenSegment((graph.GetVertexName(vertexRowID[a]), graph.GetVertexName(vertexColumnID[b])), tour.segments, graph.Order))
                    {
                        m.SetValue(a, b, float.PositiveInfinity);
                    }
                }
            }
        }
    }
}
