using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    public class ServicePersistance
    {
        private readonly string _connectionString;

        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";
            try
            {
                using (var conn = OpenConnection()) { }
                Console.WriteLine("Connexion à la base de données réussie.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion : {ex.Message}");
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────

        public uint SaveGraph(Graph g)
        {
            using (var conn = OpenConnection())
            {
                // 1. Insérer le graphe
                string sqlGraphe = @"
                    INSERT INTO Graphe (est_oriente, nombre_sommets)
                    VALUES (@estOriente, @nombreSommets);
                    SELECT LAST_INSERT_ID();";

                var cmdGraphe = new MySqlCommand(sqlGraphe, conn);
                cmdGraphe.Parameters.AddWithValue("@estOriente", g.Directed ? 1 : 0);
                cmdGraphe.Parameters.AddWithValue("@nombreSommets", g.Order);
                uint grapheId = Convert.ToUInt32(cmdGraphe.ExecuteScalar());

                // 2. Insérer les sommets.
                //    On itère sur les noms via g.Vertices et on utilise GetVertexId(nom)
                //    (fiable car c'est un lookup direct dans le dictionnaire)
                //    pour construire la correspondance indice_matrice -> id_BDD.
                uint[] indiceToBddId = new uint[g.Order];

                foreach (string nom in g.Vertices)
                {
                    int idxMatrice = g.GetVertexId(nom);
                    float valeur = g.GetVertexValue(nom);

                    string sqlSommet = @"
                        INSERT INTO Sommet (graphe_id, nom, valeur)
                        VALUES (@grapheId, @nom, @valeur);
                        SELECT LAST_INSERT_ID();";

                    var cmdSommet = new MySqlCommand(sqlSommet, conn);
                    cmdSommet.Parameters.AddWithValue("@grapheId", grapheId);
                    cmdSommet.Parameters.AddWithValue("@nom", nom);
                    cmdSommet.Parameters.AddWithValue("@valeur", valeur);

                    indiceToBddId[idxMatrice] = Convert.ToUInt32(cmdSommet.ExecuteScalar());
                }

                // 3. Insérer les arcs directement depuis la matrice d'adjacence.
                //    indiceToBddId[i] donne le bon id BDD pour l'indice matriciel i.
                for (int i = 0; i < g.Order; i++)
                {
                    for (int j = 0; j < g.Order; j++)
                    {
                        if (i == j) continue;
                        if (!g.Directed && i > j) continue;

                        float poids = g.AdjacencyMatrix.GetValue(i, j);
                        if (poids == 0f || float.IsInfinity(poids)) continue;

                        string sqlArc = @"
                            INSERT INTO Arc (graphe_id, sommet_source, sommet_dest, poids)
                            VALUES (@grapheId, @source, @dest, @poids);";

                        var cmdArc = new MySqlCommand(sqlArc, conn);
                        cmdArc.Parameters.AddWithValue("@grapheId", grapheId);
                        cmdArc.Parameters.AddWithValue("@source", indiceToBddId[i]);
                        cmdArc.Parameters.AddWithValue("@dest", indiceToBddId[j]);
                        cmdArc.Parameters.AddWithValue("@poids", poids);
                        cmdArc.ExecuteNonQuery();
                    }
                }

                return grapheId;
            }
        }

        public Graph LoadGraph(uint id)
        {
            using (var conn = OpenConnection())
            {
                // 1. Charger les métadonnées du graphe
                string sqlGraphe = "SELECT est_oriente FROM Graphe WHERE id = @id;";
                var cmdGraphe = new MySqlCommand(sqlGraphe, conn);
                cmdGraphe.Parameters.AddWithValue("@id", id);

                bool estOriente;
                using (var reader = cmdGraphe.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new ArgumentException($"Aucun graphe trouvé avec l'id {id}.");
                    estOriente = reader.GetBoolean(0);
                }

                Graph graph = new Graph(estOriente);

                // 2. Charger les sommets dans l'ordre d'insertion (ORDER BY id)
                var sommetIdToNom = new Dictionary<uint, string>();

                string sqlSommets = @"
                    SELECT id, nom, valeur
                    FROM Sommet
                    WHERE graphe_id = @id
                    ORDER BY id;";

                var cmdSommets = new MySqlCommand(sqlSommets, conn);
                cmdSommets.Parameters.AddWithValue("@id", id);

                using (var reader = cmdSommets.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uint sommetId = reader.GetUInt32(0);
                        string nom = reader.GetString(1);
                        float valeur = reader.IsDBNull(2) ? 0f : reader.GetFloat(2);

                        graph.AddVertex(nom, valeur);
                        sommetIdToNom[sommetId] = nom;
                    }
                }

                // 3. Charger les arcs (sommet_source/dest sont des FK entiers)
                string sqlArcs = @"
                    SELECT sommet_source, sommet_dest, poids
                    FROM Arc
                    WHERE graphe_id = @id;";

                var cmdArcs = new MySqlCommand(sqlArcs, conn);
                cmdArcs.Parameters.AddWithValue("@id", id);

                using (var reader = cmdArcs.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uint sourceId = reader.GetUInt32(0);
                        uint destId = reader.GetUInt32(1);
                        float poids = reader.GetFloat(2);

                        string sourceName = sommetIdToNom[sourceId];
                        string destName = sommetIdToNom[destId];

                        try { graph.AddEdge(sourceName, destName, poids); }
                        catch (ArgumentException) { }
                    }
                }

                return graph;
            }
        }

        public uint SaveTour(uint graphId, Tour t)
        {
            using (var conn = OpenConnection())
            {
                // 1. Insérer la tournée avec son coût
                string sqlTournee = @"
                    INSERT INTO Tournee (graphe_id, cout_total)
                    VALUES (@grapheId, @coutTotal);
                    SELECT LAST_INSERT_ID();";

                var cmdTournee = new MySqlCommand(sqlTournee, conn);
                cmdTournee.Parameters.AddWithValue("@grapheId", graphId);
                cmdTournee.Parameters.AddWithValue("@coutTotal", t.Cost);
                uint tourneeId = Convert.ToUInt32(cmdTournee.ExecuteScalar());

                // 2. Récupérer la correspondance nom -> sommet_id BDD
                var nomToSommetId = new Dictionary<string, uint>();

                string sqlSommets = @"
                    SELECT id, nom FROM Sommet
                    WHERE graphe_id = @grapheId ORDER BY id;";

                var cmdSommets = new MySqlCommand(sqlSommets, conn);
                cmdSommets.Parameters.AddWithValue("@grapheId", graphId);

                using (var reader = cmdSommets.ExecuteReader())
                {
                    while (reader.Read())
                        nomToSommetId[reader.GetString(1)] = reader.GetUInt32(0);
                }

                // 3. Insérer les étapes depuis Vertices (séquence ordonnée)
                //    On utilise Vertices qui inclut le sommet de retour (ex: A..A)
                //    On sauvegarde tous les sommets de la séquence.
                IList<string> vertices = t.Vertices;

                for (int ordre = 0; ordre < vertices.Count; ordre++)
                {
                    string nom = vertices[ordre];

                    if (!nomToSommetId.ContainsKey(nom))
                        throw new ArgumentException(
                            $"Sommet '{nom}' introuvable dans le graphe {graphId} en BDD.");

                    string sqlEtape = @"
                        INSERT INTO EtapeTournee (tournee_id, numero_ordre, sommet_id)
                        VALUES (@tourneeId, @ordre, @sommetId);";

                    var cmdEtape = new MySqlCommand(sqlEtape, conn);
                    cmdEtape.Parameters.AddWithValue("@tourneeId", tourneeId);
                    cmdEtape.Parameters.AddWithValue("@ordre", (uint)ordre);
                    cmdEtape.Parameters.AddWithValue("@sommetId", nomToSommetId[nom]);
                    cmdEtape.ExecuteNonQuery();
                }

                return tourneeId;
            }
        }

        public Tour LoadTour(uint id)
        {
            using (var conn = OpenConnection())
            {
                // 1. Charger le coût total et le graphe_id
                string sqlTournee = "SELECT graphe_id, cout_total FROM Tournee WHERE id = @id;";
                var cmdTournee = new MySqlCommand(sqlTournee, conn);
                cmdTournee.Parameters.AddWithValue("@id", id);

                uint grapheId;
                float coutTotal;
                using (var reader = cmdTournee.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new ArgumentException($"Aucune tournée trouvée avec l'id {id}.");
                    grapheId = reader.GetUInt32(0);
                    coutTotal = reader.GetFloat(1);
                }

                // 2. Charger les étapes dans l'ordre -> séquence de noms
                string sqlEtapes = @"
                    SELECT s.nom
                    FROM EtapeTournee et
                    JOIN Sommet s ON et.sommet_id = s.id
                    WHERE et.tournee_id = @tourneeId
                    ORDER BY et.numero_ordre;";

                var cmdEtapes = new MySqlCommand(sqlEtapes, conn);
                cmdEtapes.Parameters.AddWithValue("@tourneeId", id);

                var vertices = new List<string>();
                using (var reader = cmdEtapes.ExecuteReader())
                {
                    while (reader.Read())
                        vertices.Add(reader.GetString(0));
                }

                // 3. Reconstruire la tournée avec le constructeur (List<string>, float)
                //    La séquence inclut déjà le sommet de retour (ex: A C F B E D A)
                return new Tour(vertices, coutTotal);
            }
        }

        // ─────────────────────────────────────────────────────────────────────

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
