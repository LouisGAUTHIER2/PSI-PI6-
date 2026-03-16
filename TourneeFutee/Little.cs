namespace TourneeFutee
{
    // Résout le problème de voyageur de commerce défini par le graphe `graph`
    // en utilisant l'algorithme de Little
    public class Little
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 

        // Instancie le planificateur en spécifiant le graphe modélisant un problème de voyageur de commerce
        public Little(Graph graph)
        {
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
            // TODO : implémenter
            return (0, 0, 0.0f);

        }

        /* Renvoie vrai si le segment `segment` est un trajet parasite, c'est-à-dire s'il ferme prématurément la tournée incluant les trajets contenus dans `includedSegments`
         * Une tournée est incomplète si elle visite un nombre de villes inférieur à `nbCities`
         */
        public static bool IsForbiddenSegment((string source, string destination) segment, List<(string source, string destination)> includedSegments, int nbCities)
        {

            // TODO : implémenter
            return false;   
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }
}
