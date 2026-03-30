namespace TourneeFutee
{
    public class Graph
    {

        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private bool directed;
        private Matrix adjacencyMatrix;
        private Dictionary<string, int> nomSommet;
        private Dictionary<string, float> valeurSommet;

        // --- Construction du graphe ---

        // Contruit un graphe (`directed`=true => orienté)
        // La valeur `noEdgeValue` est le poids modélisant l'absence d'un arc (0 par défaut)
        public Graph(bool directed, float noEdgeValue = 0)
        {
            this.directed = directed;
            this.adjacencyMatrix = new Matrix(0, 0, noEdgeValue);
            this.nomSommet = new Dictionary<string, int>();
            this.valeurSommet = new Dictionary<string, float>();
        }


        // --- Propriétés ---

        // Propriété : ordre du graphe
        // Lecture seule
        public int Order
        {
            get
            {
                return nomSommet.Count;
            }
                    // pas de set
        }

        // Propriété : graphe orienté ou non
        // Lecture seule
        public bool Directed
        {
            get
            {
                return directed;
            }
                    // pas de set
        }

        public Matrix AdjacencyMatrix
        {
            get
            {
                return adjacencyMatrix;
            }
        }


        // --- Gestion des sommets ---

        // Ajoute le sommet de nom `name` et de valeur `value` (0 par défaut) dans le graphe
        // Lève une ArgumentException s'il existe déjà un sommet avec le même nom dans le graphe
        public void AddVertex(string name, float value = 0)
        {
            if (nomSommet.ContainsKey(name)) throw new ArgumentException($"Le sommet de nom {name} existe déjà dans le graphe.");

            nomSommet.Add(name, Order); // le nouvel indice du sommet est égal à l'ordre du graphe avant son ajout
            valeurSommet.Add(name, value);

            adjacencyMatrix.AddRow(adjacencyMatrix.NbRows);
            adjacencyMatrix.AddColumn(adjacencyMatrix.NbColumns);
        }


        // Supprime le sommet de nom `name` du graphe (et tous les arcs associés)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void RemoveVertex(string name)
        {
            if (!nomSommet.ContainsKey(name)) throw new ArgumentException($"Le sommet de nom {name} n'existe pas dans le graphe.");

            int index = nomSommet[name];

            // Supprimer le sommet de la matrice d'adjacence
            adjacencyMatrix.RemoveRow(index);
            adjacencyMatrix.RemoveColumn(index);

            // Supprimer le sommet des dictionnaires
            nomSommet.Remove(name);
            valeurSommet.Remove(name);

            // Mettre à jour les indices des sommets restants
            foreach (var key in nomSommet.Keys.ToList())
            {
                if (nomSommet[key] > index)
                {
                    nomSommet[key]--;
                }
            }
        }

        // Renvoie la valeur du sommet de nom `name`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public float GetVertexValue(string name)
        {
            if (!nomSommet.ContainsKey(name)) throw new ArgumentException($"Le sommet de nom {name} n'existe pas dans le graphe.");

            return valeurSommet[name];
        }

        // Affecte la valeur du sommet de nom `name` à `value`
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public void SetVertexValue(string name, float value)
        {
            if (!nomSommet.ContainsKey(name)) throw new ArgumentException($"Le sommet de nom {name} n'existe pas dans le graphe.");

            valeurSommet[name] = value;
        }


        // Renvoie la liste des noms des voisins du sommet de nom `vertexName`
        // (si ce sommet n'a pas de voisins, la liste sera vide)
        // Lève une ArgumentException si le sommet n'a pas été trouvé dans le graphe
        public List<string> GetNeighbors(string vertexName)
        {
            if (!nomSommet.ContainsKey(vertexName)) throw new ArgumentException($"Le sommet de nom {vertexName}Il n'existe pas ");

            List<string> neighborNames = new List<string>();
            int vertexIndex = nomSommet[vertexName];

            for (int i = 0; i < Order; i++)
            {
                if (i == vertexIndex) continue;
                string name = nomSommet.FirstOrDefault(x => x.Value == i).Key;
                if (adjacencyMatrix.GetValue(vertexIndex, i) != 0 || (adjacencyMatrix.GetValue(i, vertexIndex) != 0 && !directed)) neighborNames.Add(name);
            }

            return neighborNames;
        }

        // --- Gestion des arcs ---

        /* Ajoute un arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`, avec le poids `weight` (1 par défaut)
         * Si le graphe n'est pas orienté, ajoute aussi l'arc inverse, avec le même poids
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - il existe déjà un arc avec ces extrémités
         */
        public void AddEdge(string sourceName, string destinationName, float weight = 1)
        {
            if (!nomSommet.ContainsKey(sourceName)) throw new ArgumentException($"Le sommet de nom {sourceName} n'existe pas dans le graphe.");
            if (!nomSommet.ContainsKey(destinationName)) throw new ArgumentException($"Le sommet de nom {destinationName} n'existe pas dans le graphe.");

            int sourceIndex = nomSommet[sourceName];
            int destinationIndex = nomSommet[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) != 0) throw new ArgumentException($"Il existe déjà un arc allant de {sourceName} à {destinationName}.");

            adjacencyMatrix.SetValue(sourceIndex, destinationIndex, weight);

            if (!directed)
            {
                if (adjacencyMatrix.GetValue(destinationIndex, sourceIndex) != 0) throw new ArgumentException($"Il existe déjà un arc allant de {destinationName} à {sourceName}.");

                adjacencyMatrix.SetValue(destinationIndex, sourceIndex, weight);
            }
        }

        /* Supprime l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` du graphe
         * Si le graphe n'est pas orienté, supprime aussi l'arc inverse
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public void RemoveEdge(string sourceName, string destinationName)
        {
            if (!nomSommet.ContainsKey(sourceName)) throw new ArgumentException($"Le sommet de nom {sourceName} n'existe pas dans le graphe.");
            if (!nomSommet.ContainsKey(destinationName)) throw new ArgumentException($"Le sommet de nom {destinationName} n'existe pas dans le graphe.");

            int sourceIndex = nomSommet[sourceName];
            int destinationIndex = nomSommet[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) == 0) throw new ArgumentException($"Il n'existe pas d'arc allant de {sourceName} à {destinationName}.");

            adjacencyMatrix.SetValue(sourceIndex, destinationIndex, 0);

            if (!directed)
            {
                if (adjacencyMatrix.GetValue(destinationIndex, sourceIndex) == 0) throw new ArgumentException($"Il n'existe pas d'arc allant de {destinationName} à {sourceName}.");

                adjacencyMatrix.SetValue(destinationIndex, sourceIndex, 0);
            }
        }

        /* Renvoie le poids de l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName`
         * Si le graphe n'est pas orienté, GetEdgeWeight(A, B) = GetEdgeWeight(B, A) 
         * Lève une ArgumentException dans les cas suivants :
         * - un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         * - l'arc n'existe pas
         */
        public float GetEdgeWeight(string sourceName, string destinationName)
        {
            if (!nomSommet.ContainsKey(sourceName)) throw new ArgumentException($"Le sommet de nom {sourceName} n'existe pas dans le graphe.");
            if (!nomSommet.ContainsKey(destinationName)) throw new ArgumentException($"Le sommet de nom {destinationName} n'existe pas dans le graphe.");

            int sourceIndex = nomSommet[sourceName];
            int destinationIndex = nomSommet[destinationName];

            if (adjacencyMatrix.GetValue(sourceIndex, destinationIndex) == 0) throw new ArgumentException($"Il n'existe pas d'arcc allant de {sourceName} à {destinationName}.");

            return adjacencyMatrix.GetValue(sourceIndex, destinationIndex);
        }

        /* Affecte le poids l'arc allant du sommet nommé `sourceName` au sommet nommé `destinationName` à `weight` 
         * Si le graphe n'est pas orienté, affecte le même poids à l'arc inverse
         * Lève une ArgumentException si un des sommets n'a pas été trouvé dans le graphe (source et/ou destination)
         */
        public void SetEdgeWeight(string sourceName, string destinationName, float weight)
        {
            // TODO : implémenter
            if (!nomSommet.ContainsKey(sourceName)) throw new ArgumentException($"Le sommet de nom {sourceName} n'existe pas dans le graphe."); 
            if (!nomSommet.ContainsKey(destinationName)) throw new ArgumentException($"Le sommet de nom {destinationName} n'existe pas dans le graphe.");

            if (!directed)
            {
                adjacencyMatrix.SetValue(nomSommet[sourceName], nomSommet[destinationName], weight);
                adjacencyMatrix.SetValue(nomSommet[destinationName], nomSommet[sourceName], weight);
            }
            else
            {
                adjacencyMatrix.SetValue(nomSommet[sourceName], nomSommet[destinationName], weight);
            }
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }


}
