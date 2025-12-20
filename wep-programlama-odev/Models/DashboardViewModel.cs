namespace wep_programlama_odev.Models
{
    public class DashboardViewModel
    {
        // Proje istatistikleri
        public int ProjeBaslanmadi { get; set; }
        public int ProjeBeklemede { get; set; }
        public int ProjeDevamEdiyor { get; set; }
        public int ProjeTamamlandi { get; set; }
        public int ProjeToplam { get; set; }

        // Görev istatistikleri
        public int GorevBeklemede { get; set; }
        public int GorevDevamEdiyor { get; set; }
        public int GorevTamamlandi { get; set; }
        public int GorevToplam { get; set; }
    }
}
