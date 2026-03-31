namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce
    public class Tour
    {
        public Graph graph;
        public List<(string source, string destination)> segments; // liste des trajets de la tournée
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        public Tour(Graph graph, List<(string source, string destination)> segments)
        {
            this.graph = graph;
            this.segments = segments;
        }
        // propriétés


        // Coût total de la tournée
        public float Cost
        {
            get
            {
                float cost = 0;

                foreach ((string source, string destination) in segments)
                {
                    cost += graph.GetEdgeWeight(source, destination);
                }

                return cost;
            }
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get
            {
                return segments.Count;
            }
        }


        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            bool contains = false;

            foreach ((string source, string destination) in segments)
            {
                if (source == segment.source && destination == segment.destination)
                {
                    contains = true;
                    break;
                }
            }

            return contains;   // TODO : implémenter 
        }


        // Affiche les informations sur la tournée : coût total et trajets
        public void Print()
        {
            // TODO : implémenter 
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 
        public void AddSegment((string source, string destination) segment)
        {
            segments.Add(segment);
        }
    }
}
