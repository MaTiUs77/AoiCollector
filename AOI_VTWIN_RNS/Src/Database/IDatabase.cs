using System.Data;

namespace CollectorPackage.Src.Database
{
    interface IDatabase
    {
        string host { get; set; }
        string port { get; set; }
        string user { get; set; }
        string pass { get; set; }

        void LoadConfig(string AppConfigTag);
        void Connect();
        void Disconnect();
        DataTable Query(string query);
    }
}
