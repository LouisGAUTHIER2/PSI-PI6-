namespace TourneeFutee
{
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
            // TODO : implémenter
        }

        // Trouve la tournée optimale dans le graphe `this.graph`
        // (c'est à dire le cycle hamiltonien de plus faible coût)
        public Tour ComputeOptimalTour()
        {
            // TODO : implémenter
            return new Tour();
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
            for (int i = 0; i <m.NbRows; i++)
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

    }
}
