using System.Collections.Generic;
using System.Linq;

namespace MigrateIt
{
    public class MigrationsService
    {
        private readonly IVersionDetector versionDetector;
        private readonly List<IMigration> migrations = new List<IMigration>();

        public MigrationsService()
        {
        }
        public MigrationsService(IVersionDetector versionDetector)
        {
            this.versionDetector = versionDetector;
        }

        public List<IMigration> Migrations
        {
            get { return migrations.OrderBy(x => x.Version).ToList(); }
        }

        public void Register(IMigration migration)
        {
            migrations.Add(migration);
        }

        public void Migrate(string fromVersion, string toVersion)
        {
            var filteredMigrations = FilterMigrationsToRange(fromVersion, toVersion);

            foreach (var migration in filteredMigrations)
                migration.Apply();
        }

        private List<IMigration> FilterMigrationsToRange(string fromVersion, string toVersion)
        {
            var migrations = FilterOutPreviousMigrations(fromVersion);
            migrations = FilterOutLaterMigrations(toVersion, migrations);
            return migrations;
        }

        private List<IMigration> FilterOutPreviousMigrations(string fromVersion)
        {
            var filteredMigrations = Migrations;
            var fromIndex = filteredMigrations.FindLastIndex(migration => migration.Version == fromVersion) + 1;
            if (fromIndex != 0)
                filteredMigrations = filteredMigrations.Skip(fromIndex).ToList();

            return filteredMigrations;
        }
        private List<IMigration> FilterOutLaterMigrations(string toVersion, List<IMigration> filteredMigrations)
        {
            var toIndex = filteredMigrations.FindLastIndex(migration => migration.Version == toVersion);
            if (toIndex != -1)
                filteredMigrations = filteredMigrations.Take(toIndex).ToList();

            return filteredMigrations;
        }

        public void Migrate(string toVersion)
        {
            Migrate(versionDetector.GetVersion(), toVersion);
        }
    }



    public interface IMigration
    {
        string Version { get; }
        void Apply();
    }
}
