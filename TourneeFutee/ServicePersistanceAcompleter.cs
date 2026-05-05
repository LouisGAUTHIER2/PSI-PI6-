using System;
using MySql.Data.MySqlClient;

namespace TourneeFutee
{
    /// <summary>
    /// Service de persistance permettant de sauvegarder et charger
    /// des graphes et des tournées dans une base de données MySQL.
    /// </summary>
    public class ServicePersistance
    {
        // ─────────────────────────────────────────────────────────────────────
        // Attributs privés
        // ─────────────────────────────────────────────────────────────────────

        private readonly string _connectionString;
        private readonly MySqlConnection _connection;

        // TODO : si vous avez besoin de maintenir une connexion ouverte,
        //        ajoutez un attribut MySqlConnection ici.

        // ─────────────────────────────────────────────────────────────────────
        // Constructeur
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Instancie un service de persistance et se connecte automatiquement
        /// à la base de données <paramref name="dbname"/> sur le serveur
        /// à l'adresse IP <paramref name="serverIp"/>.
        /// Les identifiants sont définis par <paramref name="user"/> (utilisateur)
        /// et <paramref name="pwd"/> (mot de passe).
        /// </summary>
        /// <param name="serverIp">Adresse IP du serveur MySQL.</param>
        /// <param name="dbname">Nom de la base de données.</param>
        /// <param name="user">Nom d'utilisateur.</param>
        /// <param name="pwd">Mot de passe.</param>
        /// <exception cref="Exception">Levée si la connexion échoue.</exception>
        public ServicePersistance(string serverIp, string dbname, string user, string pwd)
        {
          // TODO : initialiser et ouvrir la connexion à la base de données
        // Exemple :
            _connectionString = $"server={serverIp};database={dbname};uid={user};pwd={pwd};";

            _connection = new MySqlConnection(_connectionString);

            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }

            try
            {
                _connection.Open();
                Console.WriteLine("Connexion à la base de données réussie.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion à la base de données : {ex.Message}");
                throw;  // Relancer l'exception pour signaler l'échec de la connexion
            }

            _connection.Close();
        }

        public List<string[]> getData(string command, MySqlConnection conn) {
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = command;
            
            MySqlDataReader reader = cmd.ExecuteReader();

            List<string[]> valueString = new List<string[]>();

            while (reader.Read())
            {
                string[] line = new string[reader.FieldCount];

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    line[i] = reader.GetValue(i).ToString();
                }
                valueString.Add(line);
            }

            conn.Close();

            return valueString;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes publiques
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sauvegarde le graphe <paramref name="g"/> en base de données
        /// (sommets et arcs inclus) et renvoie son identifiant.
        /// </summary>
        /// <param name="g">Le graphe à sauvegarder.</param>
        /// <returns>Identifiant du graphe en base de données (AUTO_INCREMENT).</returns>
        public uint SaveGraph(Graph g)
        {
            // TODO : implémenter la sauvegarde du graphe
            //
            // Ordre recommandé :
            //   1. INSERT dans la table Graphe -> récupérer l'id avec LAST_INSERT_ID()
            //   2. Pour chaque sommet de g : INSERT dans Sommet (valeur + graphe_id)
            //      -> conserver la correspondance sommet C# <-> id BdD
            //   3. Pour chaque arc de la matrice d'adjacence (poids != +inf) :
            //      INSERT dans Arc (sommet_source_id, sommet_dest_id, poids, graphe_id)
            //
            // Exemple pour récupérer l'id généré :
            //   uint id = Convert.ToUInt32(cmd.ExecuteScalar());

            var conn = OpenConnection();
            string sql_com = "INSERT INTO Graphe (est_oriente,nombre_sommets) VALUES (@estOriente,@nombreSommets); SELECT LAST_INSERT_ID();";
            var cmd = new MySqlCommand(sql_com, conn);
            cmd.Parameters.AddWithValue("@estOriente", g.Directed);
            cmd.Parameters.AddWithValue("@nombreSommets", g.Order);
            uint id = Convert.ToUInt32(cmd.ExecuteScalar());

            uint[] sommetIdMap = new uint[g.Order];
            for (int i = 0; i < g.Order; i++)
            {
                string sql_com_sommet = "INSERT INTO Sommet (graphe_id, nom, valeur) VALUES (@grapheId, @nom, @valeur);";
                var cmdSommet = new MySqlCommand(sql_com_sommet, conn);
                cmdSommet.Parameters.AddWithValue("@grapheId", id);
                cmdSommet.Parameters.AddWithValue("@valeur", g.GetVertexValue(i));
                cmdSommet.Parameters.AddWithValue("@nom", g.GetVertexName(i));
                uint sommetId = Convert.ToUInt32(cmdSommet.ExecuteScalar());
                sommetIdMap[i] = sommetId; // Conserver la correspondance indice C# <-> id BdD
            }

            for (int i = 0; i < g.Order; i++)
            {
                for (int j = 0; j < g.Order; j++)
                {
                    try
                    {
                        float poids = g.GetEdgeWeight(i, j);

                        if (poids == 0) continue; // Ignorer les poids nuls ou les boucles
                        Console.WriteLine($"Inserting arc from {g.GetVertexName(i)} to {g.GetVertexName(j)} with weight {poids}");
                        Console.WriteLine($"Corresponding sommet IDs: {sommetIdMap[i]} -> {sommetIdMap[j]}");
                        string sql_com_arc = "INSERT INTO Arc (graphe_id, sommet_source, sommet_dest, poids) VALUES (@grapheId,@sommetSource, @sommetDest, @poids);";
                        var cmdArc = new MySqlCommand(sql_com_arc, conn);
                        cmdArc.Parameters.AddWithValue("@grapheId", id);
                        cmdArc.Parameters.AddWithValue("@sommetSource", sommetIdMap[i]);
                        cmdArc.Parameters.AddWithValue("@sommetDest", sommetIdMap[j]);
                        cmdArc.Parameters.AddWithValue("@poids", poids);
                        cmdArc.ExecuteNonQuery();
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); };
                }
            }

