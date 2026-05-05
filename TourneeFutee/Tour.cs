namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce
    public class Tour
    {
        // propriétés
        public Graph graph;
        public List<(string source, string destination)> segments;
        private int ID;
        private static int nextID = 0;

        // Coût fixe (utilisé quand le graphe n'est pas disponible)
        private float? _fixedCost;

        // Séquence ordonnée de sommets (utilisée par les nouveaux tests)
        private List<string> _vertices;

        // ── Constructeur original (utilisé par Little) ──────────────────────
        public Tour(Graph graph, List<(string source, string destination)> segments)
        {
            this.graph = graph;
            this.segments = segments;
            this.ID = nextID++;
        }

        // ── Constructeur de copie ────────────────────────────────────────────
        public Tour(Tour tour)
        {
            graph = tour.graph;
            segments = new List<(string source, string destination)>(tour.segments);
            _fixedCost = tour._fixedCost;
            _vertices = tour._vertices != null
                ? new List<string>(tour._vertices)
                : null;
            this.ID = nextID++;
        }

        // ── Nouveau constructeur (utilisé par les tests de persistance) ──────
        // Prend une séquence ordonnée de sommets et un coût total fixe.
        // Exemple : ["A","C","F","B","E","D","A"], 20f
        public Tour(List<string> vertices, float cost)
        {
            this.graph = null;
            this._fixedCost = cost;
            this._vertices = new List<string>(vertices);
            this.ID = nextID++;

            // Reconstruire les segments depuis la séquence
            this.segments = new List<(string source, string destination)>();
            for (int i = 0; i < vertices.Count - 1; i++)
                segments.Add((vertices[i], vertices[i + 1]));
        }

        // ── Propriété Cost ───────────────────────────────────────────────────
        // Si un coût fixe a été fourni, on le retourne directement.
        // Sinon on le calcule depuis le graphe.
        public float Cost
        {
            get
            {
                if (_fixedCost.HasValue) return _fixedCost.Value;

                float cost = 0;
                foreach ((string source, string destination) in segments)
                    cost += graph.GetEdgeWeight(source, destination);
                return cost;
            }
        }

        // ── Propriété Vertices ───────────────────────────────────────────────
        // Retourne la séquence ordonnée de sommets (incluant le retour au départ).
        // Utilisée par les nouveaux tests de persistance.
        public IList<string> Vertices
        {
            get
            {
                if (_vertices != null) return _vertices;

                // Reconstruire depuis les segments si pas disponible
                var list = new List<string>();
                foreach (var (source, _) in segments)
                    list.Add(source);
                if (segments.Count > 0)
                    list.Add(segments[segments.Count - 1].destination);
                return list;
            }
        }

        // ── Propriétés existantes ────────────────────────────────────────────
        public int NbSegments => segments.Count;

        public List<(string source, string destination)> Segments => segments;

        public bool ContainsSegment((string source, string destination) segment)
        {
            foreach ((string source, string destination) in segments)
            {
                if (source == segment.source && destination == segment.destination)
                    return true;
            }
            return false;
        }

        public void AddSegment((string source, string destination) segment)
        {
            segments.Add(segment);
        }

        public void Print()
        {
            Console.WriteLine($"Coût total : {Cost}");
            foreach (var (source, destination) in segments)
                Console.WriteLine($"  {source} -> {destination}");
        }
    }
}
