using System.Linq.Expressions;

namespace TourneeFutee
{
    public class Matrix
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private List<List<float>> element;
        private float defaultValue;

        /* Crée une matrice de dimensions `nbRows` x `nbColums`.
         * Toutes les cases de cette matrice sont remplies avec `defaultValue`.
         * Lève une ArgumentOutOfRangeException si une des dimensions est négative
         */
        public Matrix(int nbRows = 0, int nbColumns = 0, float defaultValue = 0)
        {
            if (nbRows < 0 || nbColumns < 0)
            {
                throw new ArgumentOutOfRangeException("Les dimensions de la matrice doivent être positives.");
            }

            element = new List<List<float>>();

            for (int c = 0; c < nbColumns; c++)
            {
                List<float> column = new List<float>();
                for (int r = 0; r < nbRows; r++)
                {
                    column.Add(defaultValue);
                }
                element.Add(column);
            }
        }

        // Propriété : valeur par défaut utilisée pour remplir les nouvelles cases
        // Lecture seule
        public float DefaultValue
        {
            get { return defaultValue; }
                 // pas de set
        }

        // Propriété : nombre de lignes
        // Lecture seule
        public int NbRows
        {
            get 
            { 
                if (element.Count == 0) return 0;
                else return element[0].Count;
            }
                 // pas de set
        }

        // Propriété : nombre de colonnes
        // Lecture seule
        public int NbColumns
        {
            get
            {
                return element.Count;
            }
        }

        /* Insère une ligne à l'indice `i`. Décale les lignes suivantes vers le bas.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `i` = NbRows, insère une ligne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
         */
        public void AddRow(int i)
        {
            // TODO : implémenter
        }

        /* Insère une colonne à l'indice `j`. Décale les colonnes suivantes vers la droite.
         * Toutes les cases de la nouvelle ligne contiennent DefaultValue.
         * Si `j` = NbColums, insère une colonne en fin de matrice
         * Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
         */
        public void AddColumn(int j)
        {
            // TODO : implémenter
        }

        // Supprime la ligne à l'indice `i`. Décale les lignes suivantes vers le haut.
        // Lève une ArgumentOutOfRangeException si `i` est en dehors des indices valides
        public void RemoveRow(int i)
        {
            // TODO : implémenter
        }

        // Supprime la colonne à l'indice `j`. Décale les colonnes suivantes vers la gauche.
        // Lève une ArgumentOutOfRangeException si `j` est en dehors des indices valides
        public void RemoveColumn(int j)
        {
            // TODO : implémenter
        }

        // Renvoie la valeur à la ligne `i` et colonne `j`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public float GetValue(int i, int j)
        {
            // TODO : implémenter
            if (i < 0 || i >= NbRows)
            {
                throw new ArgumentOutOfRangeException(nameof(i), "Indice i invalide");
            }
            else if (j < 0 || j >= NbColumns)
            {
                throw new ArgumentOutOfRangeException(nameof(j), "Indice j invalide");
            }
            else
            {
                return element[i][j];
            }
        }

        // Affecte la valeur à la ligne `i` et colonne `j` à `v`
        // Lève une ArgumentOutOfRangeException si `i` ou `j` est en dehors des indices valides
        public void SetValue(int i, int j, float v)
        {
            // TODO : implémenter
        }

        // Affiche la matrice
        public void Print()
        {
            // TODO : implémenter
        }


        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }


}