            conn.Close();

            return id;
        }

        /// <summary>
        /// Charge depuis la base de données le graphe identifié par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Graph"/>.
        /// </summary>
        /// <param name="id">Identifiant du graphe à charger.</param>
        /// <returns>Instance de <see cref="Graph"/> reconstituée.</returns>
        public Graph LoadGraph(uint id)
        {
            // TODO : implémenter le chargement du graphe
            //
            // Ordre recommandé :
            //   1. SELECT dans Graphe WHERE id = @id -> récupérer IsOriented, etc.
            //   2. SELECT dans Sommet WHERE graphe_id = @id -> reconstruire les sommets
            //      (respecter l'ordre d'insertion pour que les indices de la matrice
            //       correspondent à ceux sauvegardés)
            //   3. SELECT dans Arc WHERE graphe_id = @id -> reconstruire la matrice
            //      d'adjacence en utilisant les correspondances sommet_id <-> indice

            var conn = OpenConnection();
            string sql_com = "SELECT est_oriente, nombre_sommets FROM Graphe WHERE id = @id;";
            MySqlCommand cmd = new MySqlCommand(sql_com, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = cmd.ExecuteReader();

            int nbSommets = 0;

            reader.Read();
            Console.WriteLine(reader[0] + " -- " + reader[1]);
            Graph graph = new Graph((bool)reader[0]);
            nbSommets = (int)reader[1];

            reader.Close();

            string sql_com_sommet = "SELECT nom, valeur FROM Sommet WHERE graphe_id = @id ORDER BY id;";
            MySqlCommand cmdSommet = new MySqlCommand(sql_com_sommet, conn);
            cmdSommet.Parameters.AddWithValue("@id", id);
            MySqlDataReader readerSommet = cmdSommet.ExecuteReader();
            while (readerSommet.Read())
            {
                string nom = readerSommet.GetString(0);
                float valeur = readerSommet.GetFloat(1);
                graph.AddVertex(nom, valeur);
            }
            readerSommet.Close();

            string sql_com_arc = "SELECT sommet_source, sommet_dest, poids FROM Arc WHERE graphe_id = @id;";
            MySqlCommand cmdArc = new MySqlCommand(sql_com_arc, conn);
            cmdArc.Parameters.AddWithValue("@id", id);
            MySqlDataReader readerArc = cmdArc.ExecuteReader();
            while (readerArc.Read())
            {
                string source = readerArc.GetString(0);
                string dest = readerArc.GetString(1);
                float poids = readerArc.GetFloat(2);
                graph.SetEdgeWeight(source, dest, poids);
            }

            conn.Close();
            return graph; // Remplacer par l'instance de Graph reconstituée
        }

        /// <summary>
        /// Sauvegarde la tournée <paramref name="t"/> (effectuée dans le graphe
        /// identifié par <paramref name="graphId"/>) en base de données
        /// et renvoie son identifiant.
        /// </summary>
        /// <param name="graphId">Identifiant BdD du graphe dans lequel la tournée a été calculée.</param>
        /// <param name="t">La tournée à sauvegarder.</param>
        /// <returns>Identifiant de la tournée en base de données (AUTO_INCREMENT).</returns>
        public uint SaveTour(uint graphId, Tour t)
        {
            // TODO : implémenter la sauvegarde de la tournée
            //
            // Ordre recommandé :
            //   1. INSERT dans Tournee (cout_total, graphe_id) -> récupérer l'id
            //   2. Pour chaque sommet de la séquence (avec son numéro d'ordre) :
            //      INSERT dans EtapeTournee (tournee_id, numero_ordre, sommet_id)
            //
            // Attention : conserver l'ordre des étapes est essentiel pour
            //             pouvoir reconstruire la tournée fidèlement au chargement.

            throw new NotImplementedException("SaveTour non implémenté.");
        }

        /// <summary>
        /// Charge depuis la base de données la tournée identifiée par <paramref name="id"/>
        /// et renvoie une instance de la classe <see cref="Tour"/>.
        /// </summary>
        /// <param name="id">Identifiant de la tournée à charger.</param>
        /// <returns>Instance de <see cref="Tour"/> reconstituée.</returns>
        public Tour LoadTour(uint id)
        {
            // TODO : implémenter le chargement de la tournée
            //
            // Ordre recommandé :
            //   1. SELECT dans Tournee WHERE id = @id -> récupérer cout_total et graphe_id
            //   2. SELECT dans EtapeTournee JOIN Sommet WHERE tournee_id = @id
            //      ORDER BY numero_ordre -> reconstruire la séquence ordonnée de sommets
            //   3. Construire et retourner l'instance Tour

            throw new NotImplementedException("LoadTour non implémenté.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Méthodes utilitaires privées (à compléter selon vos besoins)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée et retourne une nouvelle connexion MySQL ouverte.
        /// Encadrez toujours l'appel dans un bloc using pour garantir la fermeture.
        /// </summary>
        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private MySqlConnection CloseConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Close();
            return conn;
        }
    }
}
