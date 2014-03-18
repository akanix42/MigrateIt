using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace MigrateIt.Tests
{
    class Migrations_Tests
    {
        [Test]
        public void Should_Add_Migration()
        {
            var migration = new Migration();
            migration.Version = "0.0";

            var migrationsService = new MigrationsService();
            migrationsService.Register(migration);

            migrationsService.Migrations.Contains(migration).Should().BeTrue("because we just added this migration");
        }

        [Test]
        public void Should_List_Migrations_In_Order_By_Version()
        {
            var sourceMigrations = new List<Migration>()
            {
                new Migration {Version = "0.0"},
                new Migration { Version = "0.0.1" },
                new Migration { Version = "0.1" },
                new Migration { Version = "0.2" },
                new Migration { Version = "0.2.1" },
            };

            var migrationsService = new MigrationsService();

            migrationsService.Register(sourceMigrations[3]);
            migrationsService.Register(sourceMigrations[0]);
            migrationsService.Register(sourceMigrations[4]);
            migrationsService.Register(sourceMigrations[1]);
            migrationsService.Register(sourceMigrations[2]);

            var migrations = migrationsService.Migrations;
            for (var i = 0; i < migrations.Count; i++)
                migrations[i].Should().Be(sourceMigrations[i]);

        }

        public class Migration : IMigration
        {
            public string Version { get; set; }
            public void Apply()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Should_Apply_All_Migrations_Up_To_Given_Version()
        {

            var sourceMigrations = new List<IMigration>()
            {
                Mock.Of<IMigration>(m => m.Version == "0.0"),
                Mock.Of<IMigration>(m => m.Version == "0.0.1"),
                Mock.Of<IMigration>(m => m.Version == "0.1"),
            };

            var migrationsService = new MigrationsService();
            foreach (var migration in sourceMigrations)
                migrationsService.Register(migration);

            migrationsService.Migrate("", "0.1");

            int index = 0;
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply());
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply());
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply(), Times.Never);
        }

        [Test]
        public void Should_Skip_Migrations_Prior_To_Source_Version()
        {

            var sourceMigrations = new List<IMigration>()
            {
                Mock.Of<IMigration>(m => m.Version == "0.0"),
                Mock.Of<IMigration>(m => m.Version == "0.0.1"),
            };

            var migrationsService = new MigrationsService();
            foreach (var migration in sourceMigrations)
                migrationsService.Register(migration);

            migrationsService.Migrate("0.0", "0.1");

            int index = 0;
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply(), Times.Never);
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply());
        }

        [Test]
        public void Should_Skip_Migrations_Prior_To_Detected_Version()
        {

            var sourceMigrations = new List<IMigration>()
            {
                Mock.Of<IMigration>(m => m.Version == "0.0"),
                Mock.Of<IMigration>(m => m.Version == "0.0.1"),
            };
            var versionDetector = Mock.Of<IVersionDetector>(v => v.GetVersion() == "0.0");
            var migrationsService = new MigrationsService(versionDetector);
            foreach (var migration in sourceMigrations)
                migrationsService.Register(migration);

            migrationsService.Migrate("0.1");

            int index = 0;
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply(), Times.Never);
            Mock.Get(sourceMigrations.Skip(index++).First()).Verify(mockmigration => mockmigration.Apply());
        }
    }

}
