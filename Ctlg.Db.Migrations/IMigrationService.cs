namespace Ctlg.Db.Migrations
{
    public interface IMigrationService
    {
        string LoadMigration(int dbVersion);
    }
}
