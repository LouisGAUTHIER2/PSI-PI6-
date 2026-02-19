namespace TourneeFutee
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TODO : faire vos propres tests ici
            Matrix m = new Matrix(nbRows: 2, nbColumns: 3, defaultValue: 0);
            int nbRows = m.NbRows;
            int nbColumns = m.NbColumns;

            m.AddRow(0);
            Console.WriteLine($"NbRows = {m.NbRows} (should be 3)");
               
        }
    }
}
