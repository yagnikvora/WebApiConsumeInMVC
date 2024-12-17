namespace WebApiConsume.Models
{
    public class TalukaModel
    {
        public int TalukaID { get; set; }
        public string TalukaName { get; set; }
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }
        public int StateID { get; set; }
        public string StateName { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }
    }

    public class TalukaDropDownModel
    {
        public int TalukaID { get; set; }
        public string TalukaName { get; set; }
    }
}
